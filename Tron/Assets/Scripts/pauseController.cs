using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class pauseController : MonoBehaviour
{
    public AudioSource bgsfx;
    public Animator pausedCanvas;
    public bool isPaused = false;
    public GameObject pausedMenu = null;
    public GameObject pausedMenuPrefab;
    public GameObject pausedMenuParent;
    public Animator animator1;
    public Animator animator2;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            StartCoroutine(unPause());
        }
    }

    public IEnumerator unPause () {
        if (!isPaused) {
            bgsfx.Pause();
            isPaused = true;
            pausedCanvas.SetTrigger("startPause");
            yield return new WaitForSeconds(0.3f);
            Time.timeScale = 0f;
        } else {
            pausedCanvas.SetTrigger("endPause");
            pausedCanvas.SetTrigger("restart");
            isPaused = false;
            Time.timeScale = 1f;
            bgsfx.UnPause();
        }
    }
        

    public void mainMenuStart() {
        StartCoroutine(mainMenu());
    }

    public void Pause() {
        StartCoroutine(unPause());
    }

    public IEnumerator mainMenu() {
        Time.timeScale = 1f;
        animator1.SetTrigger("Start");
        animator2.SetTrigger("Start");
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(0);
    }
}
