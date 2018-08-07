using UnityEngine;

public class TutorialButtonScript : MonoBehaviour {

    ConsentMenuScript menu;
    public int nextScene;

	// Use this for initialization
	void Start () {
        menu = gameObject.GetComponent<ConsentMenuScript>();
	}
	
    public void ClickPlayGameButton() {
        TouchInputHandler.touchEnabled = true;
        menu.GoToNextScene(nextScene);
    }
}
