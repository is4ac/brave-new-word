using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class GameManagerScript : MonoBehaviour {

    // singleton instance
    public static GameManagerScript gameManager;

	// Set to true to log to Firebase database, false to turn off
	public static bool LOGGING = true;
	public const string LOGGING_VERSION = "BNWLogs_V1_1_0_debug";
	public const string APP_VERSION = "BNW_1.1.0_debug";
    public static string usersDbName = "users" + DBManager.scoresVersionNumber;

    public DatabaseReference dbUsers;
	CamShakeSimpleScript camShake;

    /*************************************
     * Feature booleans - these keep track of what features are on/off for this current game
     *************************************/
    public static bool DISPLAY_BUTTON;          // users must click on button to submit word
    public static bool DISPLAY_SELECTED_SCORE;  // show currently selected word score
    public static bool DISPLAY_HIGHLIGHT_FEEDBACK;      // feedback during highlighting of words
    public static bool DISPLAY_TUTORIAL;        // OUTDATED // show the tutorial screen with instructions at beginning of game
    /*********************************************/
	
	public GameObject playButton;
    public GameObject instructionsPanel;
	public static string GAME_ID;
	public static string username;
	public static string userID;
	public static string deviceModel;
	static bool areBoxesFalling = true;
	public GameObject gameOverPanel = null;
	static bool gameHasBegun = false;
	static bool initialLog = true; // keeps track of whether or not the initial logging should be done
    public static bool INSTRUCTIONS_PANEL = true;
    public static long myHighScore = 0;
    public static bool myHighScoreUpdated = false;
    public static bool globalHighScoreUpdated = false;

	void Awake() {
        // singleton pattern-esque
        if (gameManager == null) {
            gameManager = this;
            gameOverPanel.SetActive(false);
        } else {
            Destroy(gameObject);
        }
	}

	// Use this for initialization
	void Start () {
		deviceModel = SystemInfo.deviceModel;

        dbUsers = FirebaseDatabase.DefaultInstance.GetReference(usersDbName);

		// Generate a new GAME_ID using the Guid class
        GAME_ID = Guid.NewGuid().ToString();
		Debug.Log ("Game id: " + GAME_ID);

		BoxScript.camShake = gameObject.AddComponent<CamShakeSimpleScript> ();

        username = PlayerPrefs.GetString("username");

        // check version and hide/show Play Word button depending on version
        if (DISPLAY_BUTTON)
        {
            playButton.SetActive(true);
        }
        else
        {
            playButton.SetActive(false);
        }

        // Hide the instructions panel if it shouldn't be displayed
        if (!INSTRUCTIONS_PANEL)
        {
            instructionsPanel.SetActive(false);
        }

        // do various logging for the start of the game
        LogStartOfGame();
	}

    /**
     * Save frictional pattern to device whenever the game quits
     */
    void OnDisable()
    {
        // Open the file to write the data
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + StartGameScript.DATA_PATH);

        PlayerData data = new PlayerData(DISPLAY_BUTTON, 
                                         DISPLAY_SELECTED_SCORE, 
                                         DISPLAY_HIGHLIGHT_FEEDBACK, 
                                         username, 
                                         false,
                                         userID,
                                         myHighScore);
        
        // serialize and write to file
        bf.Serialize(file, data);
        file.Close();
    }

    public static void LogInstructionsClick(Vector2 pos) {
        if (LOGGING)
        {
            // log the location of the click
            ClickLogEntry entry = new ClickLogEntry();
            ClickLogEntry.ClickPayload payload = new ClickLogEntry.ClickPayload
            {
                screenX = pos.x,
                screenY = pos.y
            };

            entry.setValues("WF_InstructionsClosed", "WF_Action", payload);
            string json = JsonUtility.ToJson(entry);
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(LOGGING_VERSION);
            reference.Push().SetRawJsonValueAsync(json);
        }
    }

    public void LogStartOfGame() {
        if (LOGGING)
        {
            Debug.Log("Logging beginning of game");

            // Log beginning of game
            MetaLogEntry entry = new MetaLogEntry();
            entry.setValues("WF_GameStart", "WF_Meta", new MetaLogEntry.MetaPayload("start"));
            string json = JsonUtility.ToJson(entry);
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(LOGGING_VERSION);
            reference.Push().SetRawJsonValueAsync(json);

            Debug.Log("logging game info");

            // insert new game entry into database
            reference = FirebaseDatabase.DefaultInstance.GetReference(LOGGING_VERSION + "_games");
            DatabaseReference child = reference.Child(GAME_ID + "");

            // Log the game details and type of game
            child.Child("displayButton").SetValueAsync(DISPLAY_BUTTON);
            child.Child("displayTutorial").SetValueAsync(DISPLAY_TUTORIAL);
            child.Child("displayHighlightFeedback").SetValueAsync(DISPLAY_HIGHLIGHT_FEEDBACK);
            child.Child("displaySelectedScore").SetValueAsync(DISPLAY_SELECTED_SCORE);
            child.Child("username").SetValueAsync(username);
            child.Child("gameID").SetValueAsync(GAME_ID);
            child.Child("userID").SetValueAsync(userID);
            child.Child("loggingVersion").SetValueAsync(LOGGING_VERSION);
            child.Child("appVersion").SetValueAsync(APP_VERSION);

            // Log username into users database if it doesn't already exist or if the username has changed
            dbUsers.GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted)
                {
                    // ERROR HANDLER
                    Debug.Log("Error in logging user into users database of GameManagerScript");
                }
                else if (task.IsCompleted)
                {
                    Dictionary<string, object> results = (Dictionary<string, object>)task.Result.Value;

                    // set the value of userID's username if it doesn't exist or if it changed
                    if (results == null || !results.ContainsKey(userID) || !results[userID].Equals(username))
                    {
                        dbUsers.Child(userID).SetValueAsync(username);
                    }
                }
            });
        }
    }

    public void SetButtonDisplay(bool value) {
        playButton.SetActive(value);
    }
	
	// Update is called once per frame
	void Update () {
		// if the enter key is pressed, then submit the word
		// check against dictionary and give it points
        if (DISPLAY_BUTTON && Input.GetKeyDown (KeyCode.Return)) {
			PlayWord ();
		}

		if (Input.GetKeyDown(KeyCode.Escape)) { 
			SceneManager.LoadScene(0);
		}

		// Log the keyframe (game state) after all the boxes have stopped falling
		if (areBoxesFalling) {
			if (!checkIfBoxesAreFalling ()) {
				areBoxesFalling = false;

				if (!initialLog) {
					LogKeyFrame ("post");
				} else {
					LogKeyFrame ("gameStart");
					initialLog = false;
					//AnalyzeGameBoard ();
				}
			}
		} else {
			if (checkIfBoxesAreFalling ()) {
				areBoxesFalling = true;
			}
		}
	}

	static bool checkIfBoxesAreFalling() {
		bool falling = false;

		for (int i = 0; i < BoxScript.gridWidth; ++i) {
			if (BoxScript.IsBoxInColumnFalling (i) || !BoxScript.IsColumnFull(i)) {
				falling = true;
				break;
			}
		}

		return falling;
	}

	// Play word!
	public void PlayWord() {
		BoxScript.PlayWord ();
		Debug.Log ("Play word pressed.");

        // display the high score?
        Debug.Log("Top score: " + DBManager.instance.topScore);
	}

	public static void BeginGame() {
        TouchInputHandler.touchEnabled = true;
		gameHasBegun = true;
	}

	public static bool GameHasStarted() {
		return gameHasBegun;
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

        gameOverPanel.SetActive(false);

		int scene = SceneManager.GetActiveScene().buildIndex;
		SceneManager.LoadScene(scene); // eventually delete the key "currentPath"
		// PlayerPrefs.DeleteKey("currentPath");
	}

	public static void LogKeyFrame(string state) {
        if (LOGGING)
        {
            Debug.Log("Logging full game state");

            // log the current full game state
            KeyFrameLogEntry entry = new KeyFrameLogEntry();
            KeyFrameLogEntry.KeyFramePayload payload = new KeyFrameLogEntry.KeyFramePayload();

            payload.board = BoxScript.GetBoardPayload();
            payload.totalScore = BoxScript.score;
            payload.timeElapsed = Time.time;
            payload.totalInteractions = BoxScript.totalInteractions;
            payload.wordsPlayed = BoxScript.wordsPlayed;
            payload.state = state;

            entry.setValues("WF_GameState", "WF_KeyFrame", payload);
            string json = JsonUtility.ToJson(entry);
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(LOGGING_VERSION);
            reference.Push().SetRawJsonValueAsync(json);
        }
	}

	public static void LogEndOfGame() {
        if (LOGGING)
        {
            Debug.Log("Logging end of game");

            // log the end game state when the player finished the round
            LogKeyFrame("gameEnd");

            // log the end of the game
            MetaLogEntry metaEntry = new MetaLogEntry();
            metaEntry.setValues("WF_GameEnd", "WF_Meta", new MetaLogEntry.MetaPayload("end"));
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(LOGGING_VERSION);
            string json = JsonUtility.ToJson(metaEntry);
            reference.Push().SetRawJsonValueAsync(json);
        }
	}

	/**
	 * User or the OS force quits the application
	 */
	public static void LogGamePause() {
        if (LOGGING)
        {
            Debug.Log("Application pausing after " + Time.time + " seconds");

            // log the game state before game pauses
            LogKeyFrame("gamePause");

            // log the pause
            MetaLogEntry metaEntry = new MetaLogEntry();
            metaEntry.setValues("WF_GamePaused", "WF_Meta", new MetaLogEntry.MetaPayload("pause"));
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(LOGGING_VERSION);
            string json = JsonUtility.ToJson(metaEntry);
            reference.Push().SetRawJsonValueAsync(json);
        }
	}

	public static void LogGameUnpause() {
        if (LOGGING)
        {
            // log the unpause
            MetaLogEntry metaEntry = new MetaLogEntry();
            metaEntry.setValues("WF_GameUnpaused", "WF_Meta", new MetaLogEntry.MetaPayload("unpause"));
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(LOGGING_VERSION);
            string json = JsonUtility.ToJson(metaEntry);
            reference.Push().SetRawJsonValueAsync(json);
        }
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus) {
			LogGamePause ();
		} else {
			LogGameUnpause ();
		}
	}
}

[Serializable]
class PlayerData {
    public bool displayButton;                  // users must click on button to submit word
    public bool displaySelectedScore;           // show currently selected word score
    public bool displayHighlightFeedback;       // feedback during highlighting of words
    public string username;                     // the public username to display
    public bool instructions;                   // whether or not to show the instructions
    public string userID;
    public long myHighScore;

    public PlayerData(bool button, bool score, bool highlight, 
                      string username, bool instructions, string userID,
                     long myHighScore) {
        displayButton = button;
        displaySelectedScore = score;
        displayHighlightFeedback = highlight;
        this.username = username;
        this.instructions = instructions;
        this.userID = userID;
        this.myHighScore = myHighScore;
    }
}