using UnityEngine;

public class ShowPanels : MonoBehaviour {

    public GameObject instructionsPanel;        //Store a reference to the Game Object InstructionsPanel 
    public GameObject highScoresPanel;          //Store a reference to the Game Object HighScoresPanel    
    public GameObject settingsPanel;

    public void ShowInstructions()
    {
        instructionsPanel.SetActive(true);
    }

    public void HideInstructions()
    {
        instructionsPanel.SetActive(false);
    }

    public void ShowHighScores()
    {
        highScoresPanel.SetActive(true);
    }

    public void HideHighScores()
    {
        highScoresPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        settingsPanel.SetActive(true);
        TouchInputHandler.inputEnabled = false;
    }

    public void HideSettings()
    {
        settingsPanel.SetActive(false);
        if (GameManagerScript.GameHasStarted())
        {
            TouchInputHandler.inputEnabled = true;
        }
    }

    public void ButtonSound()
    {
        AudioManager.instance.Play("Sparkle1");
    }
}
