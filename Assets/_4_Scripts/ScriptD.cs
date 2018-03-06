using OpenCVForUnity;
using UnityEngine;
using Vuforia;
using System.IO;
using System.Collections.Generic;

public class ScriptD : MonoBehaviour {

    public Camera cam;


	private GameObject template;
	private GameObject skull;

	Texture2D unwarpedTexture;
	Texture2D unwarpedTextureClean;

	// intrinsics
    public float fx = 650;
    public float fy = 650;
    public float cx = 320;
    public float cy = 240;

	public Matrix4x4 originalProjection;

	public int width = 640; 
	public int height = 480;

    private MatOfPoint2f imagePoints;
    private Mat camImageMat;
	private Mat grayscaleMat;
	private Mat thresholdMat;

	private Mat skullTextureMat;

	private Mat matrixA;
	private Mat matrixB;
	private Mat matrixH;

	private Point corner1;
	private Point corner2;
	private Point corner3;
	private Point corner4;

    void Start () {
		skull = GameObject.Find ("Skull");
		template = GameObject.Find ("ImageTarget");

		imagePoints = new MatOfPoint2f();
		imagePoints.alloc(4);

		skullTextureMat = MatDisplay.LoadRGBATexture("_Templates/assignment2b/flying_skull_tex.png");
		unwarpedTextureClean = new Texture2D (width, height, TextureFormat.RGBA32, false);

		matrixA = new Mat(8, 8,CvType.CV_64FC1);
		matrixB = new Mat(8, 1, CvType.CV_64FC1);
		matrixH = new Mat(8, 1, CvType.CV_64FC1);
    }

