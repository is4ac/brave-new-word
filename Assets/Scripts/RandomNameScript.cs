using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class RandomNameScript : MonoBehaviour {
	
	public static string username;
	public static Text welcomeText = null;
	private static List<string> attributes = null;
	private static List<string> colors = null;
	private static List<string> animals = null;

	// Use this for initialization
	void Start () {
		if (welcomeText == null) {
			welcomeText = GameObject.Find("WelcomeText").GetComponent<Text>();
		}

		// Generate a random username
		InitializeUsernameList ();
		RandomizeName ();

		// Firebase database logistics
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://wordflood-bf7c4.firebaseio.com/");
		FirebaseApp.DefaultInstance.SetEditorP12FileName("WordFlood-66029aead4c6.p12");
		FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail("wordflood-unity-android@wordflood-bf7c4.iam.gserviceaccount.com");
		FirebaseApp.DefaultInstance.SetEditorP12Password("notasecret");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void InitializeUsernameList() {
		// import username list and put it into corresponding lists
		StreamReader file = new StreamReader(@"Assets/Dictionaries/usernamesList.csv");

		if (attributes == null)
		{
			attributes = new List<string>();

			// read first line of the file
			string line = file.ReadLine();

			if (line != null) {
				string[] tokens = line.Split(',');
				for (int i = 0; i < tokens.Length; ++i) {
					attributes.Add (tokens [i]);
				}
			}
		}

		if (colors == null)
		{
			colors = new List<string>();

			// read second line of the file
			string line = file.ReadLine();

			if (line != null) {
				string[] tokens = line.Split(',');
				for (int i = 0; i < tokens.Length; ++i) {
					colors.Add (tokens [i]);
				}
			}
		}

		if (animals == null)
		{
			animals = new List<string>();

			// read third line of the file
			string line = file.ReadLine();

			if (line != null) {
				string[] tokens = line.Split(',');
				for (int i = 0; i < tokens.Length; ++i) {
					animals.Add (tokens [i]);
				}
			}
		}
	}

	void DisplayUsername() {
		welcomeText.text = "Welcome,\n" + username + "!";
	}

	public void RandomizeName() {
		int i = Random.Range (0, attributes.Count);
		int j = Random.Range (0, colors.Count);
		int k = Random.Range (0, animals.Count);

		username =  attributes[i] + " " + colors[j] + " " + animals[k];
		DisplayUsername ();
	}
}
