using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiledSpriteExtensible : MonoBehaviour
{
    protected float width;
    private void OnEnable()
    {
        DoSetup();
    }

    protected virtual void DoSetup()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        RaycastHit2D leftHit = Physics2D.Raycast(transform.position, Vector2.left, 20f, PhysicsManager.Instance.WhatIsGround);
        RaycastHit2D rightHit = Physics2D.Raycast(transform.position, Vector2.right, 20f, PhysicsManager.Instance.WhatIsGround);

        if (!leftHit || !rightHit)
            return;

        width = rightHit.point.x - leftHit.point.x;
        spriteRenderer.size = new Vector2(width * .25f, spriteRenderer.size.y);

        transform.position = (rightHit.point + leftHit.point) * .5f;

        var collider = gameObject.AddComponent<BoxCollider2D>();
        collider.size = (GetComponent<SpriteRenderer>().bounds.size / (Vector2)transform.localScale);
        collider.offset = Vector2.down * .2f;
        collider.isTrigger = true;
    }
}
 