using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class SoundController : MonoBehaviour
    {
        [Header("SoundFX")] [SerializeField] public Sprite offSwitch;
        [SerializeField] public Sprite onSwitch;
        [SerializeField] private Image switchImage;
        [SerializeField] private TextMeshProUGUI switchText;

        [Header("Volume")] [SerializeField] private GameObject volumeSquarePanel;
        [SerializeField] private int totalVolumeSquares;
        [SerializeField] public Sprite filledVolumeSquare;
        [SerializeField] public Sprite emptyVolumeSquare;
        [SerializeField] private Vector2 volumeSquareSize;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip volumeChangeClip;
        [SerializeField] private AudioClip volumeBlockClip;

        [Header("Audio Sources")] [SerializeField]
        private List<AudioSource> soundFXAudioSources = new List<AudioSource>();

        Dictionary<int, Image> _volumeSquares = new Dictionary<int, Image>();
        private float _volumeRatio;

        public void Awake()
        {
            switchImage.sprite = SoundManager.Instance.SoundFX ? onSwitch : offSwitch;
            switchText.text = SoundManager.Instance.SoundFX ? "ON" : "OFF";
            _volumeRatio = 1f / totalVolumeSquares;

            float volume = SoundManager.Instance.Volume;

            _volumeSquares = new Dictionary<int, Image>();
            for (var i = 0; i < totalVolumeSquares; i++)
            {
                var volumeSquare = new GameObject($"VolumeSquare_{i}", typeof(RectTransform), typeof(Image));
                volumeSquare.transform.SetParent(volumeSquarePanel.transform);
                RectTransform rectTransform = volumeSquare.GetComponent<RectTransform>();
                rectTransform.sizeDelta = volumeSquareSize;
                rectTransform.localScale = Vector3.one;
                Image image = volumeSquare.GetComponent<Image>();
                image.sprite = i < volume * totalVolumeSquares ? filledVolumeSquare : emptyVolumeSquare;
                _volumeSquares.Add(i, image);
            }

            ChangeVolume(volume, false);
        }

        public void SwitchSound()
        {
            SoundManager.Instance.SoundFX = !SoundManager.Instance.SoundFX;
            switchImage.sprite = SoundManager.Instance.SoundFX ? onSwitch : offSwitch;
            switchText.text = SoundManager.Instance.SoundFX ? "ON" : "OFF";

            foreach (var soundFXAudioSource in soundFXAudioSources)
            {
                soundFXAudioSource.mute = !SoundManager.Instance.SoundFX;
            }

            audioSource.PlayOneShot(volumeChangeClip);
        }

        public void RefreshSwitch()
        {
            switchImage.sprite = SoundManager.Instance.SoundFX ? onSwitch : offSwitch;
            switchText.text = SoundManager.Instance.SoundFX ? "ON" : "OFF";
        }

        public void ChangeVolume(float volume, bool playSound = true)
        {
            if (_volumeSquares.Count == 0)
            {
                return;
            }

            int filledSquares = (int)(SoundManager.Instance.Volume * totalVolumeSquares);

            for (var i = 0; i < totalVolumeSquares; i++)
            {
                _volumeSquares[i].sprite = i < filledSquares ? filledVolumeSquare : emptyVolumeSquare;
            }

            SoundManager.Instance.Volume = volume;
            AudioListener.volume = volume;

            if (playSound)
            {
                audioSource.PlayOneShot(volumeChangeClip);
            }
        }

        public void IncreaseVolume()
        {
            if (SoundManager.Instance.Volume >= 1)
            {
                audioSource.PlayOneShot(volumeBlockClip);
                return;
            }

            SoundManager.Instance.Volume = Mathf.Clamp(SoundManager.Instance.Volume + _volumeRatio, 0, 1);
            ChangeVolume(SoundManager.Instance.Volume);
        }

        public void DecreaseVolume()
        {
            if (SoundManager.Instance.Volume <= 0)
            {
                audioSource.PlayOneShot(volumeBlockClip);
                return;
            }

            SoundManager.Instance.Volume = Mathf.Clamp(SoundManager.Instance.Volume - _volumeRatio, 0, 1);
            ChangeVolume(SoundManager.Instance.Volume);
        }
    }
}