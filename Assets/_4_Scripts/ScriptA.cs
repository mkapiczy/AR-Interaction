using OpenCVForUnity;
using UnityEngine;
using Vuforia;
using System.IO;

public class ScriptA : MonoBehaviour {

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


    void Start () {
		fish = GameObject.Find("Fish");
		corner1 = GameObject.Find ("Corner1");
		corner2 = GameObject.Find ("Corner2");
		corner3 = GameObject.Find ("Corner3");
		corner4 = GameObject.Find ("Corner4");

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

			var homography = Calib3d.findHomography(imagePoints, destPoints); // Finding the image

			Imgproc.warpPerspective(camImageMat, destPoints, homography, new Size(camImageMat.width(), camImageMat.height()));

			unwarpedTexture = unwarpedTextureClean;

			MatDisplay.MatToTexture(destPoints, ref unwarpedTexture); // Take output and transform into texture

			if (Input.GetKey("space")){
				fish.GetComponent<Renderer>().material.mainTexture = unwarpedTexture; 
			} else {
				fish.GetComponent<Renderer>().material.mainTexture = tex; 
			}

			MatDisplay.DisplayMat(destPoints, MatDisplaySettings.BOTTOM_LEFT);
			MatDisplay.DisplayMat(camImageMat, MatDisplaySettings.FULL_BACKGROUND);
        }
    }
}
