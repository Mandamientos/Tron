using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class settingsManager : MonoBehaviour
{
    public Slider musicVolume;
    public Slider sfxVolume;
    public TMP_Text txtMusicVolume;
    public TMP_Text txtSfxVolume;
    public GameObject mainMenuMusic;
    public AudioSource mainMusic;

    void Start()
    {
        mainMenuMusic = findSingleton("sfx-bg");
        Debug.Log($"{mainMenuMusic.name}");
        mainMusic = mainMenuMusic.GetComponent<AudioSource>();

        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float savedMusicVolume = PlayerPrefs.GetFloat("musicVolume", 1f);

        musicVolume.value = savedMusicVolume;
        musicVolume.onValueChanged.AddListener(changeMusicVolume);

        sfxVolume.value = savedSFXVolume;
        sfxVolume.onValueChanged.AddListener(changeSFXVolume);

        txtMusicVolume.text = $"{Mathf.RoundToInt(savedMusicVolume*100)}%";
        txtSfxVolume.text = $"{Mathf.RoundToInt(savedSFXVolume*100)}%";
    }

    public void changeMusicVolume (float volume) {
        txtMusicVolume.text = $"{Mathf.RoundToInt(volume*100)}%";
        PlayerPrefs.SetFloat("musicVolume", volume);
        mainMusic.volume = volume;
    }

    public void changeSFXVolume (float volume) {
        txtSfxVolume.text = $"{Mathf.RoundToInt(volume*100)}%";
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public GameObject findSingleton(string name) {
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject gameObject in gameObjects) {
            if (gameObject.name == name && gameObject.scene.name == "DontDestroyOnLoad") {
                return gameObject;
            }
        }

        return null;
    }

}
