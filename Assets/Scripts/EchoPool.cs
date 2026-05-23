using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EchoPool : MonoBehaviour
{
    List<SpriteRenderer> pool = new();
    [SerializeField] int startingCount;
    Transform echoParent;
    [SerializeField]
    float fadeSpeed;
    [SerializeField] Color startColor;
    [SerializeField] Material material;

    private void Awake()
    {
        GameObject parent = new GameObject("echoParent");
        echoParent = parent.transform;

        for (int i = 0; i < startingCount; i++)
            AddNewToPool();
    }

    private void FixedUpdate()
    {
        foreach (var sr in pool)
        {
            if (!sr.gameObject.activeInHierarchy) continue; 

            sr.color *= fadeSpeed;
            if (sr.color.a < .1f)
                sr.gameObject.SetActive(false);
        }
    }

    public void AddEcho(Transform creatorTransform, Sprite sprite)
    {
        foreach(var sr in pool)
        {
            if (!sr.gameObject.activeInHierarchy)
            {
                ActivateObject(sr, creatorTransform, sprite);
                return;
            }
        }
        SpriteRenderer newSR = AddNewToPool();
        ActivateObject(newSR, creatorTransform, sprite);
    }

    private void ActivateObject(SpriteRenderer sr, Transform creatorTransform, Sprite sprite)
    {
        sr.sprite = sprite;
        sr.gameObject.SetActive(true);
        sr.transform.SetPositionAndRotation(creatorTransform.position, creatorTransform.rotation);
        sr.transform.localScale = creatorTransform.localScale;
        sr.color = startColor;
        sr.sortingOrder = -1;
        sr.material.SetTexture("_SwapTex", PersistentData.Instance.CurrentCharacter.inGameTexture);
    }


    private SpriteRenderer AddNewToPool()
    {
        GameObject go = new GameObject("echo");
        var sr = go.AddComponent<SpriteRenderer>();
        go.transform.parent = echoParent;
        go.SetActive(false);

        sr.material = new Material(material);
        pool.Add(sr);
        return sr;
    }

}
