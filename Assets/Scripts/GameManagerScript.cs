using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

/**
 * Design TODOs:
 *
 * 1. Break up this class into components?
 *      a. Gameplay component (timer, spawn boxes, instructions/settings panels, play button, etc.)
 *      b. Global Game component (global static variables, etc)
 *      c. Audio component (background music start -- currently in AudioManager class, move)
 *      d. Logging component (could be the same class as BoxScript's logging component)
 * 2. Make static / global variables more static and lean so it doesn't depend on the scene
 *      (and limit it to the really global variables like LOGGING, VERSION, and feature booleans)
 * 3. 
 */


public class GameManagerScript : MonoBehaviour
{
    // singleton instance
    public static GameManagerScript gameManager;

    // Set to true to log to Firebase database, false to turn off
    // TODO: set DEBUG to false before full deploy!
    public static bool DEBUG = false;
    // TODO: set LOGGING to true before deploy!
    public static bool LOGGING = true;
    // TODO: Change version number after each update
    public const string VERSION = "0_1_2";
    public const string LOGGING_VERSION = "BNWLogs_V" + VERSION;
    public const string APP_VERSION = "BNW_" + VERSION;

    /*************************************
     * Feature booleans - these keep track of what features are on/off for this current game
     *************************************/
    public static bool OBSTRUCTION_PRODUCTIVE;      // users must click on button and see stats before submitting word
    public static bool OBSTRUCTION_UNPRODUCTIVE;    // users must tap to select each letter individually
    public static bool JUICE_PRODUCTIVE;            // juiciness is distracting but matches game state
    public static bool JUICE_UNPRODUCTIVE;          // juiciness is distracting and doesn't match game state
                                                    /*********************************************/

    public GameObject playButton;
    public GameObject instructionsPanel;
    public GameObject gameOverMessageObject;
    public GameObject gameOverScoreTextObject;
    public GameObject highScoreTextObject;
    public GameObject highestScoringWordObject;
    public GameObject rarestWordObject;
    public GameObject audioSettingsPanel;
    public AudioSettings audioSettings;
    public ConsentMenuScript menuScript;
    public GameObject settingsButton;
    public GameObject progressBarFG; // the progress bar that shows the timer
    public ParticleSystem bgAnimation; // background animation for unproductive juice
    public ParticleSystem bgAnimation2; // bg animation for unproductive juice 2
    public ParticleSystem streakingParticles; // Unproductive juicy effects
    public AudioMixer mixer;    // Audio mixer for volume settings
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
    public static DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static float timer;
    private static float waitTime = 0.2f;

    // TODO: set maxTime to 150.0f before deploy
    private static float maxTime = 150.0f; // 150 seconds = 2:30 minutes? for now.
    public static float remainingTime = maxTime;
    private static bool isBombAudioPlaying;
    private static int counter = 5;

    private static float juicyTimer;
    private static float juicyWaitTime = 0.2f;
    private static float juicyRemainingTime;

    public static float submitPromptTimer;
    public static bool submitPromptOn;
    private Coroutine audioCoroutine;

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
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        deviceModel = SystemInfo.deviceModel;

        // Generate a new GAME_ID using the Guid class
        GAME_ID = Guid.NewGuid().ToString();

        // set the initial submission epoch time
        previousSubmissionTime = ((System.DateTime.UtcNow - epochStart).TotalMilliseconds);

        // check version and hide/show Play Word button depending on version
        if (OBSTRUCTION_PRODUCTIVE || OBSTRUCTION_UNPRODUCTIVE)
        {
            playButton.SetActive(true);
            playButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            playButton.SetActive(false);
        }

        // ========FEATURE: Unproductive Juice BG Animation Particles==========
        if (JUICE_UNPRODUCTIVE)
        {
            StartUnproductiveJuice();
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

        // Set the audio setting sliders to the right levels
        mixer.GetFloat("masterVolume", out float masterVol);
        mixer.GetFloat("sfxVolume", out float sfxVol);
        mixer.GetFloat("musicVolume", out float musicVol);
        audioSettings.UpdateSliders(masterVol, sfxVol, musicVol);

        // do various logging for the start of the game
        Logger.LogStartOfGame();
    }

