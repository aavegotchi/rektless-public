using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaPit : TiledSpriteExtensible
{
    [SerializeField] GameObject bubblePrefab;
    [SerializeField] float bubblesYOffset;
    [SerializeField] float bubblesWidth;
    List<GameObject> bubbles = new ();

    protected override void DoSetup()
    {
        base.DoSetup();

        float bubblesNeeded = width * 2f;
        for (int i = 0; i < bubblesNeeded; i++)
        {
            GameObject instantiated = Instantiate(bubblePrefab, transform);

            instantiated.transform.localPosition += Vector3.up * bubblesYOffset;
            instantiated.transform.localPosition += Vector3.right * bubblesWidth * i;
            bubbles.Add(instantiated);
        }
        foreach (GameObject bubble in bubbles)
        {
            bubble.transform.localPosition += Vector3.left * bubblesWidth * bubblesNeeded * .5f;
        }
    }

    protected void OnDisable()
    {
        foreach (GameObject bubble in bubbles)
        {
            Destroy(bubble);
        }
    }
}
