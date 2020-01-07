using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Audio;

public class Logger : MonoBehaviour
{
    public static void Log(LogEntry entry)
    {
        string json = JsonUtility.ToJson(entry);
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(GameManagerScript.LOGGING_VERSION);
        reference.Push().SetRawJsonValueAsync(json);
    }

    public static void LogAudioSettings()
    {
        if (!GameManagerScript.LOGGING) return;

        AudioMixer mixer = Resources.Load("MainAudioMixer") as AudioMixer;
        if (mixer != null)
        {
            // retrieve audio mixer volume values
            mixer.GetFloat("masterVolume", out float master);
            mixer.GetFloat("sfxVolume", out float sfx);
            mixer.GetFloat("musicVolume", out float music);

            // Log audio volume settings
            AudioSettingsLogEntry entry = new AudioSettingsLogEntry();
            AudioSettingsLogEntry.AudioSettingsPayload payload = new AudioSettingsLogEntry.AudioSettingsPayload
            {
                masterVol = master,
                sfxVol = sfx,
                musicVol = music
            };

            // push to database
            entry.setValues("BNW_AudioSettings", "BNW_Action", payload);
            Log(entry);
        }
        else
        {
            //Debug.Log("Error: Could not load AudioMixer");
        }
    }

    public static void LogInstructionsClick(Vector2 pos)
    {
        if (!GameManagerScript.LOGGING) return;

        // log the location of the click
        ClickLogEntry entry = new ClickLogEntry();
        ClickLogEntry.ClickPayload payload = new ClickLogEntry.ClickPayload
        {
            screenX = pos.x,
            screenY = pos.y
        };

        entry.setValues("BNW_InstructionsClosed", "BNW_Action", payload);
        Log(entry);
    }

    public static void LogStartOfGame()
    {
        if (!GameManagerScript.LOGGING) return;

        //Debug.Log("Logging beginning of game");

        // Log beginning of game
        MetaLogEntry entry = new MetaLogEntry();
        entry.setValues("BNW_GameStart", "BNW_Meta", new MetaLogEntry.MetaPayload("start"));
        Log(entry);

        //Debug.Log("logging game info");

        // insert new game entry into database
        var reference = FirebaseDatabase.DefaultInstance.GetReference(GameManagerScript.LOGGING_VERSION + "_games");
        DatabaseReference child = reference.Child(GameManagerScript.GAME_ID);

        // Log the game details and type of game
        child.Child("obstructionProductive").SetValueAsync(GameManagerScript.OBSTRUCTION_PRODUCTIVE);
        child.Child("obstructionUnproductive").SetValueAsync(GameManagerScript.OBSTRUCTION_UNPRODUCTIVE);
        child.Child("juiceProductive").SetValueAsync(GameManagerScript.JUICE_PRODUCTIVE);
        child.Child("juiceUnproductive").SetValueAsync(GameManagerScript.JUICE_UNPRODUCTIVE);
        child.Child("username").SetValueAsync(GameManagerScript.username);
        child.Child("gameID").SetValueAsync(GameManagerScript.GAME_ID);
        child.Child("userID").SetValueAsync(GameManagerScript.userID);
        child.Child("loggingVersion").SetValueAsync(GameManagerScript.LOGGING_VERSION);
        child.Child("appVersion").SetValueAsync(GameManagerScript.APP_VERSION);
        child.Child("gameNumber").SetValueAsync(GameManagerScript.gameNumber);

        // Log username into users database if it doesn't already exist or if the username has changed
        DatabaseReference dbUsers = FirebaseDatabase.DefaultInstance.GetReference("users_" + GameManagerScript.VERSION);

        dbUsers.GetValueAsync().ContinueWith(task =>
           {
               if (task.IsFaulted)
               {
                   // ERROR HANDLER
                   Debug.LogWarning("Error in logging user into users database of GameManagerScript");
               }
               else if (task.IsCompleted)
               {
                   Dictionary<string, object> results = (Dictionary<string, object>)task.Result.Value;

                   // set the value of userID's username if it doesn't exist or if it changed
                   if (results == null || !results.ContainsKey(GameManagerScript.userID) || !results[GameManagerScript.userID].Equals(GameManagerScript.username))
                   {
                       dbUsers.Child(GameManagerScript.userID).SetValueAsync(GameManagerScript.username);
                   }
               }
           }
        );
    }

