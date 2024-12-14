using System;
using System.Collections;
using DamageNumbersPro;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TimerVFX : MonoBehaviour
{
    [HideInInspector] public UnityEvent OnTimeUp = new UnityEvent();

    [SerializeField] private DamageNumberMesh text;
    [SerializeField] private float maxTime = 1f;
    [SerializeField] private float time = 1f;
    private bool _willDestroy = false;
    private bool _willLoop = false;


    public void SetTimeTo(float timeLimit, float countTime, bool willDestroy = true, bool willLoop = false)
    {
        maxTime = timeLimit;
        time = countTime;
        _willDestroy = willDestroy;
        _willLoop = willLoop;
    }

    private void Start()
    {
        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        while (true)
        {
            time -= 1;
            text.Spawn(transform.position, time);
            yield return new WaitForSeconds(1f);

            //Timer is up
            if (time <= 0)
            {
                OnTimeUp.Invoke();
                //Destroy
                if (_willDestroy)
                {
                    Destroy(gameObject);
                }

                //Loop
                if (_willLoop)
                {
                    time = maxTime;
                }
                else
                {
                    break;
                }
            }
        }


    }
}