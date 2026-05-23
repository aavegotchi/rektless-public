using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using GameUi;
using PlayFab.ClientModels;

namespace Menu
{
    [Serializable]
    public class CharacterData
    {
        public string name;
        public string Unlocks;
        public GameObject prefab;
        public bool locked; // Set from PlayerPrefs
        public Texture2D inGameTexture;
        public string inGameTextureUrl;
        public int price = 1;
        public D_LevelConfig LevelConfig;
    }

    public class CharacterSelector : MonoBehaviour
    {
        [SerializeField] public List<CharacterData> characters;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] GameObject characterCostIcon;
        [SerializeField] private RectTransform otherCharacterPanel;
        [SerializeField] private GameObject fillerPanel;
        [SerializeField] private GameObject buyButton;
        [SerializeField] private Sprite lockedSprite;
        [SerializeField] private Vector2 selectedCharacterSize = new Vector2(300, 300);
        [SerializeField] private Vector2 otherCharacterSize = new Vector2(200, 200);
        [SerializeField] private Vector2 selectedLockedSpriteSize = new Vector2(300, 300);
        [SerializeField] private Vector2 otherLockedSpriteSize = new Vector2(200, 200);
        [SerializeField] private Vector2 selectedLockedSpritePositionOffset = new Vector2(0, 0);
        [SerializeField] private Vector2 otherLockedSpritePositionOffset = new Vector2(0, 0);
        [SerializeField] private float middleCharacterSpacing = 250f;
        [SerializeField] private float transitionDuration = 0.3f;
        [SerializeField] private float sizeReductionRate = 0.1f;
        [SerializeField] private int visibleCharacterCount = 7;

        [SerializeField] private bool unlockableCharacters = false;

        [Header("Dynamically Texture Loading")] 
        [SerializeField]
        private Material material;

        [SerializeField] private bool disableLockSprite = false;

