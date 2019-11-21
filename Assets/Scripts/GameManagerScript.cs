using System.Collections.Generic;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Database;

public class GameManagerScript : MonoBehaviour
{

    // singleton instance
    public static GameManagerScript gameManager;

    // Set to true to log to Firebase database, false to turn off
    // TODO: set LOGGING to true before deploy!
    public static bool LOGGING = true;
    public const string LOGGING_VERSION = "BNWLogs_V1_0_0_BETA";
    public const string APP_VERSION = "BNW_1_0_0_BETA";
    public static string usersDbName = "users" + DBManager.versionNumber;

    public DatabaseReference dbUsers;

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
    public ParticleSystem bgAnimation; // background animation for unproductive juice
    public ParticleSystem streakingParticles; // Unproductive juicy effects
    public static string GAME_ID;
    public static string username;
    public static string userID;
    public static string deviceModel;
    static bool areBoxesFalling = true;
    public GameObject gameOverPanel;
    static bool gameHasBegun;
    static bool initialLog = true; // keeps track of whether or not the initial logging should be done
    public static bool INSTRUCTIONS_PANEL = true;
    public static long myHighScore;
    public static bool myHighScoreUpdated;
    public static bool globalHighScoreUpdated;
    public static double previousSubmissionTime;
    public static double pauseTime;
    public static string myHighestScoringWord = "";
    public static int myHighestScoringWordScore;
    public static string myRarestWord = "";
    public static float myRarestWordRarity;
    public static int gameNumber;
    public static DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

    private static float timer;
    private static float waitTime = 0.2f;
    private static float maxTime = 150.0f; // 2:30 minutes? for now.
    public static float remainingTime = maxTime;
    private static bool isBombAudioPlaying;
    private static int counter = 5;

    private static float juicyTimer;
    private static float juicyWaitTime = 0.2f;
    private static float juicyRemainingTime;

    public static float submitPromptTimer;
    public static bool submitPromptOn;

