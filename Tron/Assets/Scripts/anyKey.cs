using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class anyKey : MonoBehaviour
{
    public GameObject Transition;
    private transitionHandler transitionHandler1;
    private transitionHandler transitionHandler2;

    void Start()
    {
        transitionHandler1 = Transition.GetComponent<transitionHandler>();
    }

    void Update()
    {
        if (Input.anyKeyDown) {
            musicHandler music = musicHandler.getInstance();

            StartCoroutine(music.fadeOut());

            transitionHandler1.changeScene(3);
        }
    }
}
