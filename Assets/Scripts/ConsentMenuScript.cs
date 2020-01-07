using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ConsentMenuScript : MonoBehaviour
{

    public int sceneToStart;                // the scene index to transition to after this scene
    public GameObject loadingPanel;

    // Use this for initialization
    void Start()
    {
        loadingPanel.SetActive(false);
    }

    public void ClickYes()
    {
        GameManagerScript.LOGGING = true;
        GoToNextScene(sceneToStart);
    }

    public void ClickNo()
    {
        GameManagerScript.LOGGING = false;
        GoToNextScene(sceneToStart);
    }

    public void GoToNextScene(int scene)
    {
        //If changeScenes is true, start fading and change scenes halfway through animation when screen is blocked by FadeImage
        if (scene < 2)
        {
            // Start loading the game scene
            StartCoroutine(LoadDelayed(scene));
        }
    }

    public IEnumerator LoadDelayed(int scene)
    {
        //Debug.Log("Loading next scene");

        loadingPanel.SetActive(true);

        //Load the selected scene in the background, by scene index number in build settings
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            // Update the loading text periods
            TMP_Text text = loadingPanel.GetComponentInChildren<TMP_Text>();
            if (text.text.LastIndexOf('.') < 9)
            {
                text.text += ".";
            }
            else
            {
                text.text = "Loading";
            }

            yield return new WaitForSeconds(.5f);
        }
    }
}
