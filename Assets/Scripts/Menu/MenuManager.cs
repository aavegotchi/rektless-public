using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    [Serializable]
    public struct SettingsColorSet
    {
        public Color mainColor;
        public Sprite hamburgerSprite;
        public Sprite soundSprite;
        public Sprite thunderSprite;
        public Sprite filledVolumeSquare;
        public Sprite emptyVolumeSquare;
        public Sprite offSwitch;
        public Sprite onSwitch;
    }

    public class MenuManager : MonoBehaviourSingleton<MenuManager>
    {
        [SerializeField] private string levelName;
        [SerializeField] private MainStarter main;
        [SerializeField] private GameObject lore;
        [SerializeField] private GameObject walletConnect;

        [SerializeField] private GameObject selectCharacter;
        [SerializeField] private CharacterSelector characterSelector;

        [SerializeField] private GameObject selectBoss;
        [SerializeField] private CharacterSelector bossSelector;

        [Header("Settings")] [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject soundPanel;
        [SerializeField] private SettingsColorSet mainColorSet;
        [SerializeField] private SettingsColorSet loreColorSet;
        [SerializeField] private SettingsColorSet walletConnectColorSet;
        [SerializeField] private Image[] settingsColorReplacementImages;
        [SerializeField] private Image hamburger;
        [SerializeField] private Image sound;
        [SerializeField] private List<Image> thunder;
        [SerializeField] private List<TextMeshProUGUI> texts;
        [SerializeField] private List<SettingsButton> settingsButtons;
        [SerializeField] private SoundController soundController;

        [SerializeField] Coroutine walletConnectCoroutine;

        private void Start()
        {
            PersistentData.Instance.DebugInfiniteLife = false;
            if (!RestartManager.Instance.CameFromGame)
            {
                main.gameObject.SetActive(true);
                lore.SetActive(false);
                walletConnect.SetActive(false);
                selectCharacter.SetActive(false);
                selectBoss.SetActive(false);
                settingsPanel.SetActive(false);
                soundPanel.SetActive(false);
                SetSettingsColor(mainColorSet);
                
                //RestartManager.Instance.StartCoroutine(RestartManager.Instance.DownloadAll(characterSelector.characters,
                //    _ => { }));
            }
            else
            {
                main.gameObject.SetActive(false);
                lore.SetActive(false);
                walletConnect.SetActive(false);
                selectCharacter.SetActive(true);
                selectBoss.SetActive(false);
                settingsPanel.SetActive(false);
                soundPanel.SetActive(false);
                SetSettingsColor(mainColorSet);

                RestartManager.Instance.CameFromGame = false;
                main.AnimationDone = true;
            }
        }

        private void StartGame()
        {
            var selectedBoss = bossSelector.SelectedCharacter;
            PersistentData.Instance.CurrentLevelConfig = selectedBoss.LevelConfig;
            PersistentData.Instance.CurrentBossName = selectedBoss.name;
            PersistentData.Instance.BossToUnlockOnDefeat = selectedBoss.Unlocks;
            SceneManager.LoadScene(levelName);
        }

        public void MainOnPlayPressed()
        {
            main.gameObject.SetActive(false);
            lore.SetActive(true);
            SetSettingsColor(loreColorSet);
        }

        public void LoreOnSkipPressed()
        {
            lore.SetActive(false);
            // walletConnect.SetActive(true);
            SetSettingsColor(walletConnectColorSet);
            AdvanceToCharacterSelect();

            var frontendWallet = WalletManager.Instance.WalletAddress;
            if (!string.IsNullOrEmpty(frontendWallet))
            {
                PlayfabManager.Instance.LoginWithWallet(frontendWallet);
            }
            else
            {
                Debug.Log("[Menu] Waiting for frontend wallet. Login as guest!");
                PlayfabManager.Instance.LoginAsGuest();
            }
        }

        public void WalletConnectOnGuestPressed()
        {
            PlayfabManager.Instance.LoginAsGuest();
            AdvanceToCharacterSelect();
        }

        public void WalletConnectOnConnectPressed()
        {
            if (walletConnectCoroutine != null)
                StopCoroutine(walletConnectCoroutine);
            walletConnectCoroutine = StartCoroutine(WalletConnect());
        }

        private IEnumerator WalletConnect()
        {
            var frontendWallet = WalletManager.Instance.WalletAddress;
            if (!string.IsNullOrEmpty(frontendWallet))
            {
                PlayfabManager.Instance.LoginWithWallet(frontendWallet);
                AdvanceToCharacterSelect();
                yield break;
            }

            float timer = 60f;
            while (string.IsNullOrEmpty(WalletManager.Instance.WalletAddress) && timer > 0f)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            frontendWallet = WalletManager.Instance.WalletAddress;
            if (!string.IsNullOrEmpty(frontendWallet))
            {
                PlayfabManager.Instance.LoginWithWallet(frontendWallet);
                AdvanceToCharacterSelect();
            }
            else
            {
                Debug.LogWarning("[Menu] WalletConnect timed out waiting for frontend wallet.");
            }
        }
        public void AdvanceToCharacterSelect()
        {
            walletConnect.SetActive(false);
            selectCharacter.SetActive(true);
            SetSettingsColor(mainColorSet);
        }

        public void SelectCharacterOnBackPressed()
        {
            // walletConnect.SetActive(true);
            // lore.SetActive(false);
            // selectCharacter.SetActive(false);
            // SetSettingsColor(walletConnectColorSet);
            AdvanceToCharacterSelect();
        }

        public void SelectCharacterOnSelectPressed()
        {
            if (characterSelector.Playable)
            {
                PersistentData.Instance.HasACharacterBeenChosen = true;
                PersistentData.Instance.IndexOfChosenCharacter = characterSelector.currentIndex;
                PersistentData.Instance.CurrentCharacter = characterSelector.SelectedCharacter;

                selectCharacter.SetActive(false);
                selectBoss.SetActive(true);
                SetSettingsColor(walletConnectColorSet);
            }
        }

        public void SelectBossOnBackPressed()
        {
            selectCharacter.SetActive(true);
            selectBoss.SetActive(false);
            SetSettingsColor(mainColorSet);
        }

        public void SelectBossOnSelectPressed()
        {
            if (bossSelector.Playable)
            {
                PersistentData.Instance.DebugInfiniteLife = false;
                StartGame();
            }
        }

        private void SetSettingsColor(SettingsColorSet colorSet)
        {
            hamburger.sprite = colorSet.hamburgerSprite;
            sound.sprite = colorSet.soundSprite;
            foreach (var image in thunder)
            {
                image.sprite = colorSet.thunderSprite;
            }

            foreach (var text in texts)
            {
                text.color = colorSet.mainColor;
            }

            foreach (var button in settingsButtons)
            {
                button.defaultColor = colorSet.mainColor;
            }

            foreach (var image in settingsColorReplacementImages)
            {
                image.material.SetColor("_ReplacementColor", colorSet.mainColor);
            }

            soundController.filledVolumeSquare = colorSet.filledVolumeSquare;
            soundController.emptyVolumeSquare = colorSet.emptyVolumeSquare;
            soundController.ChangeVolume(SoundManager.Instance.Volume, false);

            soundController.offSwitch = colorSet.offSwitch;
            soundController.onSwitch = colorSet.onSwitch;
            soundController.RefreshSwitch();
        }

        public void DebugPlay()
        {
            PersistentData.Instance.DebugInfiniteLife = true;
            StartGame();
        }

        public void DebugClearSave()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}
