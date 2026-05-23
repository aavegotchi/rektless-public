using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnable : MonoBehaviour
{
    [SerializeField]
    D_Spawnable spawnableType;

    public D_Spawnable SpawnableType => spawnableType;
}
