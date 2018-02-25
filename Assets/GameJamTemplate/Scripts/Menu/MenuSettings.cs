using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MenuSettings")]
public class MenuSettings : ScriptableObject
{

    public float menuFadeTime = .15f;
    public Color sceneChangeFadeColor = Color.white;
    [Header("Leave this at zero to start game in same scene as menu, otherwise set to scene index")]
    public int nextSceneIndex = 1;

    [Header("Add your menu music here")]
    public AudioClip mainMenuMusicLoop;
    [Header("If you want to play new music after Start is pressed, add it here")]
    public AudioClip musicLoopToChangeTo;

}
