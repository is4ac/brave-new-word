using UnityEngine;
using Firebase.Database;

public class TutorialButtonScript : MonoBehaviour
{

    ConsentMenuScript menu;
    public int nextScene;

    // Use this for initialization
    void Start()
    {
        // Turn off the instructions panel in the next scene
        GameManagerScript.INSTRUCTIONS_PANEL = false;

        menu = gameObject.GetComponent<ConsentMenuScript>();

        // Log the start of the Tutorial
        MetaLogEntry entry = new MetaLogEntry();
        entry.setValues("BNW_TutorialOpened", "BNW_Meta", new MetaLogEntry.MetaPayload("tutorial"));
        string json = JsonUtility.ToJson(entry);
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(GameManagerScript.LOGGING_VERSION);
        reference.Push().SetRawJsonValueAsync(json);

        //Debug.Log("logging tutorial");
    }

    public void ClickPlayGameButton()
    {
        // enable clicking in the next main game scene since the instructions text box will be skipped
        TouchInputHandler.touchEnabled = true;
        menu.GoToNextScene(nextScene);
    }
}
