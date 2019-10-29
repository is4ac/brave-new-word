using UnityEngine;

public class SubmitPromptButtonScript : MonoBehaviour {

    public GameObject promptPanel;

    public void SubmitButtonClick() {
        // Play the currently selected word
        BoxScript.PlayWord();

        // Enable input to the rest of the screen
        TouchInputHandler.inputEnabled = true;

        // close window
        promptPanel.SetActive(false);
    }

    public void CancelButtonClick() {
        // Enable input to the rest of the screen
        TouchInputHandler.inputEnabled = true;

        // Close window without doing anything else
        promptPanel.SetActive(false);
    }

}
