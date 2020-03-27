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
        ob1Toggle.isOn = !(GameManagerScript.obstructionProductive || GameManagerScript.obstructionUnproductive);
        ob2Toggle.isOn = GameManagerScript.obstructionProductive;
        ob3Toggle.isOn = GameManagerScript.obstructionUnproductive;
        juice1Toggle.isOn = !(GameManagerScript.juiceProductive || GameManagerScript.juiceUnproductive);
        juice2Toggle.isOn = GameManagerScript.juiceProductive;
        juice3Toggle.isOn = GameManagerScript.juiceUnproductive;
    }

    public void NoButtonChanged(bool value)
    {
        if (value)
        {
            GameManagerScript.obstructionProductive = false;
            GameManagerScript.obstructionUnproductive = false;
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
            GameManagerScript.obstructionProductive = true;
            GameManagerScript.obstructionUnproductive = false;
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
            GameManagerScript.obstructionProductive = false;
            GameManagerScript.obstructionUnproductive = true;
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

            GameManagerScript.juiceProductive = false;
            GameManagerScript.juiceUnproductive = false;
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

            GameManagerScript.juiceProductive = true;
            GameManagerScript.juiceUnproductive = false;
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

            GameManagerScript.juiceProductive = false;
            GameManagerScript.juiceUnproductive = true;
            
        }
    }
}