    void Update () {

        //Access camera image provided by Vuforia
        Image camImg = CameraDevice.Instance.GetCameraImage(Image.PIXEL_FORMAT.RGBA8888);

		if (camImg != null) {
			if (camImageMat == null) {
				//First time -> instantiate camera image specific data
				camImageMat = new Mat (camImg.Height, camImg.Width, CvType.CV_8UC4);  //Note: rows=height, cols=width
				grayscaleMat = new Mat (camImg.Height, camImg.Width, CvType.CV_8UC4);
				thresholdMat = new Mat (camImg.Height, camImg.Width, CvType.CV_8UC4);
			}
			camImageMat.put (0, 0, camImg.Pixels);

			//Grayscale
			Imgproc.cvtColor (camImageMat, grayscaleMat, Imgproc.COLOR_BGR2GRAY);

			// Blur image
			Imgproc.GaussianBlur (grayscaleMat, grayscaleMat, new Size (5, 5), 0);

			// Threshold
			Imgproc.threshold (grayscaleMat, thresholdMat, 60, 255, Imgproc.THRESH_BINARY);

			// Canny edge detector
			Mat cannyMat = new Mat (camImg.Height, camImg.Width, CvType.CV_8UC4);
			Imgproc.Canny (thresholdMat, cannyMat, 100, 200);

			//Find contours
			List<MatOfPoint> contours = new List<MatOfPoint> ();
			Imgproc.findContours (cannyMat, contours, new Mat (), Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);

			// Find biggest square contour
			double maxContourArea = 0;
			MatOfPoint2f finalContour = new MatOfPoint2f ();
			for (int i = 0; i < contours.Count; i++) {
				MatOfPoint currentContour = contours [i];
				double contourArea = Imgproc.contourArea (currentContour);

				MatOfPoint2f curve = new MatOfPoint2f (currentContour.toArray ());
				MatOfPoint2f approxCurve = new MatOfPoint2f ();
				Imgproc.approxPolyDP (curve, approxCurve, Imgproc.arcLength (curve, true) * 0.03, true);
				// Is square?
				if (approxCurve.total () == 4) {
					// Is biggest?
					if (contourArea > maxContourArea) {
						maxContourArea = contourArea;
						finalContour = approxCurve;
					}
				}
			}

			// Update corners only when the square is detected
			if (maxContourArea > 10000) {
				Point[] points = finalContour.toArray ();
				corner1 = points [0];
				corner2 = points [1];
				corner3 = points [2];
				corner4 = points [3];
			}

			// If corners have been defined
			if(corner1!=null){
				imagePoints.put (0, 0, corner1.x, corner1.y);
				imagePoints.put (1, 0, corner2.x, corner2.y);
				imagePoints.put (2, 0, corner3.x, corner3.y);
				imagePoints.put (3, 0, corner4.x, corner4.y);

				Imgproc.circle (thresholdMat, corner1, 15, new Scalar (255, 0, 0), -1);
				Imgproc.circle (thresholdMat, corner2, 15, new Scalar (255, 0, 0), -1);
				Imgproc.circle (thresholdMat, corner3, 15, new Scalar (255, 0, 0), -1);
				Imgproc.circle (thresholdMat, corner4, 15, new Scalar (255, 0, 0), -1);


//			 Destination points for unwarped camera image
				var destPoints = new MatOfPoint2f (); 
				destPoints.alloc (4); 
				destPoints.put (0, 0, width, 0);
				destPoints.put (1, 0, width, height);
				destPoints.put (2, 0, 0, height);
				destPoints.put (3, 0, 0, 0);

				// Destination points for skull texture
				var skullDestPoints = new MatOfPoint2f ();
				skullDestPoints.alloc (4); 
				skullDestPoints.put (0, 0, 1024, 0);
				skullDestPoints.put (1, 0, 1024, 1024);
				skullDestPoints.put (2, 0, 0, 1024);
				skullDestPoints.put (3, 0, 0, 0);

				// Homography from camera image to destPoints
//				Mat homography = Calib3d.findHomography (imagePoints, destPoints); 
				Mat homography = findHomographyCustom (imagePoints, destPoints); 
				// Warp perspective from camera image to dest points given homography
				Imgproc.warpPerspective (camImageMat, destPoints, homography, new Size (camImageMat.width (), camImageMat.height ()));

				// Homography from skull texture to camera image
//				Mat homography2 = Calib3d.findHomography (skullDestPoints, imagePoints);
				Mat homography2 = findHomographyCustom (skullDestPoints, imagePoints);

				// Destination for unwarped skull texture
				Mat warpedTexMat = new Mat ();
				// Warp perspective from skull texture to warped texture mat given homography2
				Imgproc.warpPerspective (skullTextureMat, warpedTexMat, homography2, new Size (camImageMat.width (), camImageMat.height ()));

				// Mat for main camera image
				Mat mainImageMat = new Mat ();
				// Put warped texture on the camera image and put both into mainImageMat
				Core.addWeighted (camImageMat, 0.95f, warpedTexMat, 0.4f, 0.0, mainImageMat);

				if (Input.GetKey ("space")) {
					MatDisplay.MatToTexture (destPoints, ref unwarpedTexture); 
					skull.GetComponent<Renderer> ().material.mainTexture = unwarpedTexture; 
				} else {
					Texture2D tex = unwarpedTextureClean;
					MatDisplay.MatToTexture(skullTextureMat, ref tex);
					skull.GetComponent<Renderer>().material.mainTexture = tex; 
				}
		
				MatDisplay.DisplayMat (destPoints, MatDisplaySettings.BOTTOM_LEFT);
				MatDisplay.DisplayMat (mainImageMat, MatDisplaySettings.FULL_BACKGROUND);
//			MatDisplay.DisplayMat(contoursMat, MatDisplaySettings.TOP_RIGHT);
				MatDisplay.DisplayMat (thresholdMat, MatDisplaySettings.BOTTOM_RIGHT);
			} else {
				MatDisplay.DisplayMat (camImageMat, MatDisplaySettings.FULL_BACKGROUND);
			}

		}
    }