    public static void LogKeyFrame(string state)
    {
        if (!GameManagerScript.LOGGING) return;

        //Debug.Log("Logging full game state");

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
        Log(entry);
    }

    public static void LogEndOfGame()
    {
        if (!GameManagerScript.LOGGING) return;

        //Debug.Log("Logging end of game");

        // log the end game state when the player finished the round
        LogKeyFrame("gameEnd");

        // log the end of the game
        MetaLogEntry metaEntry = new MetaLogEntry();
        metaEntry.setValues("BNW_GameEnd", "BNW_Meta", new MetaLogEntry.MetaPayload("end"));
        Log(metaEntry);
    }

    /**
	 * User or the OS force quits the application
	 */
    public static void LogGamePause()
    {
        if (!GameManagerScript.LOGGING) return;
        //Debug.Log("Application pausing after " + Time.time + " seconds");

        // log the game state before game pauses
        LogKeyFrame("gamePause");

        // log the pause
        MetaLogEntry metaEntry = new MetaLogEntry();
        metaEntry.setValues("BNW_GamePaused", "BNW_Meta", new MetaLogEntry.MetaPayload("pause"));
        Log(metaEntry);
    }

    public static void LogGameUnpause()
    {
        if (!GameManagerScript.LOGGING) return;

        // log the unpause
        MetaLogEntry metaEntry = new MetaLogEntry();
        metaEntry.setValues("BNW_GameUnpaused", "BNW_Meta", new MetaLogEntry.MetaPayload("unpause"));
        Log(metaEntry);
    }

    // BNW_LetterSelected or BNW_LetterDeselected logging
    public static void LogAction(string key, Vector2 pos)
    {
        if (!GameManagerScript.LOGGING) return;

        //Debug.Log("Attempts to log data");
        string letter = BoxScript.grid[(int)pos.x, (int)pos.y].gameObject.GetComponent<BoxScript>().Letter;
        LogEntry.LetterPayload payload = new LogEntry.LetterPayload();
        payload.setValues(letter, (int)pos.x, (int)pos.y);
        LetterLogEntry entry = new LetterLogEntry();
        entry.setValues(key, "BNW_Action", payload);
        Log(entry);

        // TODO: this needs to happen elsewhere
        ++BoxScript.totalInteractions;
    }

    // BNW_LetterSelected or BNW_LetterDeselected logging
    public static void LogAction(string key, string letter, int x, int y)
    {
        if (!GameManagerScript.LOGGING) return;

        //Debug.Log("Attempts to log data");
        LogEntry.LetterPayload payload = new LogEntry.LetterPayload();
        payload.setValues(letter, x, y);

        LetterLogEntry entry = new LetterLogEntry();
        entry.setValues(key, "BNW_Action", payload);
        Log(entry);

        ++BoxScript.totalInteractions;
    }

    // BNW_DeselectAll logging
    public static void LogAction(string key)
    {
        if (!GameManagerScript.LOGGING) return;

        LogEntry.LetterPayload[] letters = BoxScript.GetLetterPayloadsFromCurrentWord();
        DeselectWordLogEntry.DeselectWordPayload wordPayload = new DeselectWordLogEntry.DeselectWordPayload();
        wordPayload.word = BoxScript.currentWord;
        wordPayload.letters = letters;

        DeselectWordLogEntry entry = new DeselectWordLogEntry();
        entry.setValues(key, "BNW_Action", wordPayload);
        Log(entry);

        ++BoxScript.totalInteractions;
    }
}
