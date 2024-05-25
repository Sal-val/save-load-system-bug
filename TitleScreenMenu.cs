using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class TitleScreenMenu : Menu
{
    [Header("Menu Navigation")]
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private OptionsMenu optionsMenu;
    [SerializeField] private ConfirmationPopupMenu confirmationPopupMenu;
    [SerializeField] private MusicSlider _musicSlider;
    [SerializeField] private SFXSlider _SFXSlider;
    [SerializeField] private VoiceSlider _voiceSlider;
    private float _musicVolumeSlider;
    private float _SFXVolumeSlider;
    private float _voiceVolumeSlider;

    private void Start()
    {
        int currentRes = PlayerPrefs.GetInt("Resolution", 0);
        switch (currentRes)
        {
            case 0: Screen.SetResolution(1920, 1080, true); break;
            case 1: Screen.SetResolution(2560, 1440, true); break;
            case 2: Screen.SetResolution(3840, 2160, true); break;
        }

        PlayerController.Instance.isGamePaused = true;

        _musicVolumeSlider = PlayerPrefs.GetFloat("MusicVolume", 0);
        _SFXVolumeSlider = PlayerPrefs.GetFloat("SFXVolume", 0);
        _voiceVolumeSlider = PlayerPrefs.GetFloat("VoiceVolume", 0);
        AudioManager.Instance.musicVolume = _musicVolumeSlider;
        AudioManager.Instance.sfxVolume = _SFXVolumeSlider;
        AudioManager.Instance.voiceVolume = _voiceVolumeSlider;
    }
    public void OnPlayClicked()
    {
        mainMenu.ActivateMenu();
        this.DeactivateMenu();
    }

    public void OnOptionsClicked()
    {
        optionsMenu.ActivateMenu();
        this.DeactivateMenu();
    }

    public void OnQuitClicked()
    {
        confirmationPopupMenu.ActivateMenu(
               "Are you sure you want to quit game?",
               // function to execute if we select 'yes'
               () => {
                   Application.Quit();
               },
               // function to execute if we select 'cancel'
               () => {
                   this.ActivateMenu();
               }
           );
        this.DeactivateMenu();
    }

    public void ActivateMenu()
    {
        this.gameObject.SetActive(true);
    }

    public void DeactivateMenu()
    {
        this.gameObject.SetActive(false);
    }
}
