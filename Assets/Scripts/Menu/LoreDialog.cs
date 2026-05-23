using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Menu
{
    [Serializable]
    public class LoreText
    {
        [TextArea(5,10)] public string text;
        public int fontSize;
        public Animator SpeakerAnimator;
    }

    public class LoreDialog : MonoBehaviour
    {
        [SerializeField] private LoreText[] loreTexts;
        [SerializeField] private TextMeshProUGUI loreText;
        [SerializeField] private Color fadeColor;
        HashSet<Animator> animators = new();
        private int loreIndex = 0;

        private void Start()
        {
            loreText.text = loreTexts[loreIndex].text;
            loreText.fontSize = loreTexts[loreIndex].fontSize;

            foreach (var item in loreTexts)
                animators.Add(item.SpeakerAnimator);
        }

        public void OnNextPressed()
        {
            loreIndex++;
            if (loreIndex >= loreTexts.Length)
            {
                MenuManager.Instance.LoreOnSkipPressed();
            }
            else
            {
                loreText.text = loreTexts[loreIndex].text;
                loreText.fontSize = loreTexts[loreIndex].fontSize;
                foreach (Animator anim in animators)
                    if (anim)
                        anim.SetBool("active", anim == loreTexts[loreIndex].SpeakerAnimator);
            }
        }
    }
}