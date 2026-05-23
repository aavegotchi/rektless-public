using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncer : MonoBehaviour, IBounceable
{
    [SerializeField]
    float bounceSpeed = 10f;
    [SerializeField]
    bool canJumpToAddMoreHeight = true;


    bool IBounceable.CanJumpToAddMoreHeight => canJumpToAddMoreHeight;

    float IBounceable.BounceSpeed => bounceSpeed;

}
