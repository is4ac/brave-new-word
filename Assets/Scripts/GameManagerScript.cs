using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameManagerScript : MonoBehaviour {

	CamShakeSimpleScript camShake;

	// Use this for initialization
	void Start () {
		BoxScript.camShake = gameObject.AddComponent<CamShakeSimpleScript> ();

		try {
			string line;
			int[] versions = null;
			int currentVersion = -1;

			// Read the config file and determine what the order of UI versions will be
			using (StreamReader file = new StreamReader(Application.persistentDataPath + "/config.config")) {
				if ((line = file.ReadLine()) != null) {
					string[] tokens = line.Split(',');
					versions = new int[tokens.Length];

					for (int i = 0; i < tokens.Length; ++i) {
						if (!int.TryParse(tokens[i], out versions[i])) {
							// some sort of error handling message debug log here
						}
					}
				}

				if ((line = file.ReadLine()) != null) {
					if (!int.TryParse(line, out currentVersion)) {
						// some sort of error handling message debug log here
					}
				}

				Debug.Log(versions[0]);
				Debug.Log(currentVersion);

				file.Close();
			}
		} catch (FileNotFoundException ex) {
			// choose a random path for the player by writing to the file

			FileInfo file = new FileInfo (Application.persistentDataPath + "/config.config");
			StreamWriter writer = file.CreateText ();
			writer.WriteLine ("2,1,2");
			writer.WriteLine ("0");
			writer.Close ();
		}
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
