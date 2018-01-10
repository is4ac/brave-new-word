using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class GameManagerScript : MonoBehaviour {

	CamShakeSimpleScript camShake;
	private const int NUM_OF_PATHS = 2;
	public enum Versions { SwipeUI, ButtonUI, ButtonTimeUI };
	public static Versions currentVersion;
	GameObject playButton;

	// Use this for initialization
	void Start () {
		BoxScript.camShake = gameObject.AddComponent<CamShakeSimpleScript> ();
		playButton = GameObject.FindGameObjectWithTag ("PlayButton");

		try {
			string line;
			string firstLine = null;
			int[] versions = null;
			int versionIndex = -1;

			// Read the config file and determine what the order of UI versions will be
			using (StreamReader file = new StreamReader(Application.persistentDataPath + "/config.config")) {
				if ((line = file.ReadLine()) != null) {
					firstLine = line;
					string[] tokens = line.Split(',');
					versions = new int[tokens.Length];

					for (int i = 0; i < tokens.Length; ++i) {
						if (!int.TryParse(tokens[i], out versions[i])) {
							// some sort of error handling message debug log here
						}
					}
				}

				if ((line = file.ReadLine()) != null) {
					if (!int.TryParse(line, out versionIndex)) {
						// some sort of error handling message debug log here
					}
				}

				if (versionIndex < versions.Length) {
					if (Enum.IsDefined(typeof(Versions), versions[versionIndex])) {
						currentVersion = (Versions) versions[versionIndex];
					}
				} else {
					// reset the index back to the beginning
					versionIndex = 0;

					if (Enum.IsDefined(typeof(Versions), versions[versionIndex])) {
						currentVersion = (Versions) versions[versionIndex];
					}
				}

				Debug.Log(currentVersion);

				file.Close();
			}

			// advance version to next version in config file
			FileInfo f = new FileInfo (Application.persistentDataPath + "/config.config");
			StreamWriter writer = f.CreateText ();
			writer.WriteLine (firstLine);
			writer.WriteLine (versionIndex + 1);
			writer.Close ();

		} catch (FileNotFoundException ex) {
			// choose a random path for the player by writing to the file

			FileInfo file = new FileInfo (Application.persistentDataPath + "/config.config");
			StreamWriter writer = file.CreateText ();

			int i = UnityEngine.Random.Range (0, NUM_OF_PATHS);

			if (i == 0) {
				writer.WriteLine ("0,1,0");
			} else {
				writer.WriteLine ("1,0,1");
			}

			writer.WriteLine ("0");
			writer.Close ();
		}

		// check version and hide/show Play Word button depending on version
		if (currentVersion == Versions.SwipeUI) {
			playButton.SetActive (false);
		} else {
			playButton.SetActive (true);
		}
	}
	
	// Update is called once per frame
	void Update () {
		// if the enter key is pressed, then submit the word
		// check against dictionary and give it points
		if (currentVersion == Versions.ButtonUI && Input.GetKeyDown (KeyCode.Return)) {
			BoxScript.PlayWord ();
		}
	}

	// Play word!
	public void PlayWord() {
		BoxScript.PlayWord ();
	}
}
