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
	public static bool LOGGING = false;
	public const string LOGGING_VERSION = "WFLogs_V1_0_1_debug";
	public const string APP_VERSION = "WF_1.0.1_debug";
    public const string DATA_PATH = "/playerData.dat";

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
	GameObject nextButton;
	public static Text usernameText;
	public static int GAME_ID;
	public static string username;
	public static int userID;
	public static string deviceModel;
	static bool areBoxesFalling = true;
	public GameObject gameOverPanel = null;
	static bool gameHasBegun = false;
	static bool initialLog = true;
    public static bool INSTRUCTIONS_PANEL = true;

	/**********************************
	 * Feature: Analyzing board state and immediate feedback of relative rarity of word using a trie
	 **********************************/
	public static Trie trie;

	void Awake() {
        // singleton pattern-esque?
        if (gameManager == null) {
            gameManager = this;
            gameOverPanel.SetActive(false);
        } else if (gameManager != null) {
            Destroy(gameObject);
        }
	}

	// Use this for initialization
	void Start () {
		deviceModel = SystemInfo.deviceModel;

		// Using time since epoch date as unique game ID
		System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
		GAME_ID = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
		Debug.Log ("Game id: " + GAME_ID);

		BoxScript.camShake = gameObject.AddComponent<CamShakeSimpleScript> ();
		//playButton = GameObject.Find ("PlayButton");
		nextButton = GameObject.Find ("NextStageButton");
		if (nextButton != null) {
			nextButton.SetActive (false);
		}
		username = PlayerPrefs.GetString ("username");

		// randomize userID
		userID = UnityEngine.Random.Range (0, int.MaxValue);

		// TODO: check to see that userID is unique

		// display the username on the screen
		usernameText = GameObject.Find("UsernameText").GetComponent<Text>();
		usernameText.text = username;

		// do various logging for the start of the game
        LogStartOfGame();

        // check version and hide/show Play Word button depending on version
        if (DISPLAY_BUTTON)
        {
            playButton.SetActive(true);
        }
        else
        {
            playButton.SetActive(false);
        }
	}

    /**
     * Load frictional pattern, or randomize it whenever the game starts
     */
    void OnEnable()
    {
        if (File.Exists(Application.persistentDataPath + DATA_PATH))
        {
            // Read the file to load the frictional pattern data
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + DATA_PATH, FileMode.Open);

            PlayerData data = (PlayerData) bf.Deserialize(file);
            file.Close();

            //// set the local variables to the data from the file
            //DISPLAY_BUTTON = data.displayButton;
            //DISPLAY_SELECTED_SCORE = data.displaySelectedScore;
            //DISPLAY_HIGHLIGHT_FEEDBACK = data.displayHighlightFeedback;
            //DISPLAY_TUTORIAL = false;

            // TODO: DEBUG ONLY change back before release
            DISPLAY_BUTTON = true;
            DISPLAY_SELECTED_SCORE = data.displaySelectedScore;
            DISPLAY_HIGHLIGHT_FEEDBACK = data.displayHighlightFeedback;
            DISPLAY_TUTORIAL = false;

            //// If file doesn't exist yet, randomize and initialize variables
            //DISPLAY_BUTTON = UnityEngine.Random.Range(0, int.MaxValue) % 2 == 0;
            //DISPLAY_TUTORIAL = false;
            //DISPLAY_SELECTED_SCORE = UnityEngine.Random.Range(0, int.MaxValue) % 2 == 0;
            //DISPLAY_HIGHLIGHT_FEEDBACK = UnityEngine.Random.Range(0, int.MaxValue) % 2 == 0;
        }
        else 
        {
            // If file doesn't exist yet, randomize and initialize variables
            DISPLAY_BUTTON = UnityEngine.Random.Range(0, int.MaxValue) % 2 == 0;
            DISPLAY_TUTORIAL = false;
            DISPLAY_SELECTED_SCORE = UnityEngine.Random.Range(0, int.MaxValue) % 2 == 0;
            DISPLAY_HIGHLIGHT_FEEDBACK = UnityEngine.Random.Range(0, int.MaxValue) % 2 == 0;
        }

        Debug.Log("Button: " + DISPLAY_BUTTON);
        Debug.Log("Selected Score: " + DISPLAY_SELECTED_SCORE);
        Debug.Log("Highlight: " + DISPLAY_HIGHLIGHT_FEEDBACK);
    }

    /**
     * Save frictional pattern to device whenever the game quits
     */
    void OnDisable()
    {
        // Open the file to write the data
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + DATA_PATH);

        PlayerData data = new PlayerData(DISPLAY_BUTTON, DISPLAY_SELECTED_SCORE, DISPLAY_HIGHLIGHT_FEEDBACK, username);

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

	/***************************************************
	 * Functions for checking for combos, initializing the main board, etc
	 ***************************************************/

	/*
	bool CheckValidWord(int startX, int startY, int endX, int endY) {
		string word = "";

		// check to see if word is in column or row
		if (startX == endX) {
			// column
			if (startY < endY) {
				// forward
				for (int y = startY; y <= endY; ++y) {
					word += initialBoard [startX, y];			
				}
			} else {
				// reverse
				for (int y = startY; y >= endY; --y) {
					word += initialBoard [startX, y];
				}
			}
		} else if (startY == endY) {
			// row
			if (startX < endX) {
				//forward
				for (int x = startX; x <= endX; ++x) {
					word += initialBoard [x, startY];
				}
			} else {
				// reverse
				for (int x = startX; x >= endX; --x) {
					word += initialBoard [x, startY];
				}
			}
		} else {
			Debug.Log ("Error: CheckValidWord only checks rows or columns");
			return false;
		}

		return BoxScript.IsValidWord (word);
	}

	void MarkFlaggedBoard(int startX, int startY, int endX, int endY) {
		if (startX < endX) {
			for (int x = startX; x <= endX; ++x) {
				flaggedBoard [x, startY] = true;
			}
		} else {
			for (int y = startY; y <= endY; ++y) {
				flaggedBoard [startX, y] = true;
			}
		}
	}

	bool ContainsNoValidWords(char[,] board) {
		// check all Ngrams to see if they contain valid words
		bool flag = true;
		int rLength = board.GetLength (0);
		int cLength = board.GetLength (1);

		// check rows
		for (int row = 0; row < board.GetLength (1); ++row) {
			for (int n = 0; n < rLength; ++n) {
				for (int len = 3; (n+len) <= rLength; ++len) {
					// TODO: check to see if any of the letters are flagged

					// check forwards and backwards
					if (CheckValidWord (n, row, n + len - 1, row) 
						|| CheckValidWord (n + len - 1, row, n, row)) {
						flag = false;

						MarkFlaggedBoard (n, row, n + len - 1, row);
					}
				}
			}
		}

		// check columns
		for (int col = 0; col < board.GetLength (0); ++col) {
			for (int n = 0; n < cLength; ++n) {
				for (int len = 3; (n+len) <= cLength; ++len) {
					// TODO: check to see if any of the letters are flagged

					// check forwards and backwards
					if (CheckValidWord (col, n, col, n + len - 1) 
						|| CheckValidWord (col, n + len - 1, col, n)) {
						flag = false;

						MarkFlaggedBoard (col, n, col, n + len - 1);
					}
				}
			}
		}

		return flag;
	}

	void RerollLetters() {
		// randomize all letters that are marked "true" in flaggedBoard
		for (int x = 0; x < flaggedBoard.GetLength (0); ++x) {
			for (int y = 0; y < flaggedBoard.GetLength (1); ++y) {
				if (flaggedBoard [x, y]) {
					int i = Random.Range (0, letterFreq.Count);
					initialBoard [x, y] = (char)(letterFreq [i] + 'A');
				}
			}
		}
	}

	void ResetFlaggedBoard() {
		for (int x = 0; x < flaggedBoard.GetLength (0); ++x) {
			for (int y = 0; y < flaggedBoard.GetLength (1); ++y) {
				flaggedBoard [x, y] = false;
			}
		}
	}

	// DEBUGGING PURPOSES ONLY
	void PrintInitialBoard() {
		for (int row = 0; row < initialBoard.GetLength (1); ++row) {
			string rowStr = "";
			for (int col = 0; col < initialBoard.GetLength (0); ++col) {
				rowStr += initialBoard [col, row];
			}

			Debug.Log (rowStr);
		}
	}
	*/
}

[Serializable]
class PlayerData {
    public bool displayButton;          // users must click on button to submit word
    public bool displaySelectedScore;  // show currently selected word score
    public bool displayHighlightFeedback;      // feedback during highlighting of words
    public string username;

    public PlayerData(bool button, bool score, bool highlight, string username) {
        displayButton = button;
        displaySelectedScore = score;
        displayHighlightFeedback = highlight;
        this.username = username;
    }
}