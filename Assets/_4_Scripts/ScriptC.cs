using OpenCVForUnity;
using UnityEngine;
using Vuforia;
using System.IO;

public class ScriptC : MonoBehaviour {

	private VideoCapture videoCapture;
	private GameObject faceTargetPlane;

	private Mat camImageMat;
	private Mat frontCameraImgMat;

	private Mat faces;
	private Mat eyes;
	private Mat faceWithMarkings;

	private string faceXML;
	private string eyeXML;

	private OpenCVForUnity.Rect roi;
	private Texture2D unwarpedTexture;


	public Camera cam;



    void Start () {
		faceTargetPlane = GameObject.Find ("FacePlane");
		faceXML = "Assets/_2_Templates/haarcascade_frontalface_alt.xml";
		eyeXML = "Assets/_2_Templates/haarcascade_eye.xml";

		frontCameraImgMat = new Mat ();
		faces = new Mat ();
		eyes = new Mat ();
		faceWithMarkings = new Mat ();

		videoCapture = new VideoCapture ();
		videoCapture.open (1);
    }

    void Update () {
		Image camImg = CameraDevice.Instance.GetCameraImage(Image.PIXEL_FORMAT.RGBA8888);

		if (camImg != null) {
			if (camImageMat == null) {
				camImageMat = new Mat (camImg.Height, camImg.Width, CvType.CV_8UC4);  //Note: rows=height, cols=width
			}
			camImageMat.put(0, 0, camImg.Pixels);

			// Read from videoCap and save in mat
			videoCapture.read (frontCameraImgMat);

			Face.getFacesHAAR (frontCameraImgMat, faces, faceXML);

			Debug.Log("Faces " + faces.height ());
			if (faces.height () > 0) {
				for (var i = 0; i < faces.height (); i++) {
					double[] faceRect = faces.get (i, 0);
					Point faceRectPoint1 = new Point (faceRect [0], faceRect [1]);
					Point faceRectPoint2 = new Point (faceRect [0] + faceRect [2], faceRect [1] + faceRect [3]);
					Imgproc.rectangle (frontCameraImgMat, faceRectPoint1, faceRectPoint2, new Scalar (0, 0, 255), 5);
					roi = new OpenCVForUnity.Rect (faceRectPoint1, faceRectPoint2);
				}

		 		faceWithMarkings = new Mat (frontCameraImgMat, roi);
	
				Face.getFacesHAAR (faceWithMarkings, eyes, eyeXML);

				Debug.Log ("Eyes " + eyes.height ());
				if (eyes.height () != 0) {
					for (var i = 0; i < eyes.height(); i++) {
						if (i < 2) {
							double[] eyeRect = eyes.get (i, 0);
							Point eyeCenter = new Point (eyeRect [2] * 0.5F + eyeRect [0], eyeRect [3] * 0.5F + eyeRect [1]);
							int radius = (int)Mathf.Sqrt (Mathf.Pow (((float)eyeRect [2]) * 0.5F, 2F) + Mathf.Pow (((float)eyeRect [3]) * 0.5F, 2F));
							Imgproc.circle (faceWithMarkings, new Point (eyeCenter.x, eyeCenter.y), radius, new Scalar (255, 0, 0), 5);
						}
					}
				} 

				MatDisplay.MatToTexture (faceWithMarkings, ref unwarpedTexture);
				faceTargetPlane.GetComponent<Renderer> ().material.mainTexture = unwarpedTexture; 
			}
		}
			
		MatDisplay.DisplayMat(camImageMat, MatDisplaySettings.FULL_BACKGROUND);
		MatDisplay.DisplayMat (frontCameraImgMat, MatDisplaySettings.BOTTOM_LEFT);

    }
}
