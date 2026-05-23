using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newBackgroundData", menuName = "Data/Background")]
public class D_BackgroundLayerData : ScriptableObject
{
    public Sprite sprite;
    public float parallaxFactor;
    public Vector2 sizeScale;
}
