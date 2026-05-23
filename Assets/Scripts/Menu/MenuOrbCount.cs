using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GameUi;

public class MenuOrbCount : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_TextMeshProUGUI;
    [SerializeField] bool doCountingAnimation;
    int currentCount;

    bool counting;

    private void Update()
    {
        if (currentCount == PlayerPrefs.GetInt(GameOver.GEMS_KEY) || counting) return;

        if (doCountingAnimation)
            StartCoroutine(Co_Count());
        else m_TextMeshProUGUI.text = PlayerPrefs.GetInt(GameOver.GEMS_KEY).ToString();
    }

    private IEnumerator Co_Count()
    {
        counting = true;

        int targetNumber = PlayerPrefs.GetInt(GameOver.GEMS_KEY);

        while (currentCount != PlayerPrefs.GetInt(GameOver.GEMS_KEY))
        {
            currentCount += AmountToChangeBy(targetNumber);
            m_TextMeshProUGUI.text = currentCount.ToString();
            yield return new WaitForSeconds(.01f);
        }

        m_TextMeshProUGUI.text = targetNumber.ToString();

        counting = false;
    }

    private int AmountToChangeBy(int targetNumber)
    {
        int difference = targetNumber - currentCount;
        int absoluteDiff = Mathf.Abs(difference);
        if (absoluteDiff >= 1000)
        {
            return (int)Mathf.Sign(difference) * 100;
        }
        else if (absoluteDiff >= 100)
        {
            return (int)Mathf.Sign(difference) * 10;
        }
        else if (absoluteDiff >= 50)
        {
            return (int)Mathf.Sign(difference) * 5;
        }
        else
            return (int)Mathf.Sign(difference) * 1;

    }
}
