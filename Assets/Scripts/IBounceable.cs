using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBounceable 
{
    public float BounceSpeed { get; }
    public bool CanJumpToAddMoreHeight { get; }
}
