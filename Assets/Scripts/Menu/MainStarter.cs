using DG.Tweening;
using UnityEngine;

namespace Menu
{
    public class MainStarter : MonoBehaviour
    {
        [SerializeField] private GameObject playButton;
        [SerializeField] private RectTransform banner;
        [SerializeField] private GameObject bannerEnemies;
        [SerializeField] private GameObject darkBackground;
        [SerializeField] private RectTransform icon;
        [SerializeField] private GameObject settingsButton;

        [SerializeField] private Vector2 bannerEndSize;
        [SerializeField] private Vector2 iconEndSize;

        private bool _animationDone;

        public bool AnimationDone
        {
            get => _animationDone;
            set
            {
                _animationDone = value;
                if (_animationDone)
                {
                    icon.gameObject.SetActive(true);
                    playButton.SetActive(true);
                    bannerEnemies.SetActive(true);
                    banner.gameObject.SetActive(true);
                    darkBackground.SetActive(true);
                    settingsButton.SetActive(true);

                    banner.sizeDelta = bannerEndSize;
                    icon.sizeDelta = iconEndSize;
                }
            }
        }

        private void Awake()
        {
            //icon.gameObject.SetActive(false);
            //playButton.SetActive(false);
            //bannerEnemies.SetActive(false);
            //settingsButton.SetActive(false);
            //banner.gameObject.SetActive(true);
            //darkBackground.SetActive(true);

            //banner.sizeDelta = bannerEndSize / 10;
            //icon.sizeDelta = iconEndSize / 10;
        }

        private void OnEnable()
        {
            if (AnimationDone) return;

           // Animate();
        }

        private void Animate()
        {
            banner.DOSizeDelta(bannerEndSize, 1f).OnComplete(() =>
            {
                bannerEnemies.SetActive(true);
                icon.gameObject.SetActive(true);
                settingsButton.SetActive(true);
                icon.DOSizeDelta(iconEndSize, 0.5f).OnComplete(() => { playButton.SetActive(true); });
                AnimationDone = true;
            });
        }
    }
}