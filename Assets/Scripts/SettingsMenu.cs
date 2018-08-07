using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour {
    
    public GameObject settingsPanel;
    //public GameObject uiDropdownGameObj;
    public GameObject highlightToggleObj;
    public GameObject currentSelectedScoreToggleObj;
    public GameObject buttonToggleObj;

    //private TMP_Dropdown uiDropdown;
    private Toggle highlightToggle;
    private Toggle currentSelectedScoreToggle;
    private Toggle buttonToggle;

    // Use this for initialization
    void Start () {
        // initialize components
        //uiDropdown = uiDropdownGameObj.GetComponent<TMP_Dropdown>();
        highlightToggle = highlightToggleObj.GetComponent<Toggle>();
        currentSelectedScoreToggle = currentSelectedScoreToggleObj.GetComponent<Toggle>();
        buttonToggle = buttonToggleObj.GetComponent<Toggle>();

        // get current UI version and set the dropdown to equal that
        //uiDropdown.value = (int)GameManagerScript.currentVersion;

        // set the toggles to be whatever options they currently are
        highlightToggle.isOn = GameManagerScript.DISPLAY_HIGHLIGHT_FEEDBACK;
        currentSelectedScoreToggle.isOn = GameManagerScript.DISPLAY_SELECTED_SCORE;
        buttonToggle.isOn = GameManagerScript.DISPLAY_BUTTON;
    }

    public void OpenSettingsMenu()
    {
        // TODO: Log this action through Firebase

        // TODO: deactivate touch input for the rest of the game
        TouchInputHandler.touchEnabled = false;

        // deselect all currently selected letters (just to make it easier for highlighting feature changes, etc
        BoxScript.ClearAllSelectedTiles();

        // Open the settings panel
        settingsPanel.SetActive(true);
    }

    public void CloseSettingsMenu() {
        // TODO: Log this action


        // Close the settings panel
        settingsPanel.SetActive(false);

        // reactivate touch input for rest of game
        TouchInputHandler.touchEnabled = true;
    }

    public void OnHighlightToggleChange(bool value) {
        // TODO: log this action

        GameManagerScript.DISPLAY_HIGHLIGHT_FEEDBACK = value;
    }

    public void OnCurrentSelectedScoreToggleChange(bool value) {
        // TODO: log this action

        GameManagerScript.DISPLAY_SELECTED_SCORE = value;
    }

    public void OnButtonToggleChange(bool value)
    {
        // TODO: log this action

        GameManagerScript.DISPLAY_BUTTON = value;
        GameManagerScript gameManager = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
        gameManager.SetButtonDisplay(value);
    }
}
