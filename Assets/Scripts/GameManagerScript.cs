using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour {

	CamShakeSimpleScript camShake;

	// Use this for initialization
	void Start () {
		camShake = gameObject.AddComponent<CamShakeSimpleScript> ();
	}
	
	// Update is called once per frame
	void Update () {
		// if the enter key is pressed, then submit the word
		// check against dictionary and give it points
		if (Input.GetKeyDown (KeyCode.Return)) {
			bool valid = BoxScript.updateScore ();

			if (valid) {
				// do something celebratory! like sparkles?
			} else {
				camShake.ShakeRed (1f);
			}
		}
	}
}
