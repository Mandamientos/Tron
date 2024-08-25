using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class countdownAnim : MonoBehaviour
{
    public Animator countDown;
    public Animator countDown2;
    public Animator countDown3;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(countdownHandler());
    }

    IEnumerator countdownHandler() {

        countDown.SetTrigger("Countdown");

        yield return new WaitForSeconds(1f);

        countDown2.SetTrigger("Countdown2");

        yield return new WaitForSeconds(1f);

        countDown3.SetTrigger("Countdown3");

    }
}
