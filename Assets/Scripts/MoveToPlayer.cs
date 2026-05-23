using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPlayer : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 20;

    private void LateUpdate()
    {
        if (!Player.Instance) return;

        transform.position = Vector2.MoveTowards(transform.position, Player.Instance.transform.position, moveSpeed * Time.deltaTime);
    }
}
