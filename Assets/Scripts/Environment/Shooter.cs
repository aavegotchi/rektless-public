using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] bool flipYOnAwake;

    private void Awake()
    {
        if (flipYOnAwake)
        {
            transform.Rotate(Vector3.up, 180f);
        }
    }

    public void Anim_Fire()
    {
        if (spawnPoint == null || projectilePrefab == null)
            return;

        var shot = (GameObject)Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
