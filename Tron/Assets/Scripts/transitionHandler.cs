using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class transitionHandler : MonoBehaviour
{
    public Animator transition;
    public Animator transition2;

    public void changeScene (int sceneNumber) { 
         StartCoroutine(changingScene(sceneNumber));
    }

    IEnumerator changingScene(int sceneNumber) {
        transition.SetTrigger("Start");
        transition2.SetTrigger("Start");

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(sceneNumber);
    }
}
