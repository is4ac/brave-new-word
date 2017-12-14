using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour {

	CamShakeSimpleScript camShake;

	// Use this for initialization
	void Start () {
		BoxScript.camShake = gameObject.AddComponent<CamShakeSimpleScript> ();
	}
	
	// Update is called once per frame
	void Update () {
		// if the enter key is pressed, then submit the word
		// check against dictionary and give it points
		if (Input.GetKeyDown (KeyCode.Return)) {
			BoxScript.PlayWord ();
		}
	}

	// Play word!
	public void PlayWord() {
		BoxScript.PlayWord ();
	}
}
