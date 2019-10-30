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
    // TODO: set LOGGING to true before deploy!
	public static bool LOGGING = false;
	public const string LOGGING_VERSION = "BNWLogs_V1_1_1_debug";
	public const string APP_VERSION = "BNW_1.1.1_debug";
    public static string usersDbName = "users" + DBManager.versionNumber;

    public DatabaseReference dbUsers;
	CamShakeSimpleScript camShake;

    /*************************************
     * Feature booleans - these keep track of what features are on/off for this current game
     *************************************/
    public static bool OBSTRUCTION_PRODUCTIVE;      // users must click on button and see stats before submitting word
    public static bool OBSTRUCTION_UNPRODUCTIVE;    // users must tap to select each letter individually
    public static bool JUICE_PRODUCTIVE;            // juiciness is distracting but matches game state
    public static bool JUICE_UNPRODUCTIVE;          // juiciness is distracting and doesn't match game state
    public static bool DISPLAY_TUTORIAL;            // OUTDATED // show the tutorial screen with instructions at beginning of game
    /*********************************************/
	
	public GameObject playButton;
    public GameObject instructionsPanel;
    public GameObject gameOverMessageObject;
    public GameObject gameOverScoreTextObject;
    public GameObject highScoreTextObject;
    public GameObject highestScoringWordObject;
    public GameObject rarestWordObject;
    public GameObject progressBarFG; // the progress bar that shows the timer
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
    public static double previousSubmissionTime = 0;
    public static double pauseTime = 0;
    public static string myHighestScoringWord = "";
    public static int myHighestScoringWordScore = 0;
    public static string myRarestWord = "";
    public static float myRarestWordRarity = 0.0f;
    public static DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

    private static float timer = 0.0f;
    private static float waitTime = 0.2f;
    private static float maxTime = 120.0f; // 2 minutes? for now.
    private static float remainingTime = maxTime;
    private static bool isBombAudioPlaying = false;

	void Awake() {
        // singleton pattern-esque
        if (gameManager == null) {
            gameManager = this;
            gameOverPanel.SetActive(false);
            instructionsPanel.SetActive(false);
        } else {
            Destroy(gameObject);
        }
	}

	// Use this for initialization
	void Start () {
		deviceModel = SystemInfo.deviceModel;

        if (LOGGING) dbUsers = FirebaseDatabase.DefaultInstance.GetReference(usersDbName);

		// Generate a new GAME_ID using the Guid class
        GAME_ID = Guid.NewGuid().ToString();
		Debug.Log ("Game id: " + GAME_ID);

		BoxScript.camShake = gameObject.AddComponent<CamShakeSimpleScript> ();

        // retrieve the player's username
        username = PlayerPrefs.GetString("username");

        // set the initial submission epoch time
        previousSubmissionTime = ((System.DateTime.UtcNow - epochStart).TotalMilliseconds);

        // check version and hide/show Play Word button depending on version
        if (OBSTRUCTION_PRODUCTIVE || OBSTRUCTION_UNPRODUCTIVE)
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
            // change the text of the instructions panel
            Transform textPanel = instructionsPanel.transform.Find("InstructionsTextPanel");
            textPanel.Find("InstructionsLabel").GetComponent<Text>().text = "READY?";
            textPanel.Find("InstructionsText").GetComponent<Text>().text = "Press anywhere on the screen to begin!";
            //instructionsPanel.SetActive(false);
            //BeginGame();
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

        PlayerData data = new PlayerData(OBSTRUCTION_PRODUCTIVE, 
                                         OBSTRUCTION_UNPRODUCTIVE, 
                                         JUICE_PRODUCTIVE,
                                         JUICE_UNPRODUCTIVE,
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
            DatabaseReference child = reference.Child(GAME_ID);

            // Log the game details and type of game
            child.Child("obstructionProductive").SetValueAsync(OBSTRUCTION_PRODUCTIVE);
            child.Child("obstructionUnproductive").SetValueAsync(OBSTRUCTION_UNPRODUCTIVE);
            child.Child("juiceProductive").SetValueAsync(JUICE_PRODUCTIVE);
            child.Child("juiceUnproductive").SetValueAsync(JUICE_UNPRODUCTIVE);
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

    public static void ResetTimer() {
        timer = 0.0f;
        remainingTime = maxTime;
    }
	
	// Update is called once per frame
	void Update () {
        // TODO: Only let users go back to the main menu
        // if they go to a settings menu and click exit
        /*
		if (Input.GetKeyDown(KeyCode.Escape)) { 
            // TODO: finish this feature
			SceneManager.LoadScene(0);
		}
		*/

        // Every 1/5 of a second, update the timer progress bar
        if (gameHasBegun) timer += Time.deltaTime;

        if (timer >= waitTime) {
            // adjust remaining time
            remainingTime -= timer;

            float scale = remainingTime / maxTime;

            // update progress bar
            progressBarFG.transform.localScale = new Vector3(scale, 1.0f, 1.0f);

            // reset timer
            timer -= waitTime;

            // check to see if it's time to start playing the timer bomb audio
            // FEATURE: ONLY IF JUICINESS IS ON
            if ((JUICE_PRODUCTIVE || JUICE_UNPRODUCTIVE)
                && remainingTime <= 5.275f 
                && !isBombAudioPlaying) {
                isBombAudioPlaying = true;
                AudioManager.instance.Play("TimeBomb");
            }

            // check if game is over
            if (remainingTime <= 0.0f) {
                GameOver();
            }
        }

		// Log the keyframe (game state) after all the boxes have stopped falling
		if (areBoxesFalling) {
			if (!checkIfBoxesAreFalling ()) {
				areBoxesFalling = false;

				if (!initialLog) {
					LogKeyFrame ("post");
				} else {
					LogKeyFrame ("gameStart");

                    // display the instructions/start game panel
                    instructionsPanel.SetActive(true);

					initialLog = false;
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
        TouchInputHandler.inputEnabled = true;
        TouchInputHandler.touchEnabled = true;
        gameHasBegun = true;
	}

    public void GameOver() {
        gameOverPanel.SetActive(true);
        Text gameOverMessage = gameOverMessageObject.GetComponent<Text>();
        Text gameOverScoreText = gameOverScoreTextObject.GetComponent<Text>();
        Text highScoreText = highScoreTextObject.GetComponent<Text>();
        Text highestScoringWordText = highestScoringWordObject.GetComponent<Text>();
        Text rarestWordText = rarestWordObject.GetComponent<Text>();

        highScoreText.text = "Your High Score: " + GameManagerScript.myHighScore;

        // Check if new local high score was reached
        if (myHighScoreUpdated)
        {
            // update game over text
            gameOverMessage.text = "Congratulations! You've set a new personal high score!";

            // TODO: animate particles
        }

        // Check if new global high score was reached
        if (globalHighScoreUpdated)
        {
            // update game over text
            gameOverMessage.text = "Congratulations! You've set a new global high score!";
            highScoreText.text = "New High Score: " + BoxScript.score;

            // TODO: animate particles
        }

        gameOverScoreText.text = "Score: " + BoxScript.score;

        // update highest scoring word text
        highestScoringWordText.text = "Highest Scoring Word:\n"
            + myHighestScoringWord + "\n"
            + myHighestScoringWordScore + " points";

        // update rarest word text
        rarestWordText.text = "Rarest Word:\n"
            + myRarestWord + "\n"
            + (BoxScript.GetWordRank(myRarestWord) * 100) + "%";

        // disable touch events
        TouchInputHandler.touchEnabled = false;
        TouchInputHandler.inputEnabled = false;

        // disable button press
        playButton.GetComponent<Button>().interactable = false;

        // Log the final state of the game
        LogEndOfGame();
    }

	public static bool GameHasStarted() {
		return gameHasBegun;
	}

	public void Reset() {
        initialLog = true;
        gameHasBegun = false;
        isBombAudioPlaying = false;

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
            // on pause, pause the timings for logging word submits
			LogGamePause ();

            pauseTime = ((System.DateTime.UtcNow - epochStart).TotalMilliseconds);
		} else {
            // on unpauses, resume the timer for logging word submits
			LogGameUnpause ();

            previousSubmissionTime =
                ((System.DateTime.UtcNow - epochStart).TotalMilliseconds) - pauseTime
                    + previousSubmissionTime;
		}
	}
}

[Serializable]
class PlayerData {
    public bool obstructionProductive;                  // users must click on button to submit word
    public bool obstructionUnproductive;           // show currently selected word score
    public bool juiceProductive;       // feedback during highlighting of words
    public bool juiceUnproductive;                  // word score is based on word rarity, not frequency?
    public string username;                     // the public username to display
    public bool instructions;                   // whether or not to show the instructions
    public string userID;                       // the unique user ID
    public long myHighScore;                    // the local high score of the player

    public PlayerData(bool obP, bool obU, bool juiceP, bool juiceU, 
                      string username, bool instructions, string userID,
                     long myHighScore) {
        obstructionProductive = obP;
        obstructionUnproductive = obU;
        juiceProductive = juiceP;
        juiceUnproductive = juiceU;
        this.username = username;
        this.instructions = instructions;
        this.userID = userID;
        this.myHighScore = myHighScore;
    }
}