using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    // Public fields
    public AudioSource BackgroundMusic;
    public Slider BackgroundMusicSlider;

    // Private fields
    private SessionManager _sessionManager;

    private void Start()
    {
        _sessionManager = FindObjectOfType<SessionManager>();
        Load();
    }

    private void OnDisable()
    {
        try
        {
            Save();
        }
        catch
        { }
    }

    public void SetMusicVolume(float volume)
    {
        BackgroundMusic.volume = volume;
    }

    public void Save()
    {
        SessionManager.Settings settings = new()
        {
            MusicVolume = BackgroundMusic.volume,
        };
        _sessionManager.SaveSettings(settings);
    }

    public void Load()
    {
        SessionManager.Settings settings;
        try
        {
            settings = _sessionManager.LoadSettings();
        }
        catch (System.IO.FileNotFoundException)
        {
            settings = new()
            {
                MusicVolume = BackgroundMusic.volume,
            };
        }
        BackgroundMusicSlider.SetValueWithoutNotify(settings.MusicVolume);
        SetMusicVolume(settings.MusicVolume);
    }
}
