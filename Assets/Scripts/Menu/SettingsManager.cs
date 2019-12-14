using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Toggle ob1Toggle;
    public Toggle ob2Toggle;
    public Toggle ob3Toggle;
    public Toggle juice1Toggle;
    public Toggle juice2Toggle;
    public Toggle juice3Toggle;

    private void Start()
    {
        SetToggles();
    }

    private void OnEnable()
    {
        SetToggles();
    }

    private void SetToggles()
    {
        ob1Toggle.isOn = !(GameManagerScript.OBSTRUCTION_PRODUCTIVE || GameManagerScript.OBSTRUCTION_UNPRODUCTIVE);
        ob2Toggle.isOn = GameManagerScript.OBSTRUCTION_PRODUCTIVE;
        ob3Toggle.isOn = GameManagerScript.OBSTRUCTION_UNPRODUCTIVE;
        juice1Toggle.isOn = !(GameManagerScript.JUICE_PRODUCTIVE || GameManagerScript.JUICE_UNPRODUCTIVE);
        juice2Toggle.isOn = GameManagerScript.JUICE_PRODUCTIVE;
        juice3Toggle.isOn = GameManagerScript.JUICE_UNPRODUCTIVE;
    }

    public void NoButtonChanged(bool value)
    {
        if (value)
        {
            GameManagerScript.OBSTRUCTION_PRODUCTIVE = false;
            GameManagerScript.OBSTRUCTION_UNPRODUCTIVE = false;
            if (GameManagerScript.gameManager != null)
            {
                GameManagerScript.gameManager.playButton.SetActive(false);
            }
        }
    }

    public void ButtonPromptChanged(bool value)
    {
        if (value)
        {
            GameManagerScript.OBSTRUCTION_PRODUCTIVE = true;
            GameManagerScript.OBSTRUCTION_UNPRODUCTIVE = false;
            if (GameManagerScript.gameManager != null)
            {
                GameManagerScript.gameManager.playButton.SetActive(true);
                GameManagerScript.gameManager.UpdatePlayButton();
            }
        }
    }

    public void ButtonNoSwipeChanged(bool value)
    {
        if (value)
        {
            GameManagerScript.OBSTRUCTION_PRODUCTIVE = false;
            GameManagerScript.OBSTRUCTION_UNPRODUCTIVE = true;
            if (GameManagerScript.gameManager != null)
            {
                GameManagerScript.gameManager.playButton.SetActive(true);
                GameManagerScript.gameManager.UpdatePlayButton();
            }
        }
    }

    public void NoJuiceChanged(bool value)
    {
        if (value)
        {
            if (GameManagerScript.gameManager != null)
            {
                GameManagerScript.gameManager.StopUnproductiveJuice();
            }

            // stop previous music and start new one
            AudioManager.instance.StopAndPlay("CalmTheme");

            GameManagerScript.JUICE_PRODUCTIVE = false;
            GameManagerScript.JUICE_UNPRODUCTIVE = false;
        }
    }

    public void ProductiveJuiceChanged(bool value)
    {
        if (value)
        {
            if (GameManagerScript.gameManager != null)
            {
                GameManagerScript.gameManager.StopUnproductiveJuice();
            }

            // stop previous music and start new one
            AudioManager.instance.StopAndPlay("JuicyTheme");

            GameManagerScript.JUICE_PRODUCTIVE = true;
            GameManagerScript.JUICE_UNPRODUCTIVE = false;
        }
    }

    public void UnproductiveJuiceChanged(bool value)
    {
        if (value)
        {
            if (GameManagerScript.gameManager != null)
            {
                GameManagerScript.gameManager.StartUnproductiveJuice();
            }

            // stop previous music and start new one
            AudioManager.instance.StopAndPlay("DubstepTheme");

            GameManagerScript.JUICE_PRODUCTIVE = false;
            GameManagerScript.JUICE_UNPRODUCTIVE = true;
            
        }
    }
}
