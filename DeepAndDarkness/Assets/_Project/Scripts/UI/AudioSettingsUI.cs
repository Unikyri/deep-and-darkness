using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    // Reference to the AudioMixer and UI sliders for music and effects volume
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;

    // Keys for storing volume settings in PlayerPrefs
    // Names of the parameters in the AudioMixer
    private const string MusicKey = "MusicVolume";
    private const string EffectsKey = "EffectsVolume";

    private void Start()
    {
        // Load saved volume settings from PlayerPrefs or use default values 0.8f
        float musicValue = PlayerPrefs.GetFloat(MusicKey, 0.8f);
        float effectsValue = PlayerPrefs.GetFloat(EffectsKey, 0.8f);

        // Set the slider values without triggering the OnValueChanged event
        musicSlider.SetValueWithoutNotify(musicValue);
        effectsSlider.SetValueWithoutNotify(effectsValue);

        // Apply the loaded volume settings to the AudioMixer
        ApplyVolume("MusicVolume", musicValue);
        ApplyVolume("EffectsVolume", effectsValue);
    }

    // Conects to the Music slider OnValueChanged event in the inspector
    public void SetMusicVolume(float value)
    {
        // Apply the volume to the AudioMixer
        ApplyVolume("MusicVolume", value);
        // Save the volume setting to PlayerPrefs
        PlayerPrefs.SetFloat(MusicKey, value);
    }

    // Conects to the Effects slider OnValueChanged event in the inspector
    public void SetEffectsVolume(float value)
    {
        // Apply the volume to the AudioMixer
        ApplyVolume("EffectsVolume", value);
        // Save the volume setting to PlayerPrefs
        PlayerPrefs.SetFloat(EffectsKey, value);
    }

    // Converts the linear volume value to decibels and applies it to the AudioMixer
    private void ApplyVolume(string parameterName, float value)
    {
        // Convert linear volume (0.0 to 1.0) to decibels
        // Max avoids calculate Log10(0) which is undefined
        float decibels = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        // Changes the volume of the specified parameter in the AudioMixer
        audioMixer.SetFloat(parameterName, decibels);
    }
}
