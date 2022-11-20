using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    // Public fields
    public AudioMixer AudioMixer;
    public Slider MasterSlider, MusicSlider, UISlider, IngameSlider;

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

    public void SetMasterVolume(float volume) => AudioMixer.SetFloat("MasterVolume", volume);
    public void SetMusicVolume(float volume) => AudioMixer.SetFloat("MusicVolume", volume);
    public void SetUIVolume(float volume) => AudioMixer.SetFloat("UIVolume", volume);
    public void SetIngameVolume(float volume) => AudioMixer.SetFloat("IngameVolume", volume);

    public void Save()
    {
        float[] volumes = GetVolumes();
        SessionManager.Settings settings = new()
        {
            MasterVolume = volumes[0],
            MusicVolume = volumes[1],
            UIVolume = volumes[2],
            IngameVolume = volumes[3]

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
            float[] volumes = GetVolumes();
            settings = new()
            {
                MasterVolume = volumes[0],
                MusicVolume = volumes[1],
                UIVolume = volumes[2],
                IngameVolume = volumes[3]
            };
        }
        MasterSlider.SetValueWithoutNotify(settings.MasterVolume);
        SetMasterVolume(settings.MasterVolume);
        MusicSlider.SetValueWithoutNotify(settings.MusicVolume);
        SetMusicVolume(settings.MusicVolume);
        UISlider.SetValueWithoutNotify(settings.UIVolume);
        SetUIVolume(settings.UIVolume);
        IngameSlider.SetValueWithoutNotify(settings.IngameVolume);
        SetIngameVolume(settings.IngameVolume);
    }

    private float[] GetVolumes()
    {
        float[] volumes = new float[] { 0, 0, 0, 0 };
        AudioMixer.GetFloat("MasterVolume", out volumes[0]);
        AudioMixer.GetFloat("MusicVolume", out volumes[1]);
        AudioMixer.GetFloat("UIVolume", out volumes[2]);
        AudioMixer.GetFloat("IngameVolume", out volumes[3]);
        return volumes;
    }
}
