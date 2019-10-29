using UnityEngine;
using UnityEngine.UI;

public class PlayButtonScript : MonoBehaviour {

    public GameObject submitPromptPanel;
    public Text promptText;
    public Text rarityText;
    public Text pointsText;
	
    public void PlayButtonClick() {
        // Display prompt if productive
        if (GameManagerScript.OBSTRUCTION_PRODUCTIVE)
        {
            // get rarity
            float rarity = BoxScript.GetWordRank(BoxScript.currentWord);
            if (rarity < 0) {
                rarity = 0;
            }

            // update text
            promptText.text = "Are you sure you want to submit "
                + BoxScript.currentWord + "?";
            rarityText.text = "Rarity: " + (rarity * 100).ToString("0.00") + "%";
            pointsText.text = "Points: " + BoxScript.GetScore(BoxScript.currentWord, null);

            submitPromptPanel.SetActive(true);

            // Disable touch of the rest of the screen
            TouchInputHandler.inputEnabled = false;
        } else {
            BoxScript.PlayWord();
        }
    }
}
