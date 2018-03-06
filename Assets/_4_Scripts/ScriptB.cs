using OpenCVForUnity;
using UnityEngine;
using Vuforia;
using System.IO;

public class ScriptB : MonoBehaviour {

    public Camera cam;

    private GameObject corner1;
    private GameObject corner2;
    private GameObject corner3;
    private GameObject corner4;

	private GameObject fish;

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
	private Texture2D tex;
    private byte[] texData;
	private byte[] fileData;

	private Mat matrixA;
	private Mat matrixB;
	private Mat matrixH;
//	private Mat homography;

    void Start () {
		fish = GameObject.Find("Fish");
		corner1 = GameObject.Find ("Corner1");
		corner2 = GameObject.Find ("Corner2");
		corner3 = GameObject.Find ("Corner3");
		corner4 = GameObject.Find ("Corner4");

		matrixA = new Mat(8, 8,CvType.CV_64FC1);
		matrixB = new Mat(8, 1, CvType.CV_64FC1);
		matrixH = new Mat(8, 1, CvType.CV_64FC1);

		imagePoints = new MatOfPoint2f();
		imagePoints.alloc(4);
		tex = new Texture2D (2, 2);
		fileData = File.ReadAllBytes ("Assets/_Templates/assignment2b/fish_tex.png");
		tex.LoadImage(fileData);
		unwarpedTextureClean = new Texture2D (width, height, TextureFormat.RGBA32, false);
    }

    void Update () {

		//Access camera image provided by Vuforia
		Image camImg = CameraDevice.Instance.GetCameraImage(Image.PIXEL_FORMAT.RGBA8888);

		if(camImg!=null) {
			if (camImageMat == null) {
				//First time -> instantiate camera image specific data
				camImageMat = new Mat(camImg.Height, camImg.Width, CvType.CV_8UC4);  //Note: rows=height, cols=width
			}

			camImageMat.put(0,0, camImg.Pixels);

			//Replace with your own projection matrix. This approach only uses fy.
			cam.fieldOfView = 2 * Mathf.Atan(camImg.Height * 0.5f / fy) * Mathf.Rad2Deg;

			Vector3 worldPnt1 = corner1.transform.position;
			Vector3 worldPnt2 = corner2.transform.position;
			Vector3 worldPnt3 = corner3.transform.position;
			Vector3 worldPnt4 = corner4.transform.position;

			//See lecture slides
			Matrix4x4 Rt = cam.transform.worldToLocalMatrix;
			Matrix4x4 A = Matrix4x4.identity;
			A.m00 = fx;
			A.m11 = fy;
			A.m02 = cx;
			A.m12 = cy;

			Matrix4x4 worldToImage = A * Rt;

			Vector3 hUV1 = worldToImage.MultiplyPoint3x4(worldPnt1);
			Vector3 hUV2 = worldToImage.MultiplyPoint3x4(worldPnt2);
			Vector3 hUV3 = worldToImage.MultiplyPoint3x4(worldPnt3);
			Vector3 hUV4 = worldToImage.MultiplyPoint3x4(worldPnt4);

			//hUV are the image coordinates in 2D homogeneous coordinates, we need to normalize, i.e., divide by Z
			Vector2 uv1 = new Vector2(hUV1.x, hUV1.y) / hUV1.z;
			Vector2 uv2 = new Vector2(hUV2.x, hUV2.y) / hUV2.z;
			Vector2 uv3 = new Vector2(hUV3.x, hUV3.y) / hUV3.z;
			Vector2 uv4 = new Vector2(hUV4.x, hUV4.y) / hUV4.z;

			//don't forget to alloc before putting values into a MatOfPoint2f
			imagePoints.put(0, 0, uv1.x, camImg.Height - uv1.y);
			imagePoints.put(1, 0, uv2.x, camImg.Height - uv2.y);
			imagePoints.put(2, 0, uv3.x, camImg.Height - uv3.y);
			imagePoints.put(3, 0, uv4.x, camImg.Height - uv4.y);

			//Debug draw points
			Point imgPnt1 = new Point(imagePoints.get(0, 0));
			Point imgPnt2 = new Point(imagePoints.get(1, 0));
			Point imgPnt3 = new Point(imagePoints.get(2, 0));
			Point imgPnt4 = new Point(imagePoints.get(3, 0));
			Imgproc.circle(camImageMat, imgPnt1, 5, new Scalar(255, 0, 0, 255));
			Imgproc.circle(camImageMat, imgPnt2, 5, new Scalar(0, 255, 0, 255));
			Imgproc.circle(camImageMat, imgPnt3, 5, new Scalar(0, 0, 255, 255));
			Imgproc.circle(camImageMat, imgPnt4, 5, new Scalar(255, 255, 0, 255));
			Scalar lineCl = new Scalar(200, 120, 0, 160);
			Imgproc.line(camImageMat, imgPnt1, imgPnt2, lineCl);
			Imgproc.line(camImageMat, imgPnt2, imgPnt3, lineCl);
			Imgproc.line(camImageMat, imgPnt3, imgPnt4, lineCl);
			Imgproc.line(camImageMat, imgPnt4, imgPnt1, lineCl);


			var destPoints = new MatOfPoint2f(); // Creating a destination
			destPoints.alloc(4); 
			destPoints.put(0, 0, width, 0);
			destPoints.put(1, 0, width, height);
			destPoints.put(2, 0, 0, height);
			destPoints.put(3, 0, 0, 0);

			Mat homography = findHomographyCustom (imagePoints, destPoints);

			Mat destImageMat = new Mat ();
			Imgproc.warpPerspective(camImageMat, destImageMat, homography, new Size(camImageMat.width(), camImageMat.height()));

			unwarpedTexture = unwarpedTextureClean;

			MatDisplay.MatToTexture(destImageMat, ref unwarpedTexture); // Take output and transform into texture

			if (Input.GetKey("space")){
				fish.GetComponent<Renderer>().material.mainTexture = unwarpedTexture; 
			} else {
				fish.GetComponent<Renderer>().material.mainTexture = tex; 
			}

			MatDisplay.DisplayMat(destImageMat, MatDisplaySettings.BOTTOM_LEFT);
			MatDisplay.DisplayMat(camImageMat, MatDisplaySettings.FULL_BACKGROUND);
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
