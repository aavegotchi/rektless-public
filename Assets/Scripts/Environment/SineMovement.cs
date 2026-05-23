using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineMovement : MonoBehaviour
{
    [SerializeField] Vector2 OriginalPos;

    [SerializeField] float yWavelength = 1, yAmplitude = 1, xWavelength = 1, xAmplitude = 1;

    Vector2 currentOffset = new();

    private void OnEnable()
    {
        OriginalPos = transform.localPosition;
    }

    private void Update()
    {
        currentOffset.y = Mathf.Sin(Time.time * yWavelength) * yAmplitude;
        currentOffset.x = Mathf.Cos(Time.time * xWavelength) * xAmplitude;
        transform.localPosition = OriginalPos + currentOffset;
    }
}
