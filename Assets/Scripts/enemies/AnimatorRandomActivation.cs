// using PlayFab.AdminModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorRandomActivation : MonoBehaviour
{
    [SerializeField] int minLoops = 1, maxLoops = 5;
    [SerializeField] float minIdleTime = 0f, maxIdleTime = 1f;
    float currentIdleTime;
    int currentRemainingLoops;

    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (currentIdleTime > 0f)
        {
            currentIdleTime -= Time.deltaTime;
        }
        else anim.SetBool("active", true);
    }
    public void Reset()
    {
        anim.SetBool("active", false);
        currentIdleTime = Random.Range(minIdleTime, maxIdleTime);
        currentRemainingLoops = Random.Range(minLoops, maxLoops);
    }

    public void Anim_LoopStart()
    {
        currentRemainingLoops--;
        if (currentRemainingLoops < 0)
        {
            Reset();
        }
    }
}
