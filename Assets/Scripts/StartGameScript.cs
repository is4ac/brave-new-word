using UnityEngine;
//using Firebase;
//using Firebase.Unity.Editor;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;
using UnityEngine.Audio;

public class StartGameScript : MonoBehaviour
{
    public static event Action<string> OnUsernameChange = delegate { };
    public static event Action OnRandomizeName = delegate { };

    public const string DATA_PATH = "/BraveNewWord_playerData.dat";
    public GameObject settingsButton;
    public GameObject instructionsPanel;
    public GameObject settingsPanel;
    public AudioMixer mixer;
    public AudioSettings audioSettings;

    private bool _randomizeFeatures = false;

    // use for initialization
    void Awake()
    {
        if (!GameManagerScript.debug)
        {
            settingsButton.SetActive(false);
        }
        else
        {
            settingsButton.SetActive(true);
        }

        // Set screen orientation mode to portrait only
        Screen.orientation = ScreenOrientation.Portrait;

        // set input method based on if touch input is supported or not
        if (Input.touchSupported)
        {
            //Debug.Log("Touch supported!");
            TouchInputHandler.touchSupported = true;
        }
        else
        {
            //Debug.Log("Touch not supported!");
            TouchInputHandler.touchSupported = false;
        }

        /*
        // Firebase database logistics for editor
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://wordflood-bf7c4.firebaseio.com/");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            Firebase.DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                    "Could not resolve all Firebase dependencies: " + dependencyStatus);
                GameManagerScript.logging = false;
            }
        });
        */
    }

    void Start()
    {
        // DEBUG MODE: Randomize the various frictional features of the game
        //RandomizeFeatures();

        // Load player data file
        LoadFile();

        PlayBgMusic();
    }

