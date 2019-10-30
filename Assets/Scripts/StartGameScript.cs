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
            // TODO: uncomment this before deploy
            /*
            GameManagerScript.OBSTRUCTION_PRODUCTIVE = data.obstructionProductive;
            GameManagerScript.OBSTRUCTION_UNPRODUCTIVE = data.obstructionUnproductive;
            GameManagerScript.JUICE_PRODUCTIVE = data.juiceProductive;
            GameManagerScript.JUICE_UNPRODUCTIVE = data.juiceUnproductive;
            */
            GameManagerScript.DISPLAY_TUTORIAL = false;
            GameManagerScript.INSTRUCTIONS_PANEL = data.instructions;
            GameManagerScript.userID = data.userID;
            GameManagerScript.myHighScore = data.myHighScore;


            // TODO: DEBUG ONLY change back before release
            GameManagerScript.myHighScore = 0;
            GameManagerScript.userID = Guid.NewGuid().ToString();
            GameManagerScript.OBSTRUCTION_PRODUCTIVE = true;
            GameManagerScript.OBSTRUCTION_UNPRODUCTIVE = false;
            GameManagerScript.JUICE_PRODUCTIVE = false;
            GameManagerScript.JUICE_UNPRODUCTIVE = false;
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
        // TODO: make this more uniform!
        // If file doesn't exist yet, randomize and initialize variables
        GameManagerScript.OBSTRUCTION_PRODUCTIVE = UnityEngine.Random.Range(0, int.MaxValue) % 2 == 0;
        GameManagerScript.OBSTRUCTION_UNPRODUCTIVE = !GameManagerScript.OBSTRUCTION_PRODUCTIVE;
        GameManagerScript.JUICE_PRODUCTIVE = UnityEngine.Random.Range(0, int.MaxValue) % 2 == 0;
        // Only productive/productive and unproductive/unproductive combinations are allowed
        GameManagerScript.JUICE_UNPRODUCTIVE = !GameManagerScript.JUICE_PRODUCTIVE && GameManagerScript.OBSTRUCTION_UNPRODUCTIVE;
        GameManagerScript.JUICE_PRODUCTIVE = GameManagerScript.JUICE_PRODUCTIVE && GameManagerScript.OBSTRUCTION_PRODUCTIVE;

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
