using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConsentMenuScript : MonoBehaviour {

    public int sceneToStart;                // the scene index to transition to after this scene
    public GameObject loadingPanel;
    public GameObject loadingBar;

	// Use this for initialization
	void Start () {
        loadingPanel.SetActive(false);

        if (GameManagerScript.DISPLAY_TUTORIAL) {
            sceneToStart = 2;
        } else {
            sceneToStart = 3;
        }
	}

    public void ClickYes() {
        GameManagerScript.LOGGING = true;
        GoToNextScene(sceneToStart);
    }

    public void ClickNo() {
        GameManagerScript.LOGGING = false;
        GoToNextScene(sceneToStart);
    }

    public void GoToNextScene(int scene) {
        //If changeScenes is true, start fading and change scenes halfway through animation when screen is blocked by FadeImage
        if (scene != 1) 
        {
            // Start loading the game scene
            StartCoroutine(LoadDelayed(scene));
        } 
    }

    public IEnumerator LoadDelayed(int scene)
    {
        Debug.Log("Loading next scene");

        loadingPanel.SetActive(true);

        //Load the selected scene in the background, by scene index number in build settings
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);

        // Wait until the asynchronous scene fully loads
        while(!asyncLoad.isDone)
        {
            // Update the loading bar with the current progress
            loadingBar.transform.localScale = new Vector3(asyncLoad.progress, 1.0f, 1.0f);
            yield return null;
        }
    }
}
