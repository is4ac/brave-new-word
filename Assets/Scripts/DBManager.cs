using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;

public class DBManager : MonoBehaviour 
{
	public static DBManager instance;

    // edit this to a new version whenever the game has changed 
    // so much that it needs a new high score list
    // TODO: update this to next version number before major release
    public static string versionNumber = "_0_1";
    public static string scoresDbName = "scores" + versionNumber;

    private DatabaseReference dbScores;

    public delegate void ScoreAction();
    public static event ScoreAction TopScoreUpdated;

	public long topScore = 0;
    public string topUser = "";
	private long curScore = 0;

    public Dictionary<string, long> userToScore;
    public Dictionary<string, string> userIDToUsernames;

    // Awake at the beginning, used for initialization
	void Awake()
    {
		if (instance == null)
        {
			instance = this;
		}
        else
        {
            Destroy(gameObject);
        }
	}

    void Start()
    {
        if (GameManagerScript.LOGGING)
        {
            // reference database from the appropriate scores entry
            dbScores = FirebaseDatabase.DefaultInstance.GetReference(scoresDbName);

            userToScore = new Dictionary<string, long>();
            userIDToUsernames = new Dictionary<string, string>();

            // Get top score, listen for changes.
            GetTopScore();
            dbScores.ValueChanged += HandleTopScoreChange;

            // Load high scores
            RetrieveTopScores();
        }
    }

    private void GetTopScore() 
    {
        if (GameManagerScript.LOGGING)
        {
            dbScores.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    // ERROR HANDLER
                    Debug.Log("Error in GetTopScore() of DBManager");
                }
                else if (task.IsCompleted)
                {
                    Dictionary<string, object> results = (Dictionary<string, object>)task.Result.Value;

                    // initialize topScore if it doesn't exist yet
                    if (results == null || !results.ContainsKey("topScore") || !results.ContainsKey("topUser"))
                    {
                        dbScores.Child("topUser").SetValueAsync("");
                        dbScores.Child("topScore").SetValueAsync(0);
                        LogScore(curScore);
                    }
                    else
                    {
                        topScore = (long)results["topScore"];
                        topUser = (string)results["topUser"];
                    }

                    // get user's personal high score, or initialize it if necessary
                    if (results == null || !results.ContainsKey(GameManagerScript.userID))
                    {
                        dbScores.Child(GameManagerScript.userID).SetValueAsync(0);
                    }
                    else
                    {
                        long dbScore = (long)results[GameManagerScript.userID];
                        if (GameManagerScript.myHighScore < dbScore)
                        {
                            GameManagerScript.myHighScore = dbScore;
                        }
                        else
                        {
                            dbScores.Child(GameManagerScript.userID).SetValueAsync(GameManagerScript.myHighScore);
                        }
                    }
                }
            });
        }
    }

	public void LogScore(long s) 
    {
        if (GameManagerScript.LOGGING)
        {
            curScore = s;

            // update global top score
            if (curScore > topScore)
            {
                dbScores.RunTransaction(UpdateTopScore);
                GameManagerScript.globalHighScoreUpdated = true;
            }

            // update local high score
            if (curScore > GameManagerScript.myHighScore)
            {
                UpdateLocalHighScore();
                GameManagerScript.myHighScoreUpdated = true;
            }
        }
    }

    void UpdateLocalHighScore()
    {
        if (GameManagerScript.LOGGING)
        {
            GameManagerScript.myHighScore = curScore;
            dbScores.Child(GameManagerScript.userID).SetValueAsync(curScore);
        }
    }

    private TransactionResult UpdateTopScore(MutableData md) 
    {
        if (md.Value != null) 
        {
            Dictionary<string,object> updatedScore = md.Value as Dictionary<string,object>;
            topScore = (long) updatedScore ["topScore"];
            topUser = (string)updatedScore["topUser"];
        }

        // Compare the cur score to the top score.
        if (curScore > topScore) 
        { // Update topScore, triggers other UpdateTopScores to retry
            topScore = curScore;
            md.Value = new Dictionary<string,object>(){
                {"topScore", curScore},
                {"topUser", GameManagerScript.userID}
            };
            return TransactionResult.Success(md);
        }

        return TransactionResult.Abort (); // Aborts the transaction
    }

    void HandleTopScoreChange(object sender, ValueChangedEventArgs args) 
    {
        Dictionary<string,object> update = (Dictionary<string,object>)args.Snapshot.Value;
        topScore = (long) update["topScore"];
        topUser = (string)update["topUser"];
        Debug.Log ("New Top Score: " + topScore);
        Debug.Log("New Top UserID: " + topUser);
        if (TopScoreUpdated != null) TopScoreUpdated ();
    }

    public void RetrieveTopScores()
    {
        if (GameManagerScript.LOGGING)
        {
            dbScores.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                // ERROR HANDLER
                Debug.Log("Error in RetrieveTopScores() of DBManager");
                }
                else if (task.IsCompleted)
                {
                    Dictionary<string, object> results = (Dictionary<string, object>)task.Result.Value;

                // retrieve the top scores and userIDs
                if (results == null || !results.ContainsKey("topScore") || !results.ContainsKey("topUser"))
                    {
                        dbScores.Child("topUser").SetValueAsync("");
                        LogScore(curScore);
                    }
                    else
                    {
                        foreach (KeyValuePair<string, object> entry in results)
                        {
                            if (!entry.Key.Equals("topScore") && !entry.Key.Equals("topUser"))
                            {
                                userToScore[entry.Key] = (long)entry.Value;
                            }
                        }
                    }

                // retrieve the userIDs and usernames
                DatabaseReference usersDb = FirebaseDatabase.DefaultInstance.GetReference(GameManagerScript.usersDbName);
                    usersDb.GetValueAsync().ContinueWith(task2 =>
                    {
                        if (task2.IsFaulted)
                        {
                        // ERROR HANDLER
                        Debug.Log("Error in RetrieveTopScores() of DBManager");
                        }
                        else if (task2.IsCompleted)
                        {
                            Dictionary<string, object> results2 = (Dictionary<string, object>)task2.Result.Value;

                        // retrieve the top scores and userIDs
                        if (results2 == null)
                            {
                            // skip
                        }
                            else
                            {
                                foreach (KeyValuePair<string, object> entry in results2)
                                {
                                    userIDToUsernames[entry.Key] = (string)entry.Value;
                                }
                            }

                        // update and display the high scores 
                        HighScoreDisplay.instance.UpdateHighScores();
                            HighScoreDisplay.instance.DisplayHighScores();
                        }
                    });
                }
            });
        }
    }
}
