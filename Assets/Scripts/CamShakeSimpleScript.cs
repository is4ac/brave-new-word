using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CamShakeSimpleScript : MonoBehaviour {

	Vector3 originalCameraPosition;
	Color originalColor;
	float shakeAmt = 0;
	float shakeFactor = 0.1f;
	public float decreaseFactor = 2.5f;

	public Camera mainCamera;

	void Start() {
		mainCamera = Camera.main;
		originalCameraPosition = mainCamera.transform.position;
		originalColor = GameObject.FindGameObjectWithTag ("Walls").GetComponent<Image> ().color;
	}

	public void Shake(float amount) {
		shakeAmt = amount;
	}

	// Shake the camera and make the walls red!
	public void ShakeRed(float amount) {
		shakeAmt = amount;
		ColorWalls (Color.red);
	}

	void ColorWalls(Color changeColor) {
		GameObject[] wallObjs = GameObject.FindGameObjectsWithTag ("Walls");

		foreach (GameObject wall in wallObjs) {
			wall.GetComponent<Image> ().color = changeColor;
		}
	}

	void Update() {
		if(shakeAmt>0) 
		{
			mainCamera.transform.localPosition = (Random.insideUnitSphere * shakeAmt * shakeFactor) + mainCamera.transform.position;

			// Reduce the amount of shaking for next tick.
			shakeAmt -= Time.deltaTime * decreaseFactor;

			if (shakeAmt <= 0.0f) {
				shakeAmt = 0.0f;
				mainCamera.transform.position = originalCameraPosition;
				ColorWalls (originalColor);
			}
		}
	}
}
