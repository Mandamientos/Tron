using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class musicHandler : MonoBehaviour
{
    private static musicHandler instance;
    public AudioSource audioSource;
    private float fadeOutSeconds = 0.5f;
    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }

        audioSource.volume = PlayerPrefs.GetFloat("musicVolume", 1f);
    }

    public static musicHandler getInstance () {
        return instance;
    }

    public IEnumerator fadeOut () {

        float startingVolume = audioSource.volume;
        Debug.Log(startingVolume);

        for (float t = 0; t < fadeOutSeconds; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startingVolume, 0, t / fadeOutSeconds);
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();

        Destroy(gameObject);


    }

}