        [Header("Audio")] [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip forbiddenSound;
        [SerializeField] private AudioClip selectSound;

        private List<GameObject> characterObjects = new List<GameObject>();

        public int currentIndex = 0;
        public int DefaultIndex;

        public bool IsBossSelection = false;

        public bool Playable
        {
            get
            {
                audioSource.PlayOneShot(!characters[currentIndex].locked ? selectSound : forbiddenSound);
                return !characters[currentIndex].locked;
            }
        }

        public CharacterData SelectedCharacter => characters[currentIndex];

        private void Awake()
        {
            if (IsBossSelection == false)
            {
                if (PersistentData.Instance.HasACharacterBeenChosen == false)
                {
                    currentIndex = DefaultIndex;
                }
                else
                {
                    currentIndex = PersistentData.Instance.IndexOfChosenCharacter;
                }
            }

            Init();
        }

        private void Init()
        {
            //currentIndex = characters.Count / 2;
            CreateCharacterObjects();
            UpdateCharacterPositions(false);
        }

        private void CreateCharacterObjects()
        {
            for (int i = 0; i < characters.Count; i++)
            {
                if (PlayerPrefs.GetInt(characters[i].name, 1) != 1)
                {
                    UnlockCharacter(characters[i]);
                }
                GameObject charObject = Instantiate(characters[i].prefab, transform);
                charObject.name = $"Character_{i}";
                if (i == currentIndex)
                {
                    charObject.transform.SetAsFirstSibling();
                }

                RectTransform rectTransform = charObject.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition += Vector2.up * 10f;

                Image charImage = charObject.GetComponent<Image>();
                //charImage.sprite = characters[i].sprite.texture;
                charImage.preserveAspect = true;

                Color filterColor = new Color(0, 0, 0, characters[i].locked ? 0.75f : 0f);


                Material mat = new Material(material);
                    
                Texture2D texture = characters[i].inGameTexture;
                mat.SetTexture("_SwapTex",
                    characters[i].inGameTexture);
                mat.SetColor("_FilterColor ", filterColor);
                charImage.material = mat;


                // Add lock gameobject
                GameObject lockObject = new GameObject("Lock", typeof(RectTransform));
                lockObject.transform.SetParent(charObject.transform);
                RectTransform lockTransform = lockObject.GetComponent<RectTransform>();
                lockTransform.anchorMin = new Vector2(0.5f, 0.5f);
                lockTransform.anchorMax = new Vector2(0.5f, 0.5f);
                lockTransform.pivot = new Vector2(0.5f, 0.5f);
                lockTransform.anchoredPosition = i == currentIndex
                    ? selectedLockedSpritePositionOffset
                    : otherLockedSpritePositionOffset;
                lockTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1f);
                lockTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1f);

                if (!disableLockSprite)
                {
                    Vector2 lockSize = PlayerPrefs.GetInt(characters[i].name) == 1
                        ? currentIndex == i ? selectedLockedSpriteSize : otherLockedSpriteSize
                        : otherLockedSpriteSize;
                    lockTransform.sizeDelta = lockSize;

                    Image lockImage = lockObject.AddComponent<Image>();
                    lockImage.sprite = lockedSprite;
                    lockImage.preserveAspect = true;
                }

                characterObjects.Add(charObject);
            }
        }

        private void UpdateCharacterPositions(bool animate)
        {
            int halfCount = visibleCharacterCount / 2;
            float centerBuffer = 50f; // Extra space around the center character

            for (int i = 0; i < visibleCharacterCount; i++)
            {
                int relativeIndex = i - halfCount;
                int actualIndex = (currentIndex + relativeIndex + characters.Count) % characters.Count;
                var character = characterObjects[actualIndex];
                RectTransform rectTransform = character.GetComponent<RectTransform>();
                Image charImage = character.GetComponent<Image>();

                // Determine if this is an edge character
                bool isEdgeCharacter = i == 0 || i == visibleCharacterCount - 1;


                charImage.material.SetColor("_FilterColor",
                    new Color(0, 0, 0, characters[actualIndex].locked ? 0.75f : 0f));


                if (i == halfCount) // Center (selected) character
                {
                    rectTransform.SetParent(transform);
                    Vector2 targetSize = selectedCharacterSize;
                    Vector2 targetPosition = Vector2.up * 25;

                    if (animate)
                    {
                        rectTransform.DOSizeDelta(targetSize, transitionDuration);
                        rectTransform.DOAnchorPos(targetPosition, transitionDuration);

                        if (!disableLockSprite)
                        {
                            RectTransform lockTransform =
                                character.transform.Find("Lock").GetComponent<RectTransform>();
                            lockTransform.DOSizeDelta(selectedLockedSpriteSize, transitionDuration);
                            lockTransform.DOAnchorPos(selectedLockedSpritePositionOffset, transitionDuration);
                        }
                    }
                    else
                    {
                        rectTransform.sizeDelta = targetSize;
                        rectTransform.anchoredPosition = targetPosition;

                        if (!disableLockSprite)
                        {
                            RectTransform lockTransform =
                                character.transform.Find("Lock").GetComponent<RectTransform>();
                            lockTransform.sizeDelta = selectedLockedSpriteSize;
                            lockTransform.anchoredPosition = selectedLockedSpritePositionOffset;

                            Image lockImage = character.transform.Find("Lock").GetComponent<Image>();
                            lockImage.enabled = characters[actualIndex].locked;
                        }
                    }

                    rectTransform.SetAsLastSibling(); // Ensure center character is on top
                }
                else // Other characters
                {
                    rectTransform.SetParent(otherCharacterPanel);

                    float baseSpacing = middleCharacterSpacing + centerBuffer;
                    float xPosition = relativeIndex * baseSpacing;

                    float sizeReduction = 1f - (Mathf.Abs(relativeIndex) * sizeReductionRate);
                    sizeReduction = Mathf.Clamp(sizeReduction, 0.3f, 0.8f);

                    Vector2 targetSize = otherCharacterSize * sizeReduction;
                    Vector2 lockSize = otherLockedSpriteSize * sizeReduction;

                    if (animate)
                    {
                        // Disable image component for edge characters at the start of animation
                        if (isEdgeCharacter)
                        {
                            charImage.enabled = false;
                            if (!disableLockSprite)
                            {
                                Image lockImage = character.transform.Find("Lock").GetComponent<Image>();
                                lockImage.enabled = false;
                            }
                        }

                        rectTransform.DOSizeDelta(targetSize, transitionDuration);
                        rectTransform.DOAnchorPos(new Vector2(xPosition, 0), transitionDuration).OnComplete(() =>
                        {
                            // Re-enable image component for edge characters at the end of animation
                            if (isEdgeCharacter)
                            {
                                charImage.enabled = true;
                                if (!disableLockSprite)
                                {
                                    Image lockImage = character.transform.Find("Lock").GetComponent<Image>();
                                    lockImage.enabled = characters[actualIndex].locked;
                                }
                            }
                        });

                        if (!disableLockSprite)
                        {
                            RectTransform lockTransform =
                                character.transform.Find("Lock").GetComponent<RectTransform>();
                            lockTransform.DOSizeDelta(lockSize, transitionDuration);
                            lockTransform.DOAnchorPos(otherLockedSpritePositionOffset, transitionDuration);
                        }
                    }
                    else
                    {
                        rectTransform.sizeDelta = targetSize;
                        rectTransform.anchoredPosition = new Vector2(xPosition, 0);

                        if (!disableLockSprite)
                        {
                            RectTransform lockTransform =
                                character.transform.Find("Lock").GetComponent<RectTransform>();
                            lockTransform.sizeDelta = lockSize;
                            lockTransform.anchoredPosition = otherLockedSpritePositionOffset;

                            Image lockImage = character.transform.Find("Lock").GetComponent<Image>();
                            lockImage.enabled = characters[actualIndex].locked;
                        }

                        // Ensure image components are enabled for non-animated updates
                        charImage.enabled = true;
                    }
                }

                if (!disableLockSprite && !isEdgeCharacter)
                {
                    Image lockImage = character.transform.Find("Lock").GetComponent<Image>();
                    lockImage.enabled = characters[actualIndex].locked;
                }

                character.SetActive(true);
            }

            // Hide characters that are not visible
            for (int i = 0; i < characters.Count; i++)
            {
                bool isVisible = false;
                for (int j = 0; j < visibleCharacterCount; j++)
                {
                    if (i == (currentIndex + j - halfCount + characters.Count) % characters.Count)
                    {
                        isVisible = true;
                        break;
                    }
                }

                characterObjects[i].SetActive(isVisible);
            }

            if (characters[currentIndex].locked)
            {
                nameText.text = "Locked";
                if (unlockableCharacters)
                {
                    nameText.text = characters[currentIndex].price.ToString();
                    characterCostIcon.SetActive(true);
                    buyButton.SetActive(true);
                }
            }
            else
            {
                nameText.text = characters[currentIndex].name;
                if (unlockableCharacters)
                {
                    characterCostIcon.SetActive(false);
                    buyButton.SetActive(false);
                }
            }
        }

        public void NextCharacter()
        {
            currentIndex = (currentIndex + 1) % characters.Count;
            UpdateCharacterPositions(true);
        }

        public void PreviousCharacter()
        {
            currentIndex = (currentIndex - 1 + characters.Count) % characters.Count;
            UpdateCharacterPositions(true);
        }

        private void OnEnable()
        {
            if (characterObjects.Count == characters.Count) UpdateCharacterPositions(false);
            if (fillerPanel != null)
            {
                fillerPanel.SetActive(true);
            }
        }

        private void OnDisable()
        {
            characterObjects.ForEach(c => c.SetActive(false));
            if (fillerPanel != null)
            {
                fillerPanel.SetActive(false);
            }
        }

        public void TryUnlockCurrent()
        {
            if (!unlockableCharacters) return;
            var character = characters[currentIndex];
            if (!character.locked) return;

            var gems = PlayerPrefs.GetInt(GameOver.GEMS_KEY, 0);
            if (character.locked && character.price <= gems)
            {
                character.locked = false;
                UnlockCharacter(character);
               // RestartManager.Instance.CharacterTextures[character.name].Unlock();
                PlayerPrefs.SetInt(GameOver.GEMS_KEY, gems - character.price);
                UpdateCharacterPositions(true);
            }
            else
            {
                Debug.LogWarning(
                    $"Not enough gems to unlock {character.name}, required: {character.price}, available: {gems}");
            }
        }

        private void UnlockCharacter(CharacterData toUnlock)
        {
            PlayerPrefs.SetInt(toUnlock.name, 0);
            toUnlock.locked = false;
        }
    }
}
