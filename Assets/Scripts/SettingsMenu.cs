using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour {
    
    public GameObject settingsPanel;
    public GameObject uiDropdownGameObj;

    private TMP_Dropdown uiDropdown;
    private int myUIVersion;
    private int myNewUIVersion;

    private void Awake()
    {
        
    }

    // Use this for initialization
    void Start () {
        // initialize dropdown component
        uiDropdown = uiDropdownGameObj.GetComponent<TMP_Dropdown>();

        // get current UI version and set the dropdown to equal that
        myUIVersion = (int) GameManagerScript.currentVersion;
        myNewUIVersion = myUIVersion;
        uiDropdown.value = myUIVersion;
	}

    public void OpenSettingsMenu()
    {
        // TODO: Log this action through Firebase

        // TODO: deactivate touch input for the rest of the game
        TouchInputHandler.touchEnabled = false;

        // Open the settings panel
        settingsPanel.SetActive(true);
    }

    public void CancelMenu() {
        // TODO: Log this action

        // TODO: reactivate touch input for rest of game
        TouchInputHandler.touchEnabled = true;

        // TODO: save old settings and reset it before closing the panel
        myNewUIVersion = myUIVersion;
        uiDropdown.value = myUIVersion;

        // Close the settings panel without applying any of the changes (reset back)
        settingsPanel.SetActive(false);
    }

    public void SaveMenu() {
        // TODO: Log this action

        // TODO: reactivate touch input for rest of game
        TouchInputHandler.touchEnabled = true;

        // TODO: Save the changes of the new options
        GameManagerScript.currentVersion = (GameManagerScript.Versions) myNewUIVersion;

        // Close the settings panel
        settingsPanel.SetActive(false);
    }

    public void OnUIDropdownChange(int value) {
        // TODO: log this action

        // store the changed value in a variable
        myNewUIVersion = uiDropdown.value;
        Debug.Log("Dropdown changed: " + myNewUIVersion);
    }
}