    void Update()
    {
        // randomize features if the flag is turned on (for threading issues, Unity doesn't allow Library calls
        // outside of the main thread)
        if (_randomizeFeatures)
        {
            RandomizeFeatures();
            _randomizeFeatures = false;
        }
        
        // Android back button should exit the game if on main screen
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                // If on Instructions menu, then it should close that panel
                if (instructionsPanel.activeInHierarchy)
                {
                    instructionsPanel.SetActive(false);
                }
                else if (settingsPanel.activeInHierarchy)
                {
                    settingsPanel.SetActive(false);
                }
                else
                {
                    // Exit game app
                    Application.Quit();
                }
            }
        }
    }

    public void SetRandomize()
    {
        _randomizeFeatures = true;
    }

    void PlayBgMusic()
    {
        // TODO: MOVE THIS code to the GameManager Audio component class?
        if (GameManagerScript.juiceProductive)
        {
            AudioManager.instance.Play("JuicyTheme");
        }
        else if (GameManagerScript.juiceUnproductive)
        {
            AudioManager.instance.Play("DubstepTheme");
        }
        else
        {
            AudioManager.instance.Play("CalmTheme");
        }
    }

    void OnDisable()
    {
        // save player data
        GameManagerScript.SavePlayerData(mixer);
    }

    public void LoadFile()
    {
        if (File.Exists(Application.persistentDataPath + DATA_PATH))
        {
            Debug.Log("Loading from file... " + Application.persistentDataPath + DATA_PATH);

            // Read the file to load the frictional pattern data
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + DATA_PATH, FileMode.Open);

            PlayerData data = (PlayerData)bf.Deserialize(file);
            file.Close();

            //// set the local variables to the data from the file
            // TODO: uncomment this before deploy

            GameManagerScript.obstructionProductive = data.obstructionProductive;
            GameManagerScript.obstructionUnproductive = data.obstructionUnproductive;
            GameManagerScript.juiceProductive = data.juiceProductive;
            GameManagerScript.juiceUnproductive = data.juiceUnproductive;
            GameManagerScript.displayInstructions = data.instructions;
            GameManagerScript.userId = data.userId;
            GameManagerScript.myHighScore = data.myHighScore;
            GameManagerScript.gameNumber = data.gameNumber;

            OnUsernameChange(data.username);

            // check to see if some values are blank
            if (GameManagerScript.userId == null || GameManagerScript.userId.Trim() == "")
            {
                GameManagerScript.userId = Guid.NewGuid().ToString();
            }

            // set audio levels
            audioSettings.UpdateSliders(data.masterVolume, data.sfxVolume, data.musicVolume);
            mixer.SetFloat("masterVolume", data.masterVolume);
            mixer.SetFloat("sfxVolume", data.sfxVolume);
            mixer.SetFloat("musicVolume", data.musicVolume);
            
            // Check deviceId db. If values are the same, then no change. 
            // if db values are different from local file, then revert to db values.
            DbManager.instance.CheckDeviceId();

            // TODO: DEBUG ONLY: COMMENT OUT before release
            /*
            GameManagerScript.myHighScore = 0;
            GameManagerScript.userID = Guid.NewGuid().ToString();
            GameManagerScript.OBSTRUCTION_PRODUCTIVE = true;
            GameManagerScript.OBSTRUCTION_UNPRODUCTIVE = false;
            GameManagerScript.JUICE_PRODUCTIVE = false;
            GameManagerScript.JUICE_UNPRODUCTIVE = true;
            */

            // TODO: COMMENT this line before release. DEBUG testing only!!
            //RandomizeFeatures();
        }
        else
        {
            GameManagerScript.displayInstructions = true;

            // If file doesn't exist yet, check to see if deviceID exists in DB.
            // If it does, grab the userID and game version from the DB.
            // If deviceID doesn't exist yet, randomize and initialize variables
            // and add deviceID to DB
            // randomize first in case Google Firebase servers are down (which is very possible at this time)
            RandomizeFeatures();
            DbManager.instance.InitializeDeviceId();
        }

        Debug.Log("Obstruction Prod.: " + GameManagerScript.obstructionProductive);
        Debug.Log("Obstruction Unprod.: " + GameManagerScript.obstructionUnproductive);
        Debug.Log("Juice Prod.: " + GameManagerScript.juiceProductive);
        Debug.Log("Juice Unprod.: " + GameManagerScript.juiceUnproductive);
    }

    public void RandomizeFeatures()
    {
        // If file doesn't exist yet, randomize and initialize variables
        // default to false
        GameManagerScript.obstructionProductive = false;
        GameManagerScript.obstructionUnproductive = false;
        GameManagerScript.juiceProductive = false;
        GameManagerScript.juiceUnproductive = false;

        // randomize the version
        int version = UnityEngine.Random.Range(0, 9);
        switch (version)
        {
            case 0:
                // everything remains off
                break;
            case 1:
                GameManagerScript.obstructionProductive = true;
                goto case 2;
            case 2:
                GameManagerScript.juiceProductive = true;
                break;
            case 3:
                GameManagerScript.obstructionUnproductive = true;
                goto case 4;
            case 4:
                GameManagerScript.juiceUnproductive = true;
                break;
            case 5:
                GameManagerScript.obstructionProductive = true;
                break;
            case 6:
                GameManagerScript.obstructionUnproductive = true;
                break;
            case 7:
                GameManagerScript.obstructionUnproductive = true;
                break;
            case 8:
                GameManagerScript.obstructionUnproductive = true;
                break;
        }

        // randomize userID using a GUID (UUID) 
        GameManagerScript.userId = Guid.NewGuid().ToString();
        
        // randomize username
        OnRandomizeName();
    }

    public void OpenTermsOfService()
    {
        Application.OpenURL("http://is4ac.github.io/brave-new-word-site/terms");
    }

    public void OpenPrivacyPolicy()
    {
        Application.OpenURL("http://is4ac.github.io/brave-new-word-site/privacy");
    }

    // Initialize the Firebase database:
    protected virtual void InitializeFirebase()
    {
        // sign in anomymously
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });
    }
}
