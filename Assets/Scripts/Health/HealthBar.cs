using System;
using UnityEngine;
using UnityEngine.UI;

namespace Health
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Sprite fullHealthSprite;
        [SerializeField] private Sprite halfHealthSprite;
        [SerializeField] private Sprite emptyHealthSprite;
        [SerializeField] private bool useHalfBars;

        private Image _image;
        private int point = 2;
        
        public bool IsFull => point == 2;
        public bool IsEmpty => point == 0;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.sprite = fullHealthSprite;
            if (!useHalfBars)
                point = 1;
        }

        /// <summary>
        /// </summary>
        /// <returns>If bar is zero</returns>
        public bool DecreaseHealth()
        {
            if (!useHalfBars)
            {
                point--;
                _image.color = point == 0 ? Color.clear : Color.white;
                return IsEmpty;
            }

            if (point < 0 || point > 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            point--;
            switch (point)
            {
                case 2:
                    _image.sprite = fullHealthSprite;
                    break;
                case 1:
                    _image.sprite = halfHealthSprite;
                    break;
                case 0:
                    _image.sprite = emptyHealthSprite;
                    break;
            }

            return IsEmpty;
        }

        /// <summary>
        /// </summary>
        /// <returns>If bar is full</returns>
        public bool IncreaseHealth()
        {
            if (!useHalfBars)
            {
                point++;
                _image.color = point == 0 ? Color.clear : Color.white;
                return IsFull;
            }

            if (point < 0 || point > 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            point++;
            switch (point)
            {
                case 2:
                    _image.sprite = fullHealthSprite;
                    break;
                case 1:
                    _image.sprite = halfHealthSprite;
                    break;
                case 0:
                    _image.sprite = emptyHealthSprite;
                    break;
            }

            return IsFull;
        }


    }
}