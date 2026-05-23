using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MintButton : MonoBehaviour
{
    public void Mint()
    {
        int mintLevel = 1;
        if (Player.Instance.DistanceStatistic > 500)
        {
            mintLevel = 3;
        }
        if (Player.Instance.DistanceStatistic > 1000)
        {
            mintLevel = 2;
        }

        string mintURL = "https://highlight.xyz/mint/base:0xdFF8cfBDDaE18D50c31978B47f4bA0fec4b670Fe:" + mintLevel.ToString();

        // Open the URL in the browser
        Application.OpenURL(mintURL);
    }
}
