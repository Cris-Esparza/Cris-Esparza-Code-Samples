using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System;

public class SettingsMenu : MonoBehaviour
{
    //Saves the audio mixer
    [SerializeField]
    public AudioMixer audioMixer;

    //Saves the resolution dropdown
    [SerializeField]
    public TMP_Dropdown resolutionDropdown;

    // saves the volume slider
    [SerializeField]
    Slider volumeSlider;

    // saves the fullscreen toggle
    [SerializeField]
    Toggle fullscreenToggle;

    [SerializeField]
    GameObject settingsMenu;

    [SerializeField]
    GameObject mainMenu;

    //Creates an array of resolutions
    Resolution[] resolutions;

    void Start()
    {
        //Saves resolutions into an array to show on resolution dropdown
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
		resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex");
		resolutionDropdown.RefreshShownValue();

        // set starting value for settings
        volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        fullscreenToggle.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("FullScreen"));
    }

    private void Update()
    {
        if (Input.GetAxisRaw("Pause") != 0)
        {
            settingsMenu.SetActive(!settingsMenu.active);
            mainMenu.SetActive(!mainMenu.active);
        }
    }

    /// <summary>
    /// Sets the resolution by taking in the index of the dropdown
    /// </summary>
    /// <param name="resolutionIndex"></param>
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
    }

    /// <summary>
    /// Sets the volume by taking in the float of the slider
    /// </summary>
    /// <param name="volume"></param>
    public void SetVolume(float volume)
    {
        if (volume != 0)
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20f);
        }
        else
        {
            audioMixer.SetFloat("MasterVolume", -80);
        }
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    /// <summary>
    /// Sets the quality of the game by taking in the index of the dropdown
    /// </summary>
    /// <param name="qualityIndex"></param>
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    /// <summary>
    /// Sets whether or not the game is fullscreen through the toggle's bool
    /// </summary>
    /// <param name="isFullscreen"></param>
    public void SetFullScreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("FullScreen", Convert.ToInt32(isFullscreen));
    }
}
