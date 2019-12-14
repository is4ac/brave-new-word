using UnityEngine;
using Firebase;
using Firebase.Unity.Editor;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;

public class StartGameScript : MonoBehaviour {

    public const string DATA_PATH = "/BraveNewWord_playerData.dat";
    public GameObject settingsButton;
    public GameObject instructionsPanel;
    public GameObject settingsPanel;

    // use for initialization
    void Awake()
    {
        if (!GameManagerScript.DEBUG)
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
            Debug.Log("Touch supported!");
            TouchInputHandler.touchSupported = true;
        }
        else 
        {
            Debug.Log("Touch not supported!");
            TouchInputHandler.touchSupported = false;
        }

        // Firebase database logistics for editor
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://wordflood-bf7c4.firebaseio.com/");
        
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            Firebase.DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                    "Could not resolve all Firebase dependencies: " + dependencyStatus);
                GameManagerScript.LOGGING = false;
            }
        });

        // DEBUG MODE: Randomize the various frictional features of the game
        //RandomizeFeatures();

        // Load from file
        LoadFile();
    }

    void Update()
    {
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

    public void LoadFile()
    {
        if (File.Exists(Application.persistentDataPath + DATA_PATH))
        {
            // Read the file to load the frictional pattern data
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + DATA_PATH, FileMode.Open);

            PlayerData data = (PlayerData)bf.Deserialize(file);
            file.Close();

            //// set the local variables to the data from the file
            // TODO: uncomment this before deploy
            
            GameManagerScript.OBSTRUCTION_PRODUCTIVE = data.obstructionProductive;
            GameManagerScript.OBSTRUCTION_UNPRODUCTIVE = data.obstructionUnproductive;
            GameManagerScript.JUICE_PRODUCTIVE = data.juiceProductive;
            GameManagerScript.JUICE_UNPRODUCTIVE = data.juiceUnproductive;
            GameManagerScript.DISPLAY_TUTORIAL = false;
            GameManagerScript.INSTRUCTIONS_PANEL = data.instructions;
            GameManagerScript.userID = data.userID;
            GameManagerScript.myHighScore = data.myHighScore;
            GameManagerScript.gameNumber = data.gameNumber;

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
            // If file doesn't exist yet, randomize and initialize variables
            RandomizeFeatures();
        }

        Debug.Log("Obstruction Prod.: " + GameManagerScript.OBSTRUCTION_PRODUCTIVE);
        Debug.Log("Obstruction Unprod.: " + GameManagerScript.OBSTRUCTION_UNPRODUCTIVE);
        Debug.Log("Juice Prod.: " + GameManagerScript.JUICE_PRODUCTIVE);
        Debug.Log("Juice Unprod.: " + GameManagerScript.JUICE_UNPRODUCTIVE);
    }

    public void RandomizeFeatures() 
    {
        // If file doesn't exist yet, randomize and initialize variables
        // default to false
        GameManagerScript.OBSTRUCTION_PRODUCTIVE = false;
        GameManagerScript.OBSTRUCTION_UNPRODUCTIVE = false;
        GameManagerScript.JUICE_PRODUCTIVE = false;
        GameManagerScript.JUICE_UNPRODUCTIVE = false;

        // randomize the version
        int version = UnityEngine.Random.Range(0, 6);
        switch (version)
        {
            case 0:
                // everything remains off
                break;
            case 1:
                GameManagerScript.OBSTRUCTION_PRODUCTIVE = true;
                goto case 2;
            case 2:
                GameManagerScript.JUICE_PRODUCTIVE = true;
                break;
            case 3:
                GameManagerScript.OBSTRUCTION_UNPRODUCTIVE = true;
                goto case 4;
            case 4:
                GameManagerScript.JUICE_UNPRODUCTIVE = true;
                break;
            case 5:
                GameManagerScript.OBSTRUCTION_PRODUCTIVE = true;
                break;
            case 6:
                GameManagerScript.OBSTRUCTION_UNPRODUCTIVE = true;
                break;
        }

        // randomize userID using a GUID (UUID) 
        GameManagerScript.userID = Guid.NewGuid().ToString();
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

        auth.SignInAnonymouslyAsync().ContinueWith(task => {
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
