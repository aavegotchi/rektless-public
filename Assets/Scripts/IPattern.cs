using System.Collections.Generic;
using UnityEngine;

public enum PatternType
{
    Meteor,
    Boss,
    Default
}

public interface IPattern
{
    public Vector2 RightAlignmentPoint { get; }
    public Vector2 LeftAlignmentPoint { get; }
    public PatternType PatternType { get; }
    public float Difficulty { get; }
    public Dictionary<D_Spawnable, List<(Vector2 pos, Quaternion rot)>> PositionsBySpawnable();
    public void Deactivate();
    public bool IsFinished();
    public void SetBoundaries(Transform leftJoinPoint, Transform rightJoinPoint);

    public GameObject GetGameObject();
}