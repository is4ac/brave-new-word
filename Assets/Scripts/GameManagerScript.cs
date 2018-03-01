using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class GameManagerScript : MonoBehaviour {

	// Set to true to log to Firebase database, false to turn off
	public const bool LOGGING = true;
	public const string LOGGING_VERSION = "WFLogs_V1_0_1";
	public const string APP_VERSION = "WF_1.0.1";

	CamShakeSimpleScript camShake;
	//private const int NUM_OF_PATHS = 6;
	public enum Versions { SwipeUI, ButtonUI, ButtonTimeUI };
	/*
	private static Versions[][] allPaths = { 
		new Versions[] { Versions.SwipeUI, Versions.ButtonUI, Versions.SwipeUI },
		new Versions[] { Versions.ButtonUI, Versions.SwipeUI, Versions.ButtonUI },
		new Versions[] { Versions.SwipeUI, Versions.SwipeUI, Versions.SwipeUI },
		new Versions[] { Versions.ButtonUI, Versions.ButtonUI, Versions.ButtonUI },
		new Versions[] { Versions.ButtonUI, Versions.ButtonUI, Versions.SwipeUI },
		new Versions[] { Versions.SwipeUI, Versions.SwipeUI, Versions.ButtonUI } 
	};
	private static Versions[] currentPath;
	private static int versionIndex;
	*/
	public static Versions currentVersion;
	GameObject playButton;
	GameObject nextButton;
	Text usernameText;
	//Text timerText;
	public static int GAME_ID;
	public static string username;
	public static int userID;
	public static string deviceModel;
	static bool areBoxesFalling = true;
	//public static float timer = 10; // 5 minutes in seconds
	//public static bool timerEnded = false;

	// Use this for initialization
	void Start () {
		deviceModel = SystemInfo.deviceModel;

		// Using time since epoch date as unique game ID
		System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
		GAME_ID = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
		Debug.Log ("Game id: " + GAME_ID);

		BoxScript.camShake = gameObject.AddComponent<CamShakeSimpleScript> ();
		playButton = GameObject.Find ("PlayButton");
		nextButton = GameObject.Find ("NextStageButton");
		if (nextButton != null) {
			nextButton.SetActive (false);
		}
		username = PlayerPrefs.GetString ("username");

		// randomize userID
		userID = Random.Range (0, int.MaxValue);

		// Log beginning of game
		if (LOGGING) {
			MetaLogEntry entry = new MetaLogEntry ();
			entry.setValues ("WF_GameStart", "WF_Meta", new MetaLogEntry.MetaPayload("Start"));
			string json = JsonUtility.ToJson (entry);
			DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference (LOGGING_VERSION);
			reference.Push ().SetRawJsonValueAsync (json);

			//StartCoroutine(WaitUntilGameInitAndLog ()); // log keyframe after the board is set
		}

		/*
		timerText = GameObject.Find ("TimerText").GetComponent<Text> ();
		DisplayTime ();
		*/

		// display the username on the screen
		usernameText = GameObject.Find("UsernameText").GetComponent<Text>();
		usernameText.text = username;

		/*
		// check to see if a path has already been saved in PlayerPrefsX
		if (PlayerPrefs.HasKey ("currentPath")) {
			currentPath = (Versions[])(object)PlayerPrefsX.GetIntArray ("currentPath");
			versionIndex = PlayerPrefs.GetInt ("versionIndex");
			if (versionIndex >= currentPath.Length) {
				versionIndex = 0;
			}
		} else {
			// Randomly decide on the version path this game will take
			int i = UnityEngine.Random.Range (0, NUM_OF_PATHS);
			currentPath = allPaths[i];
			versionIndex = 0;

			PlayerPrefsX.SetIntArray ("currentPath", (int[])(object)currentPath);
		}

		Debug.Log ("index: " + versionIndex);
		currentVersion = currentPath[versionIndex++];
		if (versionIndex >= currentPath.Length) {
			versionIndex = 0;
		}

		PlayerPrefs.SetInt ("versionIndex", versionIndex);
		*/

		currentVersion = (Versions) Random.Range (0, 2);

		// check version and hide/show Play Word button depending on version
		if (currentVersion == Versions.SwipeUI) {
			playButton.SetActive (false);
		} else {
			playButton.SetActive (true);
		}

		Debug.Log ("Version: " + currentVersion);

		// insert user and game data into users table in firebase
		if (LOGGING) {
			Debug.Log ("logging user??");

			// insert new user entry into database
			DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference ("users");
			DatabaseReference child = reference.Child (username);
			child.Child ("gameType").SetValueAsync ((int)currentVersion);
			child.Child ("gameID").SetValueAsync (GAME_ID);
			child.Child ("userID").SetValueAsync (userID);
		}
	}
	
	// Update is called once per frame
	void Update () {
		// if the enter key is pressed, then submit the word
		// check against dictionary and give it points
		if (currentVersion == Versions.ButtonUI && Input.GetKeyDown (KeyCode.Return)) {
			BoxScript.PlayWord ();
		}

		if (Input.GetKeyDown(KeyCode.Escape)) { 
			SceneManager.LoadScene(0);
		}

		// Log the keyframe (game state) after all the boxes have stopped falling
		if (areBoxesFalling) {
			if (!checkIfBoxesAreFalling ()) {
				areBoxesFalling = false;
				LogKeyFrame ("post");
			}
		} else {
			if (checkIfBoxesAreFalling ()) {
				areBoxesFalling = true;
			}
		}

		// check to see if the game is over (aka, MAX_SCORE is reached)
		if (BoxScript.score >= BoxScript.MAX_SCORE) {
			// TODO: game over
		}

		/*
		// timer start
		if (SpawnBoxScript.isInitialized() && !timerEnded) {
			timer -= Time.deltaTime;

			if (timer <= 0) {
				timer = 0;
				timerEnded = true;
			}

			DisplayTime ();
		}

		if (timerEnded) {
			nextButton.SetActive (true);
		}
		*/
	}

	bool checkIfBoxesAreFalling() {
		bool falling = false;

		for (int i = 0; i < BoxScript.gridWidth; ++i) {
			if (BoxScript.IsBoxInColumnFalling (i) || !BoxScript.IsColumnFull(i)) {
				falling = true;
				break;
			}
		}

		return falling;
	}

	void DisplayTime() {
		/*
		int minutes = Mathf.FloorToInt(timer / 60F);
		int seconds = Mathf.FloorToInt(timer - minutes * 60);
		string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
		*/
		//timerText.text = niceTime; // disable for now
	}

	// Play word!
	public void PlayWord() {
		BoxScript.PlayWord ();
	}

	public void CloseInstructionsPanel() {
		GameObject panel = GameObject.Find ("Instructions");
		panel.SetActive (false);
	}

	public void Reset() {
		/*
		timer = 10; // 5 minutes in seconds
		timerEnded = false;
		*/

		GameObject boxes = GameObject.Find ("SpawnBoxes");

		foreach (Transform child in boxes.transform) {
			child.gameObject.GetComponent<SpawnBoxScript> ().Reset ();
		}

		BoxScript.Reset ();

		int scene = SceneManager.GetActiveScene().buildIndex;
		SceneManager.LoadScene(scene); // eventually delete the key "currentPath"
		// PlayerPrefs.DeleteKey("currentPath");
	}

	IEnumerator WaitUntilGameInitAndLog() {
		while (!SpawnBoxScript.isInitialized ()) {
			yield return null;
		}

		LogKeyFrame ("pre");
	}

	public static void LogKeyFrame(string preOrPost) {
		Debug.Log ("Logging full game state");

		// log the current full game state
		KeyFrameLogEntry entry = new KeyFrameLogEntry ();
		KeyFrameLogEntry.KeyFramePayload payload = new KeyFrameLogEntry.KeyFramePayload ();

		payload.board = BoxScript.GetBoardPayload();
		payload.totalScore = BoxScript.score;
		payload.timeElapsed = Time.time;
		payload.totalInteractions = BoxScript.totalInteractions;
		payload.wordsPlayed = BoxScript.wordsPlayed;
		payload.preOrPost = preOrPost;

		entry.setValues ("WF_GameState", "WF_KeyFrame", payload);
		string json = JsonUtility.ToJson (entry);
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference (LOGGING_VERSION);
		reference.Push ().SetRawJsonValueAsync (json);
	}
}
