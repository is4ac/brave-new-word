using Firebase.Database;
using UnityEngine;

public class SubmitPromptButtonScript : MonoBehaviour
{

    public GameObject promptPanel;

    public void SubmitButtonClick()
    {
        // Play the currently selected word
        BoxScript.PlayWord();

        // Enable input to the rest of the screen
        TouchInputHandler.inputEnabled = true;

        // close window
        promptPanel.SetActive(false);

        ResetSubmitPromptTimer();
    }

    public void CancelButtonClick()
    {
        // Enable input to the rest of the screen
        TouchInputHandler.inputEnabled = true;

        // Log the cancel button click
        LogCancelButtonClick();

        // Close window without doing anything else
        promptPanel.SetActive(false);

        ResetSubmitPromptTimer();
    }

    public void ResetSubmitPromptTimer()
    {
        GameManagerScript.submitPromptOn = false;
        GameManagerScript.submitPromptTimer = 0f;
    }

    public void LogCancelButtonClick()
    {
        if (GameManagerScript.LOGGING)
        {
            CancelPlayWordLogEntry entry = new CancelPlayWordLogEntry();
            CancelPlayWordLogEntry.CancelPlayWordPayload payload =
                new CancelPlayWordLogEntry.CancelPlayWordPayload(
                    BoxScript.currentWord,
                    BoxScript.GetWordRank(BoxScript.currentWord),
                    BoxScript.GetScore(BoxScript.currentWord, null)
                );

            entry.setValues("BNW_CancelButton", "BNW_Action", payload);
            string json = JsonUtility.ToJson(entry);
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(GameManagerScript.LOGGING_VERSION);
            DatabaseReference child = reference.Push();
            child.SetRawJsonValueAsync(json);

            BoxScript.totalInteractions++;
        }
    }
}
