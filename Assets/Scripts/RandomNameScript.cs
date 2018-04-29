using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class RandomNameScript : MonoBehaviour
{

	public TextAsset p12Asset;
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
		RandomizeName ();

		// Firebase database logistics for editor
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl ("https://wordflood-bf7c4.firebaseio.com/");
		FirebaseApp.DefaultInstance.SetEditorP12FileName (@"Assets/Firebase/WordFlood-66029aead4c6.p12.txt");
		FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail ("wordflood-unity-android@wordflood-bf7c4.iam.gserviceaccount.com");
		FirebaseApp.DefaultInstance.SetEditorP12Password ("notasecret");


		FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
			Firebase.DependencyStatus dependencyStatus = task.Result;
			if (dependencyStatus == DependencyStatus.Available) {
				InitializeFirebase();
			} else {
				Debug.LogError(
					"Could not resolve all Firebase dependencies: " + dependencyStatus);
			}
		});

	}

	// Initialize the Firebase database:
	protected virtual void InitializeFirebase() {
		FirebaseApp app = FirebaseApp.DefaultInstance;
		// NOTE: You'll need to replace this url with your Firebase App's database
		// path in order for the database connection to work correctly in editor

		//This is needed only for the unity editor
		//app.SetEditorDatabaseUrl("https://wordflood-bf7c4.firebaseio.com/");
		if (app.Options.DatabaseUrl != null) {
			//app.SetEditorDatabaseUrl (app.Options.DatabaseUrl);
		}
		//--------------------------------------
		/*
		Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
		auth.SignInWithEmailAndPasswordAsync("isaacsung@gmail.com", "notasecret").ContinueWith(task => {
			if (task.IsCanceled) {
				Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
				return;
			}
			if (task.IsFaulted) {
				Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
				return;
			}
			Firebase.Auth.FirebaseUser newUser = task.Result;
			Debug.LogFormat("User signed in successfully: {0} ({1})",
				newUser.DisplayName, newUser.UserId);
		});
		*/
	}

	// Update is called once per frame
	void Update ()
	{
		
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
