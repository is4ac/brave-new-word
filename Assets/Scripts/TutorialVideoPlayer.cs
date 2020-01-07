using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialVideoPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public GameObject loadingText;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(PlayVideo());
    }

    IEnumerator PlayVideo()
    {
        videoPlayer.Prepare();

        //Wait until video is prepared
        while (!videoPlayer.isPrepared)
        {
            //Debug.Log("Preparing Video");
            yield return null;
        }

        loadingText.SetActive(false);
        videoPlayer.Play();
        rawImage.texture = videoPlayer.texture;
    }

    public void PlayGameButtonHandler()
    {

    }
}
