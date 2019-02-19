using UnityEngine;

public class ButtonEvents : MonoBehaviour {
    ConsentMenuScript menu;

    public void playAgainButtonClick() {
        // open the survey link -- old code
        //Application.OpenURL("https://uwmadison.co1.qualtrics.com/jfe/form/SV_0JTMfAzNqLPJPVP");

        // Reset the game and start over
        GameManagerScript.gameManager.Reset();
    }

    public void mainMenuButtonClick()
    {
        // Reset the game and start over
        GameManagerScript.gameManager.Reset();

        // Change scenes back to main menu
        menu = gameObject.GetComponent<ConsentMenuScript>();

        menu.GoToNextScene(0);
    }
}
