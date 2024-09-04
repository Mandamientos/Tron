using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class countdownAnim : MonoBehaviour
{
    public Animator countDown;
    public Animator countDown2;
    public Animator countDown3;
    public AudioSource beepa;
    public AudioSource beepb;
    public AudioSource beepc;
    public AudioSource bgsfx;
    public playerController playerControllerScript;
    public GameObject greenBot;
    public botController greenBotID;
    public GameObject blueBot;
    public botController blueBotID;
    public GameObject purpleBot;
    public botController purpleBotID;
    public GameObject yellowBot;
    public botController yellowBotID;


    void Start()
    {
        playerControllerScript = FindObjectOfType<playerController>();
        greenBotID = greenBot.GetComponent<botController>();
        blueBotID = blueBot.GetComponent<botController>();
        purpleBotID = purpleBot.GetComponent<botController>();
        yellowBotID = yellowBot.GetComponent<botController>();
        
        StartCoroutine(countdownHandler());
    }

    IEnumerator countdownHandler() {

        countDown.SetTrigger("Countdown");
        yield return new WaitForSeconds(1f);
        beepa.Play();

        countDown2.SetTrigger("Countdown2");

        yield return new WaitForSeconds(1f);
        beepb.Play();

        countDown3.SetTrigger("Countdown3");

        yield return new WaitForSeconds(1f);
        beepc.Play();

        yield return new WaitForSeconds(0.3f);
        bgsfx.Play();
        playerControllerScript.isAlive = true;
        greenBotID.isAlive = true;
        blueBotID.isAlive = true;
        purpleBotID.isAlive = true;
        yellowBotID.isAlive = true;
        StartCoroutine(playerControllerScript.playerMovement());
        StartCoroutine(greenBotID.updateBot());
        StartCoroutine(blueBotID.updateBot());
        StartCoroutine(purpleBotID.updateBot());
        StartCoroutine(yellowBotID.updateBot());
    }
}
