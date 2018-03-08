using OpenCVForUnity;
using UnityEngine;
using Vuforia;
using System.IO;

public class ScriptC : MonoBehaviour {

//    public Camera cam;
//
//	private GameObject plane;
    
//	// intrinsics
//	public float fx = 650;
//	public float fy = 650;
//	public float cx = 320;
//	public float cy = 240;
//
//	public Matrix4x4 originalProjection;
//
//	public int width = 640; 
//	public int height = 480;
//
//    private MatOfPoint2f imagePoints;
//    private Mat camImageMat;
//	private VideoCapture cap(0);
//
//	Mat skullTextureMat;
//
//	Texture2D unwarpedTexture;
//	Texture2D unwarpedTextureClean;

    void Start () {
//		plane = GameObject.Find ("Plane");
//		unwarpedTextureClean = new Texture2D (width, height, TextureFormat.RGBA32, false);

    }

    void Update () {

        //Access camera image provided by Vuforia
//        Image camImg = CameraDevice.Instance.GetCameraImage(Image.PIXEL_FORMAT.RGBA8888);
//		OpenCVForUnity.VideoCapture
//        if(camImg!=null) {
//            if (camImageMat == null) {
//                //First time -> instantiate camera image specific data
//                camImageMat = new Mat(camImg.Height, camImg.Width, CvType.CV_8UC4);  //Note: rows=height, cols=width
//            }
//
//            camImageMat.put(0,0, camImg.Pixels);
//
//            //Replace with your own projection matrix. This approach only uses fy.
//            cam.fieldOfView = 2 * Mathf.Atan(camImg.Height * 0.5f / fy) * Mathf.Rad2Deg;
//
//			unwarpedTexture = unwarpedTextureClean;
//
//			MatDisplay.MatToTexture(camImageMat, ref unwarpedTexture);
//			plane.GetComponent<Renderer>().material.mainTexture = unwarpedTexture; 
//
//			MatDisplay.DisplayMat(camImageMat, MatDisplaySettings.BOTTOM_LEFT);
////			MatDisplay.DisplayMat(mainImageMat, MatDisplaySettings.FULL_BACKGROUND);
//        }
    }
}
