using UnityEngine;
using UnityEngine.UI;
using EZCameraShake;
using Firebase.Database;

public class PlayButtonScript : MonoBehaviour {

    public GameObject submitPromptPanel;
    public Text promptText;
    public Text rarityText;
    public Text pointsText;
	
    public void PlayButtonClick() {
        // increase number of interactions
        BoxScript.totalInteractions++;

        // Play sound
        AudioManager.instance.Play("Sparkle1");

        //=========FEATURE: Unproductive Juice=================================
        // CAMERA SHAKE when pressing button
        //=====================================================================
        if (GameManagerScript.juiceUnproductive)
        {
            CameraShaker.instance.ShakeOnce(3f, 4f, .1f, .6f);
        }

        //=========FEATURE: Productive Obstruction=============================
        // Display prompt if productive
        //=====================================================================
        if (GameManagerScript.obstructionProductive)
        {
            // get score
            long score = BoxScript.GetScore(BoxScript.currentWord, null);

            // get rarity
            float rarity = BoxScript.GetWordRank(BoxScript.currentWord);
            if (rarity < 0) {
                rarity = 0;
            }

            // update text
            promptText.text = "Are you sure you want to submit "
                + BoxScript.currentWord + "?";
            rarityText.text = "Rarity: " + (rarity * 100).ToString("0.00") + "%";
            pointsText.text = "Points: " + score;

            submitPromptPanel.SetActive(true);

            // Turn on the timer for logging
            GameManagerScript.submitPromptOn = true;

            // Disable touch of the rest of the screen
            TouchInputHandler.inputEnabled = false;

            // Log the action
            LogPlayButtonClick(BoxScript.currentWord, rarity, score);
        } else {
            BoxScript.PlayWord();
        }
    }

    public void LogPlayButtonClick(string word, float freq, long score)
    {
        if (GameManagerScript.logging)
        {
            ClickPlayWordButtonLogEntry.ClickPlayWordButtonPayload payload =
                new ClickPlayWordButtonLogEntry.ClickPlayWordButtonPayload(
                    word,
                    freq,
                    score
                );
            ClickPlayWordButtonLogEntry entry = new ClickPlayWordButtonLogEntry();
            entry.SetValues("BNW_ClickPlayWord", "BNW_Action", payload);
            string json = JsonUtility.ToJson(entry);
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(GameManagerScript.LOGGING_VERSION);
            DatabaseReference child = reference.Push();
            child.SetRawJsonValueAsync(json);
        }
    }
}
