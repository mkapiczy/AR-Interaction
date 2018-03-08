using OpenCVForUnity;
using UnityEngine;
using Vuforia;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class ScriptD : MonoBehaviour, IVirtualButtonEventHandler {

	private GameObject A;
	private GameObject B;
	private GameObject C;
	private GameObject D;
	private GameObject E;
	private GameObject F;
	private GameObject G;
	private GameObject H;
	private GameObject I;
	private GameObject J;
	private GameObject K;
	private GameObject L;
	private GameObject M;
	private GameObject N;
	private GameObject O;
	private GameObject U;
	private GameObject P;
	private GameObject R;
	private GameObject S;
	private GameObject T;
	private GameObject Ó;
	private GameObject V;
	private GameObject W;
	private GameObject Y;
	private GameObject Z;
	private GameObject X;
	private GameObject SPACE;
	private GameObject DELETE;


	private string text;
	private GameObject note;

	private bool enableWriting = true;

    void Start () {
		A = GameObject.Find ("A");
		B = GameObject.Find ("B");
		C = GameObject.Find ("C");
		D = GameObject.Find ("D");
		E = GameObject.Find ("E");
		F = GameObject.Find ("F");
		G = GameObject.Find ("G");
		H = GameObject.Find ("H");
		I = GameObject.Find ("I");
		J = GameObject.Find ("J");
		K = GameObject.Find ("K");
		L = GameObject.Find ("L");
		M = GameObject.Find ("M");
		N = GameObject.Find ("N");
		O = GameObject.Find ("O");
		U = GameObject.Find ("U");
		P = GameObject.Find ("P");
		R = GameObject.Find ("R");
		S = GameObject.Find ("S");
		T = GameObject.Find ("T");
		U = GameObject.Find ("U");
		V = GameObject.Find ("V");
		W = GameObject.Find ("W");
		Y = GameObject.Find ("Y");
		Z = GameObject.Find ("Z");
		X = GameObject.Find ("X");
		SPACE = GameObject.Find ("SPACE");
		DELETE = GameObject.Find ("DELETE");



		note = GameObject.Find("Text");


		A.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		B.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		C.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		D.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		E.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		F.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		G.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		H.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		I.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		J.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		K.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		L.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		M.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		N.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		O.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		U.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		P.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		R.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		S.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		T.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		U.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		V.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		W.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		Y.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		Z.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		X.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		SPACE.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);
		DELETE.GetComponent<VirtualButtonBehaviour> ().RegisterEventHandler (this);


    }

    void Update () {

		if (enableWriting == false)
		{
			ExecuteAfterTime (60);
		}
    }

	IEnumerator ExecuteAfterTime(float time)
	{
		yield return new WaitForSeconds(time/1000);

		enableWriting = true;
	}

	public void OnButtonPressed(VirtualButtonBehaviour vb) {
		Debug.Log ("Button pressed " + vb.VirtualButtonName);
		if (enableWriting) {
			enableWriting = false;
			switch (vb.VirtualButtonName) {
			case "A":
				addText ("A");
				break;
			case "B":
				addText ("B");
				break;
			case "C":
				addText ("C");
				break;
			case "D":
				addText ("D");
				break;
			case "E":
				addText ("E");
				break;
			case "F":
				addText ("F");
				break;
			case "G":
				addText ("G");
				break;
			case "H":
				addText ("H");
				break;
			case "I":
				addText ("I");
				break;
			case "J":
				addText ("J");
				break;
			case "K":
				addText ("K");
				break;
			case "L":
				addText ("L");
				break;
			case "M":
				addText ("M");
				break;
			case "N":
				addText ("N");
				break;
			case "O":
				addText ("O");
				break;
			case "P":
				addText ("P");
				break;
			case "Q":
				addText ("Q");
				break;
			case "R":
				addText ("R");
				break;
			case "S":
				addText ("S");
				break;
			case "T":
				addText ("T");
				break;
			case "U":
				addText ("U");
				break;
			case "V":
				addText ("V");
				break;
			case "W":
				addText ("W");
				break;
			case "X":
				addText ("X");
				break;
			case "Y":
				addText ("Y");
				break;
			case "Z":
				addText ("Z");
				break;
			case "SPACE":
				addText (" ");
				break;
			case "DELETE":
				removeChar ();
				break;
			}
		}
	}

	public IEnumerable<WaitForSeconds> Waaait(float s)
	{
		yield return new WaitForSeconds(s);
	}

	public void OnButtonReleased(VirtualButtonBehaviour vb)
	{
		//vbButtonObject.GetComponent<AudioSource>().Stop();
	}

	private void addText(string input){
		text = text + input;
		note.GetComponent<TextMesh> ().text = text;
		Debug.Log (text);
	}

	private void removeChar()
	{
		if (text.Length > 0)
		{
			text = text.Remove(text.Length - 1);
			note.GetComponent<TextMesh>().text = text;
			Debug.Log(text);
		}
	}
}