	private Mat findHomographyCustom(MatOfPoint2f imagePoints, MatOfPoint2f destPoints){
		var u1 = destPoints.get(0, 0)[0];
		var v1 = destPoints.get(0, 0)[1];
		var u2 = destPoints.get(1, 0)[0];
		var v2 = destPoints.get(1, 0)[1];
		var u3 = destPoints.get(2, 0)[0];
		var v3 = destPoints.get(2, 0)[1];
		var u4 = destPoints.get(3, 0)[0];
		var v4 = destPoints.get(3, 0)[1];

		var x1 = imagePoints.get(0, 0)[0];
		var y1 = imagePoints.get(0, 0)[1];
		var x2 = imagePoints.get(1, 0)[0];
		var y2 = imagePoints.get(1, 0)[1];
		var x3 = imagePoints.get(2, 0)[0];
		var y3 = imagePoints.get(2, 0)[1];
		var x4 = imagePoints.get(3, 0)[0];
		var y4 = imagePoints.get(3, 0)[1];


		// First pair
		matrixA.put(0, 0, x1);
		matrixA.put(0, 1, y1);
		matrixA.put(0, 2, 1);
		matrixA.put(0, 6, -u1*x1);
		matrixA.put(0, 7, -u1*y1);

		matrixA.put(1, 3, x1);
		matrixA.put(1, 4, y1);
		matrixA.put(1, 5, 1);
		matrixA.put(1, 6, -v1 * x1);
		matrixA.put(1, 7, -v1 * y1);

		// Second pair
		matrixA.put(2, 0, x2);
		matrixA.put(2, 1, y2);
		matrixA.put(2, 2, 1);
		matrixA.put(2, 6, -u2 * x2);
		matrixA.put(2, 7, -u2 * y2);

		matrixA.put(3, 3, x2);
		matrixA.put(3, 4, y2);
		matrixA.put(3, 5, 1);
		matrixA.put(3, 6, -v2 * x2);
		matrixA.put(3, 7, -v2 * y2);

		// Third pair
		matrixA.put(4, 0, x3);
		matrixA.put(4, 1, y3);
		matrixA.put(4, 2, 1);
		matrixA.put(4, 6, -u3 * x3);
		matrixA.put(4, 7, -u3 * y3);

		matrixA.put(5, 3, x3);
		matrixA.put(5, 4, y3);
		matrixA.put(5, 5, 1);
		matrixA.put(5, 6, -v3 * x3);
		matrixA.put(5, 7, -v3 * y3);

		// Forth pair
		matrixA.put(6, 0, x4);
		matrixA.put(6, 1, y4);
		matrixA.put(6, 2, 1);
		matrixA.put(6, 6, -u4 * x4);
		matrixA.put(6, 7, -u4 * y4);

		matrixA.put(7, 3, x4);
		matrixA.put(7, 4, y4);
		matrixA.put(7, 5, 1);
		matrixA.put(7, 6, -v4 * x4);
		matrixA.put(7, 7, -v4 * y4);

		Mat matrixB = new Mat(8, 1, CvType.CV_64FC1);

		// Initialize the b vector 
		matrixB.put(0, 0, u1);
		matrixB.put(1, 0, v1);
		matrixB.put(2, 0, u2);
		matrixB.put(3, 0, v2);
		matrixB.put(4, 0, u3);
		matrixB.put(5, 0, v3);
		matrixB.put(6, 0, u4);
		matrixB.put(7, 0, v4);

		//			var homography = findHomographyCustom(imagePoints, destPoints); // Finding the image

		//			destPoints = homography * imagePoins;
		//			(u, v) = H * (x, y)
		//			A * v = b
		Core.solve(matrixA, matrixB, matrixH);
		Mat homography = new Mat(3, 3, CvType.CV_64FC1);

		// Reallocate values to a 3x3 matrix
		homography.put(0, 0, matrixH.get(0, 0));
		homography.put(0, 1, matrixH.get(1, 0));
		homography.put(0, 2, matrixH.get(2, 0));
		homography.put(1, 0, matrixH.get(3, 0));
		homography.put(1, 1, matrixH.get(4, 0));
		homography.put(1, 2, matrixH.get(5, 0));
		homography.put(2, 0, matrixH.get(6, 0));
		homography.put(2, 1, matrixH.get(7, 0));
		homography.put(2, 2, 1); // Normalize
		return homography;
	}
}
