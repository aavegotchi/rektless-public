using System.Collections.Generic;
using UnityEngine;

public class IcePit : TiledSpriteExtensible
{
    protected override void DoSetup()
    {
        base.DoSetup();

        var collider = gameObject.AddComponent<BoxCollider2D>();
        collider.size = (GetComponent<SpriteRenderer>().bounds.size / (Vector2)transform.localScale) - Vector2.up * .01f;
        collider.isTrigger = false;

    }

    protected void OnDisable()
    {
    }
}