    void Awake()
    {
        // singleton pattern-esque
        if (gameManager == null)
        {
            gameManager = this;
            gameOverPanel.SetActive(false);
            instructionsPanel.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start()
    {
        deviceModel = SystemInfo.deviceModel;

        if (LOGGING) dbUsers = FirebaseDatabase.DefaultInstance.GetReference(usersDbName);

        // Generate a new GAME_ID using the Guid class
        GAME_ID = Guid.NewGuid().ToString();
        Debug.Log("Game id: " + GAME_ID);

        // retrieve the player's username
        username = PlayerPrefs.GetString("username");

        // set the initial submission epoch time
        previousSubmissionTime = ((System.DateTime.UtcNow - epochStart).TotalMilliseconds);

        // check version and hide/show Play Word button depending on version
        if (OBSTRUCTION_PRODUCTIVE || OBSTRUCTION_UNPRODUCTIVE)
        {
            playButton.SetActive(true);
            UpdatePlayButton();
        }
        else
        {
            playButton.SetActive(false);
        }

        // ========FEATURE: Unproductive Juice BG Animation Particles==========
        if (JUICE_UNPRODUCTIVE)
        {
            bgAnimation.Play();
            juicyRemainingTime = UnityEngine.Random.Range(10f, 20f);
            StartCoroutine(AudioManager.instance.PlayRandomLoop(new string[] { "Sparkle2" }));
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

    // Update is called once per frame
    void Update()
    {
        // if submit prompt panel is open, update the timer
        if (submitPromptOn)
        {
            submitPromptTimer += Time.deltaTime;
        }

        // Every 1/5 of a second, update the timer progress bar
        if (gameHasBegun) timer += Time.deltaTime;

        if (timer >= waitTime)
        {
            // adjust remaining time
            remainingTime -= timer;

            float scale = remainingTime / maxTime;

            // update progress bar
            progressBarFG.transform.localScale = new Vector3(scale, 1.0f, 1.0f);

            // reset timer
            timer -= waitTime;

            // Countdown timer displays seconds remaining near the end.
            if (counter > 0 && remainingTime > counter-1 && remainingTime <= counter)
            {
                TextFaderScript textFader = GameObject.Find("CountdownMessage").GetComponent<TextFaderScript>();
                textFader.FadeText(0.1f, counter.ToString());
                counter--;
            }

            // check to see if it's time to start playing the timer bomb audio
            // FEATURE: ONLY IF JUICINESS IS ON
            if ((JUICE_PRODUCTIVE || JUICE_UNPRODUCTIVE)
                && remainingTime <= 5.275f
                && !isBombAudioPlaying)
            {
                isBombAudioPlaying = true;
                AudioManager.instance.Play("TimeBomb");
            }

            // check if game is over
            if (remainingTime <= 0.0f)
            {
                GameOver();
            }
        }

        //=============FEATURE: Random Pew Pew effects=========================
        // Unproductive Juice: every once in a while, do a random particle
        //                     animation along with pew pew sfx.
        //=====================================================================
        if (JUICE_UNPRODUCTIVE)
        {
            if (gameHasBegun) juicyTimer += Time.deltaTime;

            if (juicyTimer >= juicyWaitTime)
            {
                juicyRemainingTime -= juicyTimer;

                // reset timer
                juicyTimer -= juicyWaitTime;

                // check if it's time to do juicy effects
                if (juicyRemainingTime <= 0.0f)
                {
                    PlayJuicyEffects();

                    // reset
                    juicyRemainingTime = UnityEngine.Random.Range(10f, 30f);
                }
            }
        }

        // Log the keyframe (game state) after all the boxes have stopped falling
        if (areBoxesFalling)
        {
            if (!CheckIfBoxesAreFalling())
            {
                areBoxesFalling = false;

                if (!initialLog)
                {
                    // enable input again after all boxes have fallen
                    TouchInputHandler.inputEnabled = true;

                    LogKeyFrame("post");
                }
                else
                {
                    LogKeyFrame("gameStart");

                    // display the instructions/start game panel
                    instructionsPanel.SetActive(true);

                    initialLog = false;
                }
            }
        }
        else if (CheckIfBoxesAreFalling())
        {
            areBoxesFalling = true;
        }
    }

    public void PlayJuicyEffects()
    {
        // Set new Particle System GameObject as a child of desired GO.
        // Right now parent would be the same GO in which this script is attached
        // You can also make it others child by ps.transform.parent = otherGO.transform.parent;

        // After setting this, replace the position of that GameObject as where the parent is located.
        streakingParticles.Play();
        StartCoroutine(AudioManager.instance.PlayMultiple("LaserPew", 3));
    }

    public void UpdatePlayButton()
    {
        if (BoxScript.currentWord.Length >= 3 &&
            BoxScript.GetWordRank(BoxScript.currentWord) > -1)
        {
            playButton.GetComponent<Button>().enabled = true;
        }
        else
        {
            playButton.GetComponent<Button>().enabled = false;
        }
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
                                         myHighScore,
                                         gameNumber);

        // serialize and write to file
        bf.Serialize(file, data);
        file.Close();
    }

    public static void LogInstructionsClick(Vector2 pos)
    {
        if (LOGGING)
        {
            // log the location of the click
            ClickLogEntry entry = new ClickLogEntry();
            ClickLogEntry.ClickPayload payload = new ClickLogEntry.ClickPayload
            {
                screenX = pos.x,
                screenY = pos.y
            };

            entry.setValues("BNW_InstructionsClosed", "BNW_Action", payload);
            string json = JsonUtility.ToJson(entry);
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(LOGGING_VERSION);
            reference.Push().SetRawJsonValueAsync(json);
        }
    }

    public void LogStartOfGame()
    {
        if (LOGGING)
        {
            Debug.Log("Logging beginning of game");

            // Log beginning of game
            MetaLogEntry entry = new MetaLogEntry();
            entry.setValues("BNW_GameStart", "BNW_Meta", new MetaLogEntry.MetaPayload("start"));
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
            child.Child("gameNumber").SetValueAsync(gameNumber);

            // Log username into users database if it doesn't already exist or if the username has changed
            dbUsers.GetValueAsync().ContinueWith(task =>
            {
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

    public void SetButtonDisplay(bool value)
    {
        playButton.SetActive(value);
    }

    public static void ResetTimer()
    {
        timer = 0.0f;
        remainingTime = maxTime;
    }

    static bool CheckIfBoxesAreFalling()
    {
        bool falling = false;

        for (int i = 0; i < BoxScript.gridWidth; ++i)
        {
            if (BoxScript.IsBoxInColumnFalling(i) || !BoxScript.IsColumnFull(i))
            {
                falling = true;
                break;
            }
        }

        return falling;
    }

    // Play word!
    public void PlayWord()
    {
        BoxScript.PlayWord();
        Debug.Log("Play word pressed.");

        // display the high score?
        Debug.Log("Top score: " + DBManager.instance.topScore);
    }

    public static void BeginGame()
    {
        TouchInputHandler.inputEnabled = true;
        TouchInputHandler.touchEnabled = true;
        gameHasBegun = true;
        gameNumber++;
    }

    public void GameOver()
    {
        gameHasBegun = false;
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

    public static bool GameHasStarted()
    {
        return gameHasBegun;
    }

    public void Reset()
    {
        initialLog = true;
        gameHasBegun = false;
        isBombAudioPlaying = false;

        GameObject boxes = GameObject.Find("SpawnBoxes");

        foreach (Transform child in boxes.transform)
        {
            child.gameObject.GetComponent<SpawnBoxScript>().Reset();
        }

        BoxScript.Reset();

        gameOverPanel.SetActive(false);

        int scene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(scene); // eventually delete the key "currentPath"
                                       // PlayerPrefs.DeleteKey("currentPath");
    }

    public static void LogKeyFrame(string state)
    {
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

            entry.setValues("BNW_GameState", "BNW_KeyFrame", payload);
            string json = JsonUtility.ToJson(entry);
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(LOGGING_VERSION);
            reference.Push().SetRawJsonValueAsync(json);
        }
    }

    public static void LogEndOfGame()
    {
        if (LOGGING)
        {
            Debug.Log("Logging end of game");

            // log the end game state when the player finished the round
            LogKeyFrame("gameEnd");

            // log the end of the game
            MetaLogEntry metaEntry = new MetaLogEntry();
            metaEntry.setValues("BNW_GameEnd", "BNW_Meta", new MetaLogEntry.MetaPayload("end"));
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(LOGGING_VERSION);
            string json = JsonUtility.ToJson(metaEntry);
            reference.Push().SetRawJsonValueAsync(json);
        }
    }

    /**
	 * User or the OS force quits the application
	 */
    public static void LogGamePause()
    {
        if (LOGGING)
        {
            Debug.Log("Application pausing after " + Time.time + " seconds");

            // log the game state before game pauses
            LogKeyFrame("gamePause");

            // log the pause
            MetaLogEntry metaEntry = new MetaLogEntry();
            metaEntry.setValues("BNW_GamePaused", "BNW_Meta", new MetaLogEntry.MetaPayload("pause"));
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(LOGGING_VERSION);
            string json = JsonUtility.ToJson(metaEntry);
            reference.Push().SetRawJsonValueAsync(json);
        }
    }

    public static void LogGameUnpause()
    {
        if (LOGGING)
        {
            // log the unpause
            MetaLogEntry metaEntry = new MetaLogEntry();
            metaEntry.setValues("BNW_GameUnpaused", "BNW_Meta", new MetaLogEntry.MetaPayload("unpause"));
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(LOGGING_VERSION);
            string json = JsonUtility.ToJson(metaEntry);
            reference.Push().SetRawJsonValueAsync(json);
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // on pause, pause the timings for logging word submits
            LogGamePause();

            pauseTime = ((System.DateTime.UtcNow - epochStart).TotalMilliseconds);
        }
        else
        {
            // on unpauses, resume the timer for logging word submits
            LogGameUnpause();

            previousSubmissionTime =
                ((System.DateTime.UtcNow - epochStart).TotalMilliseconds) - pauseTime
                    + previousSubmissionTime;
        }
    }
}

[Serializable]
class PlayerData
{
    public bool obstructionProductive;      // users must click on button to submit word
    public bool obstructionUnproductive;    // show currently selected word score
    public bool juiceProductive;            // feedback during highlighting of words
    public bool juiceUnproductive;          // word score is based on word rarity, not frequency?
    public string username;                 // the public username to display
    public bool instructions;               // whether or not to show the instructions
    public string userID;                   // the unique user ID
    public long myHighScore;                // the local high score of the player
    public int gameNumber;                  // the # of game the player is playing

    public PlayerData(bool obP, bool obU, bool juiceP, bool juiceU,
                      string username, bool instructions, string userID,
                     long myHighScore, int gameNumber)
    {
        obstructionProductive = obP;
        obstructionUnproductive = obU;
        juiceProductive = juiceP;
        juiceUnproductive = juiceU;
        this.username = username;
        this.instructions = instructions;
        this.userID = userID;
        this.myHighScore = myHighScore;
        this.gameNumber = gameNumber;
    }
}