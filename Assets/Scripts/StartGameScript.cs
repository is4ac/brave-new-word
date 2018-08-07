using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class StartGameScript : MonoBehaviour {
    
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
            }
        });

        // Randomize the various frictional features of the game
        RandomizeFeatures();
	}

    public void RandomizeFeatures() 
    {
        GameManagerScript.DISPLAY_BUTTON = Random.Range(0, int.MaxValue) % 2 == 0;
        GameManagerScript.DISPLAY_TUTORIAL = Random.Range(0, int.MaxValue) % 2 == 0;
        GameManagerScript.DISPLAY_SELECTED_SCORE = Random.Range(0, int.MaxValue) % 2 == 0;
        GameManagerScript.DISPLAY_HIGHLIGHT_FEEDBACK = Random.Range(0, int.MaxValue) % 2 == 0;

        Debug.Log("Button: " + GameManagerScript.DISPLAY_BUTTON);
        Debug.Log("Tutorial: " + GameManagerScript.DISPLAY_TUTORIAL);
        Debug.Log("Selected Score: " + GameManagerScript.DISPLAY_SELECTED_SCORE);
        Debug.Log("Highlight: " + GameManagerScript.DISPLAY_HIGHLIGHT_FEEDBACK);
    }

    // Initialize the Firebase database:
    protected virtual void InitializeFirebase()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        // NOTE: You'll need to replace this url with your Firebase App's database
        // path in order for the database connection to work correctly in editor

        //This is needed only for the unity editor
        //app.SetEditorDatabaseUrl("https://wordflood-bf7c4.firebaseio.com/");
        if (app.Options.DatabaseUrl != null)
        {
            //app.SetEditorDatabaseUrl (app.Options.DatabaseUrl);
        }
        //--------------------------------------
        /*
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.SignInWithEmailAndPasswordAsync("isaacsung@gmail.com", "notasecret").ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });
        */
    }
}
