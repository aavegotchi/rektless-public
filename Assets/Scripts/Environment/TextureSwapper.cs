using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureSwapper : MonoBehaviour
{
    [SerializeField] Texture2D textureToSwap;


    private void Awake()
    {
        StartCoroutine(ChangeTexture());
    }

    IEnumerator ChangeTexture()
    {
        Debug.Log("changingTexture");

        GetComponent<SpriteRenderer>().material.SetTexture("_SwapTex", textureToSwap);

        yield return null;
    }
}
