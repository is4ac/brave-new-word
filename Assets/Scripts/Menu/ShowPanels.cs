using UnityEngine;
using UnityEngine.UI;

public class ShowPanels : MonoBehaviour {

    public GameObject instructionsPanel;        //Store a reference to the Game Object InstructionsPanel 
    public GameObject highScoresPanel;          //Store a reference to the Game Object HighScoresPanel    

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
}
