using Firebase.Database;
using UnityEngine;

public class ButtonEvents : MonoBehaviour
{
    ConsentMenuScript menu;

    public void PlayAgainButtonClick()
    {
        // open the survey link -- old code
        //Application.OpenURL("https://uwmadison.co1.qualtrics.com/jfe/form/SV_0JTMfAzNqLPJPVP");

        // Reset the game and start over
        GameManagerScript.gameManager.Reset();

        AudioManager.instance.Play("Sparkle1");
    }

    public void MainMenuButtonClick()
    {
        // Reset the game and start over
        GameManagerScript.gameManager.Reset();

        AudioManager.instance.Play("Sparkle1");

        // Change scenes back to main menu
        menu = gameObject.GetComponent<ConsentMenuScript>();

        menu.GoToNextScene(0);
    }

    public void LogPlayAgainButton()
    {
        // Log the play again button click
        if (GameManagerScript.LOGGING)
        {
            LogEntry log = new LogEntry();
            log.setValues("BNW_PlayAgainButtonClick", "BNW_Action");
            string json = JsonUtility.ToJson(log);
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(GameManagerScript.LOGGING_VERSION);
            DatabaseReference child = reference.Push();
            child.SetRawJsonValueAsync(json);
        }
    }
}
