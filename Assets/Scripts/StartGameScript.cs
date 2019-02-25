using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;

public class StartGameScript : MonoBehaviour {

    public const string DATA_PATH = "/playerData.dat";

    // use for initialization
    void Awake()
    {
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
        FirebaseApp.DefaultInstance.SetEditorP12FileName(@"Assets/WordFlood-66029aead4c6.p12");
        FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail("wordflood-unity-android@wordflood-bf7c4.iam.gserviceaccount.com");
        FirebaseApp.DefaultInstance.SetEditorP12Password("notasecret");

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

        // Randomize the various frictional features of the game
        //RandomizeFeatures();

        // Load from file
        LoadFile();
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
            GameManagerScript.DISPLAY_BUTTON = data.displayButton;
            GameManagerScript.DISPLAY_SELECTED_SCORE = data.displaySelectedScore;
            GameManagerScript.DISPLAY_HIGHLIGHT_FEEDBACK = data.displayHighlightFeedback;
            GameManagerScript.DISPLAY_TUTORIAL = false;
            GameManagerScript.INSTRUCTIONS_PANEL = data.instructions;
            GameManagerScript.userID = data.userID;
            GameManagerScript.myHighScore = data.myHighScore;

            // TODO: DEBUG ONLY change back before release
            //userID = Guid.NewGuid().ToString();
            //DISPLAY_BUTTON = true;
            //DISPLAY_SELECTED_SCORE = true;
            //DISPLAY_HIGHLIGHT_FEEDBACK = true;
            //DISPLAY_TUTORIAL = false;
        }
        else
        {
            // If file doesn't exist yet, randomize and initialize variables
            GameManagerScript.DISPLAY_BUTTON = UnityEngine.Random.Range(0, int.MaxValue) % 2 == 0;
            GameManagerScript.DISPLAY_TUTORIAL = false;
            GameManagerScript.DISPLAY_SELECTED_SCORE = UnityEngine.Random.Range(0, int.MaxValue) % 2 == 0;
            GameManagerScript.DISPLAY_HIGHLIGHT_FEEDBACK = UnityEngine.Random.Range(0, int.MaxValue) % 2 == 0;

            // randomize userID using a GUID (UUID) 
            GameManagerScript.userID = Guid.NewGuid().ToString();
        }

        Debug.Log("Button: " + GameManagerScript.DISPLAY_BUTTON);
        Debug.Log("Selected Score: " + GameManagerScript.DISPLAY_SELECTED_SCORE);
        Debug.Log("Highlight: " + GameManagerScript.DISPLAY_HIGHLIGHT_FEEDBACK);
    }

    public void RandomizeFeatures() 
    {
        GameManagerScript.DISPLAY_BUTTON = UnityEngine.Random.Range(0, int.MaxValue) % 2 == 0;
        //GameManagerScript.DISPLAY_TUTORIAL = Random.Range(0, int.MaxValue) % 2 == 0;
        GameManagerScript.DISPLAY_TUTORIAL = false;
        GameManagerScript.DISPLAY_SELECTED_SCORE = UnityEngine.Random.Range(0, int.MaxValue) % 2 == 0;
        GameManagerScript.DISPLAY_HIGHLIGHT_FEEDBACK = UnityEngine.Random.Range(0, int.MaxValue) % 2 == 0;

        Debug.Log("Button: " + GameManagerScript.DISPLAY_BUTTON);
        Debug.Log("Tutorial: " + GameManagerScript.DISPLAY_TUTORIAL);
        Debug.Log("Selected Score: " + GameManagerScript.DISPLAY_SELECTED_SCORE);
        Debug.Log("Highlight: " + GameManagerScript.DISPLAY_HIGHLIGHT_FEEDBACK);

        // Change the scene to start (show tutorial or not) depending on DISPLAY_TUTORIAL
        StartOptions startOptions = gameObject.GetComponent<StartOptions>();
        startOptions.sceneToStart = 2;

        //if (GameManagerScript.DISPLAY_TUTORIAL)
        //{
        //    startOptions.sceneToStart = 1;
        //}
        //else
        //{
        //    startOptions.sceneToStart = 2;
        //}
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