    // Update is called once per frame
    void Update()
    {
        // Android back button should go back to main menu
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                if (audioSettingsPanel.activeInHierarchy)
                {
                    Logger.LogAudioSettings();
                    audioSettingsPanel.SetActive(false);
                }
                else
                {
                    // Change scenes back to main menu
                    menuScript.GoToNextScene(0);
                    return;
                }
            }
        }

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
            if (scale < 0) scale = 0;

            // update progress bar
            progressBarFG.transform.localScale = new Vector3(scale, 1.0f, 1.0f);

            // reset timer
            timer -= waitTime;

            // Countdown timer displays seconds remaining near the end.
            if (counter > 0 && remainingTime > counter - 1 && remainingTime <= counter)
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

                    Logger.LogKeyFrame("post");
                }
                else
                {
                    Logger.LogKeyFrame("gameStart");

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

    public void StartUnproductiveJuice()
    {
        bgAnimation.Play();
        juicyRemainingTime = UnityEngine.Random.Range(10f, 20f);
        audioCoroutine = StartCoroutine(AudioManager.instance.PlayRandomLoop(new string[] { "Sparkle2" }));
    }

    public void StopUnproductiveJuice()
    {
        bgAnimation.Stop();

        if (audioCoroutine != null)
        {
            StopCoroutine(audioCoroutine);
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
            playButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            playButton.GetComponent<Button>().interactable = false;
        }
    }

    /**
     * Save frictional pattern to device whenever the game quits
     */
    void OnDisable()
    {
        SavePlayerData(mixer);
    }

    public static void SavePlayerData(AudioMixer mixer)
    {
        // grab audio volume levels
        mixer.GetFloat("masterVolume", out float masterVol);
        mixer.GetFloat("sfxVolume", out float sfxVol);
        mixer.GetFloat("musicVolume", out float musicVol);

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
                                         gameNumber,
                                         masterVol,
                                         sfxVol,
                                         musicVol);

        // serialize and write to file
        bf.Serialize(file, data);
        file.Close();
    }

    public void SetButtonDisplay(bool value)
    {
        playButton.SetActive(value);
    }

    public static void ResetTimer()
    {
        timer = 0.0f;
        remainingTime = maxTime;
        submitPromptTimer = 0.0f;
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
        //Debug.Log("Play word pressed.");

        // display the high score?
        //Debug.Log("Top score: " + DBManager.instance.topScore);
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

        highScoreText.text = "Your High Score: " + myHighScore;

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

        string rarityText = (BoxScript.GetWordRank(myRarestWord) * 100).ToString("0.00");
        if (myRarestWord.Trim() == "")
        {
            rarityText = "0";
        }

        // update rarest word text
        rarestWordText.text = "Rarest Word:\n"
            + myRarestWord + "\n"
            + rarityText + "%";

        // disable touch events
        TouchInputHandler.touchEnabled = false;
        TouchInputHandler.inputEnabled = false;

        // disable button press
        playButton.GetComponent<Button>().interactable = false;

        // Log the final state of the game
        Logger.LogEndOfGame();
    }

    public static bool GameHasStarted()
    {
        return gameHasBegun;
    }

    public void Reset()
    {
        // reset all important variables
        initialLog = true;
        gameHasBegun = false;
        isBombAudioPlaying = false;
        myHighestScoringWord = "";
        myHighestScoringWordScore = 0;
        myRarestWord = "";
        myRarestWordRarity = 0;
        myHighScoreUpdated = false;
        counter = 5;

        GameObject boxes = GameObject.Find("SpawnBoxes");

        foreach (Transform child in boxes.transform)
        {
            child.gameObject.GetComponent<SpawnBoxScript>().Reset();
        }

        // reset all important BoxScript variables
        BoxScript.Reset();
        ResetTimer();

        UpdatePlayButton();

        gameOverPanel.SetActive(false);

        int scene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(scene); // eventually delete the key "currentPath"
                                       // PlayerPrefs.DeleteKey("currentPath");
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // on pause, pause the timings for logging word submits
            Logger.LogGamePause();

            pauseTime = ((System.DateTime.UtcNow - epochStart).TotalMilliseconds);
        }
        else
        {
            // on unpauses, resume the timer for logging word submits
            Logger.LogGameUnpause();

            previousSubmissionTime =
                ((System.DateTime.UtcNow - epochStart).TotalMilliseconds) - pauseTime
                    + previousSubmissionTime;
        }
    }
}