﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class TextFaderScript : MonoBehaviour {

	public static Color normalColor = new Color32( 0x16, 0xE8, 0xE3, 0xFF );
	public static Color errorColor = Color.black;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void FadeText(float t, string text) {
		Text myText = GetComponentInChildren<Text> ();
		Image myPanel = GetComponent<Image> (); 
		myPanel.color = normalColor; 
		myText.fontSize = 58;
		StartCoroutine (FadeTextToFullAlpha(t, text));
	}

	public void FadeErrorText(float t, string text) {
		Text myText = GetComponentInChildren<Text> ();
		Image myPanel = GetComponent<Image> (); 
		myPanel.color = errorColor;
		myText.fontSize = 30;
		StartCoroutine (FadeTextToFullAlpha (t, text));
	}

	public IEnumerator FadeTextToFullAlpha(float t, string text)
	{
		Text myText = GetComponentInChildren<Text> ();
		Image myPanel = GetComponent<Image> (); 
		myText.text = text;

		myText.color = new Color(myText.color.r, myText.color.g, myText.color.b, 0);
		myPanel.color = new Color(myPanel.color.r, myPanel.color.g, myPanel.color.b, 0);
		while (myText.color.a < 1.0f)
		{
			myText.color = new Color(myText.color.r, myText.color.g, myText.color.b, myText.color.a + (Time.deltaTime / t));
			myPanel.color = new Color(myPanel.color.r, myPanel.color.g, myPanel.color.b, myPanel.color.a + (Time.deltaTime / t) * 0.9f);
			yield return null;
		}

		yield return new WaitForSeconds(0.4f);

		StartCoroutine (FadeTextToZeroAlpha (t, text));
	}

	public IEnumerator FadeTextToZeroAlpha(float t, string text)
	{
		Text myText = GetComponentInChildren<Text> ();
		Image myPanel = GetComponent<Image> (); 
		myText.text = text;

		while (myText.color.a > 0.0f)
		{
			myText.color = new Color(myText.color.r, myText.color.g, myText.color.b, myText.color.a - (Time.deltaTime / t));
			myPanel.color = new Color(myPanel.color.r, myPanel.color.g, myPanel.color.b, myPanel.color.a - (Time.deltaTime / t) * 0.9f);
			yield return null;
		}
	}
}
