using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class StartGameScript : MonoBehaviour {

    void Awake()
    {
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


    }

    // Use this for initialization
    void Start () {
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
	}

    public void RandomizeFeatures() 
    {
        GameManagerScript.DISPLAY_BUTTON = Random.Range(0, int.MaxValue) % 2 == 0;
        //GameManagerScript.DISPLAY_TUTORIAL = Random.Range(0, int.MaxValue) % 2 == 0;
        GameManagerScript.DISPLAY_TUTORIAL = false;
        GameManagerScript.DISPLAY_SELECTED_SCORE = Random.Range(0, int.MaxValue) % 2 == 0;
        GameManagerScript.DISPLAY_HIGHLIGHT_FEEDBACK = Random.Range(0, int.MaxValue) % 2 == 0;

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
