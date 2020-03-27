using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartOptions : MonoBehaviour
{
    public int sceneToStart = 1;										//Index number in build settings of scene to load if changeScenes is true
    public CanvasGroup fadeOutImageCanvasGroup;                         //Canvas group used to fade alpha of image which fades in before changing scenes
    public Image fadeImage;                                             //Reference to image used to fade out before changing scenes
    public float menuFadeTime = 0.2f;

    [HideInInspector] public bool inMainMenu = true;					//If true, pause button disabled in main menu (Cancel in input manager, default escape key)

    void Awake()
    {
        fadeImage.color = Color.black;
    }


    public void StartButtonClicked()
    {
        //Debug.Log("Start button clicked");

        //If changeScenes is true, start fading and change scenes halfway through animation when screen is blocked by FadeImage
        //Use invoke to delay calling of LoadDelayed by half the length of fadeColorAnimationClip
        Invoke("LoadDelayed", menuFadeTime);

        StartCoroutine(FadeCanvasGroupAlpha(0f, 1f, fadeOutImageCanvasGroup));
    }

    public void ButtonSound()
    {
        AudioManager.instance.Play("Sparkle1");
    }

    public void SelectName()
    {

    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += SceneWasLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneWasLoaded;
    }

    //Once the level has loaded, check if we want to call PlayLevelMusic
    void SceneWasLoaded(Scene scene, LoadSceneMode mode)
    {

    }

    public void LoadDelayed()
    {
        //Debug.Log("Loading next scene");

        //Pause button now works if escape is pressed since we are no longer in Main menu.
        inMainMenu = false;

        StartCoroutine(FadeCanvasGroupAlpha(1f, 0f, fadeOutImageCanvasGroup));

        //Debug.Log("Coroutine done. Next scene loaded!");

        //Load the selected scene, by scene index number in build settings
        SceneManager.LoadScene(sceneToStart);
    }

    public IEnumerator FadeCanvasGroupAlpha(float startAlpha, float endAlpha, CanvasGroup canvasGroupToFadeAlpha)
    {

        float elapsedTime = 0f;
        float totalDuration = menuFadeTime;

        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / totalDuration);
            canvasGroupToFadeAlpha.alpha = currentAlpha;
            yield return null;
        }
    }
}
