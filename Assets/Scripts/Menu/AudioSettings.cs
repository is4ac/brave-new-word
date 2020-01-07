using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider musicSlider;

    public void UpdateSliders(float master, float sfx, float music)
    {
        masterSlider.value = master;
        sfxSlider.value = sfx;
        musicSlider.value = music;
    }

    public void OnMasterVolumeSliderChange(float value)
    {
        mixer.SetFloat("masterVolume", value);
    }

    public void OnSFXVolumeSliderChange(float value)
    {
        mixer.SetFloat("sfxVolume", value);
    }

    public void OnMusicVolumeSliderChange(float value)
    {
        mixer.SetFloat("musicVolume", value);
    }
}
