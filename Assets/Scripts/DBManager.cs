using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;

public class DbManager : MonoBehaviour
{
    public static event Action<string> OnUsernameChange = delegate { };
    
    public static DbManager instance;
    public StartGameScript startGameScript;

    // name of device ID db
    private static readonly string DevicesDbName = "devices_" + GameManagerScript.VERSION;
    
    // name of users db
    private static readonly string UsersDbName = "users_" + GameManagerScript.VERSION;
    
    //public delegate void ScoreAction();
    //public static event ScoreAction TopScoreUpdated;

    //public long topScore;

    //public string topUser = "";
    //private long curScore;

    //public Dictionary<string, long> userToScore;

    //[FormerlySerializedAs("userIDToUsernames")]
    //public Dictionary<string, string> userIdToUsernames;

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

    private void SetGameValuesFromDb(Dictionary<string, object> values, string deviceID)
    {
        Debug.Log("SetGameValuesFromDb: yeah");
        if (values.ContainsKey("userID"))
        {
            GameManagerScript.userId = (string) values["userID"];
        }
        else
        {
            GameManagerScript.userId = Guid.NewGuid().ToString();
            var dbDevices = FirebaseDatabase.DefaultInstance.GetReference(DevicesDbName);
            dbDevices.Child(deviceID).Child("userID").SetValueAsync(GameManagerScript.userId);
        }
        
        Debug.Log("SetGameValuesFromDb: loaded userID: " + GameManagerScript.userId);
        
        GameManagerScript.juiceProductive = (bool) values["juiceProductive"];
        GameManagerScript.juiceUnproductive = (bool) values["juiceUnproductive"];
        GameManagerScript.obstructionProductive = (bool) values["obstructionProductive"];
        GameManagerScript.obstructionUnproductive = (bool) values["obstructionUnproductive"];
        
        // set the username if it is available
        SetUsernameFromDb(GameManagerScript.userId);
    }

    private void SetUsernameFromDb(string userId)
    {
        var dbUsers = FirebaseDatabase.DefaultInstance.GetReference(UsersDbName);
        dbUsers.OrderByKey().EqualTo(userId).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // handle error
                Debug.Log("SetUsernameFromDb db retrieval failed.");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Value != null)
                {
                    // successfully retrieved userID data snapshot
                    var values = (Dictionary<string, object>) snapshot.Value;
                    var user = (Dictionary<string, object>) values[userId];
                    if (user.ContainsKey("username"))
                    {
                        OnUsernameChange((string) user["username"]);
                    }
                }
                else
                {
                    // userID does not exist in db
                    // Don't do anything?
                    Debug.Log("SetUsernameFromDb: userID does not exist.");
                }
                
                Logger.LogUser();
            }
        });
    }

    private void SetDbValuesFromGame(DatabaseReference reference)
    {
        Debug.Log("SetDbValuesFromGame: Setting db devicesID values");
        reference.Child("userID").SetValueAsync(GameManagerScript.userId);
        reference.Child("juiceProductive").SetValueAsync(GameManagerScript.juiceProductive);
        reference.Child("juiceUnproductive").SetValueAsync(GameManagerScript.juiceUnproductive);
        reference.Child("obstructionProductive").SetValueAsync(GameManagerScript.obstructionProductive);
        reference.Child("obstructionUnproductive").SetValueAsync(GameManagerScript.obstructionUnproductive);
        Logger.LogUser();
    }

    public void InitializeDeviceId()
    {
        Debug.Log("InitializeDeviceId: Initializing device id");
        string deviceID = DeviceIDManager.GetDeviceID();
        Debug.Log("InitializeDeviceId: deviceID: " + deviceID);
        DatabaseReference dbDevices = FirebaseDatabase.DefaultInstance.GetReference(DevicesDbName);
        dbDevices.OrderByKey().EqualTo(deviceID).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Handle error
                Debug.Log("InitializeDeviceId db retrieval failed.");
                
                // randomize features i guess
                startGameScript.SetRandomize();
                SetDbValuesFromGame(dbDevices.Child(deviceID));
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Value != null)
                {
                    // check to see if deviceID exists in DB.
                    // If it does, grab the userID and game version from the DB.
                    var values = (Dictionary<string, object>) snapshot.Value;
                    SetGameValuesFromDb((Dictionary<string, object>) values[deviceID], deviceID);
                }
                else
                {
                    // If deviceID doesn't exist yet, randomize and initialize variables
                    // and add deviceID to DB
                    //startGameScript.SetRandomize();
                    SetDbValuesFromGame(dbDevices.Child(deviceID));
                }
            }
        });
    }

    public void CheckDeviceId()
    {
        Debug.Log("CheckDeviceId: Checking device id");
        string deviceID = DeviceIDManager.GetDeviceID();
        Debug.Log("CheckDeviceId: deviceID: " + deviceID);
        DatabaseReference dbDevices = FirebaseDatabase.DefaultInstance.GetReference(DevicesDbName);
        dbDevices.OrderByKey().EqualTo(deviceID).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Handle error
                Debug.Log("CheckDeviceId db retrieval failed.");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Value != null)
                {
                    // Check deviceId db. Update game values to match that of db.
                    var values = (Dictionary<string, object>) snapshot.Value;
                    SetGameValuesFromDb((Dictionary<string, object>) values[deviceID], deviceID);
                }
                else
                {
                    // If deviceID doesn't exist yet, then add deviceID to DB
                    SetDbValuesFromGame(dbDevices.Child(deviceID));
                }
            }
        });
    }

    /*
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
            Dictionary<string, object> updatedScore = md.Value as Dictionary<string, object>;
            topScore = (long)updatedScore["topScore"];
            topUser = (string)updatedScore["topUser"];
        }

        // Compare the cur score to the top score.
        if (curScore > topScore)
        { // Update topScore, triggers other UpdateTopScores to retry
            topScore = curScore;
            md.Value = new Dictionary<string, object>(){
                {"topScore", curScore},
                {"topUser", GameManagerScript.userID}
            };
            return TransactionResult.Success(md);
        }

        return TransactionResult.Abort(); // Aborts the transaction
    }

    void HandleTopScoreChange(object sender, ValueChangedEventArgs args)
    {
        Dictionary<string, object> update = (Dictionary<string, object>)args.Snapshot.Value;
        topScore = (long)update["topScore"];
        topUser = (string)update["topUser"];
        Debug.Log("New Top Score: " + topScore);
        Debug.Log("New Top UserID: " + topUser);
        if (TopScoreUpdated != null) TopScoreUpdated();
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
                    DatabaseReference usersDb = FirebaseDatabase.DefaultInstance.GetReference(usersDbName);
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
    */
}