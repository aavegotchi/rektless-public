 using UnityEngine;

namespace Menu
{
    public class SoundManager : MonoBehaviourSingletonPersistent<SoundManager>
    {
        private static readonly string SoundFXKey = "SoundFX";
        private static readonly string VolumeKey = "Volume";

        public float Volume
        {
            get => PlayerPrefs.GetFloat(VolumeKey, .4f);
            set => PlayerPrefs.SetFloat(VolumeKey, value);
        }

        public bool SoundFX
        {
            get => PlayerPrefs.GetInt(SoundFXKey, 1) == 1;
            set => PlayerPrefs.SetInt(SoundFXKey, value ? 1 : 0);
        }
    }
}