using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class RandomNameScript : MonoBehaviour
{

	public TextAsset usernamesText;

	public static string username;
	public static Text welcomeText = null;
	private static List<string> attributes = null;
	private static List<string> colors = null;
	private static List<string> animals = null;
	public static Firebase.Auth.FirebaseAuth auth;
	private Firebase.Auth.FirebaseUser newUser;

	// Use this for initialization
	void Start ()
	{
		Screen.orientation = ScreenOrientation.Portrait;

		if (welcomeText == null) {
			welcomeText = GameObject.Find ("WelcomeText").GetComponent<Text> ();
		}

		// Generate a random username
		InitializeUsernameList ();

        // Check for saved file
        if (File.Exists(Application.persistentDataPath + StartGameScript.DATA_PATH))
        {
            // Read the file to load the frictional pattern data
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + StartGameScript.DATA_PATH, FileMode.Open);

            PlayerData data = (PlayerData)bf.Deserialize(file);
            file.Close();

            username = data.username;
            DisplayUsername();
        }
        else
        {
            // If file doesn't exist yet, randomize and initialize variables
            RandomizeName();
        }
	}

	void InitializeUsernameList ()
	{
		// import username list and put it into corresponding lists
		string[] lines = usernamesText.text.Split ('\n');

		if (attributes == null) {
			attributes = new List<string> ();

			// read first line of the file
			string line = lines [0];

			if (line != null) {
				string[] tokens = line.Split (',');
				for (int i = 0; i < tokens.Length; ++i) {
					attributes.Add (tokens [i]);
				}
			}
		}

		if (colors == null) {
			colors = new List<string> ();

			// read second line of the file
			string line = lines [1];

			if (line != null) {
				string[] tokens = line.Split (',');
				for (int i = 0; i < tokens.Length; ++i) {
					colors.Add (tokens [i]);
				}
			}
		}

		if (animals == null) {
			animals = new List<string> ();

			// read third line of the file
			string line = lines [2];

			if (line != null) {
				string[] tokens = line.Split (',');
				for (int i = 0; i < tokens.Length; ++i) {
					animals.Add (tokens [i]);
				}
			}
		}
	}

	void DisplayUsername ()
	{
		welcomeText.text = "Welcome,\n" + username + "!";
	}

	public void RandomizeName ()
	{
		int i = UnityEngine.Random.Range (0, attributes.Count);
		int j = UnityEngine.Random.Range (0, colors.Count);
		int k = UnityEngine.Random.Range (0, animals.Count);

		username = attributes [i].Trim() + " " + colors [j].Trim() + " " + animals [k].Trim();
		DisplayUsername ();
	}
}
