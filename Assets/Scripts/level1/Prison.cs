using System;
using System.Collections.Generic;
using DG.Tweening;
using GameUi;
using UnityEngine;
using Random = UnityEngine.Random;

namespace level1
{
    [Serializable]
    class PrisonData
    {
        public string textureName;
        public Texture2D texture;
        public Texture2D textureRescued; // In case of couldn't find in RestartManager.CharacterTextures
    }

    [RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
    public class Prison : MonoBehaviour
    {
        public static readonly string PRISON_KEY = "Prison";

        [SerializeField] private List<PrisonData> data;
        private static readonly int Explode = Animator.StringToHash("explode");

        private PrisonData rescuedData;

        private void Awake()
        {
            List<PrisonData> gonnaDestroy = new List<PrisonData>();
            foreach (var d in data)
            {
                string key = $"{PRISON_KEY}_{d.textureName}";
                bool isRescued = PlayerPrefs.GetInt(key, 0) == 1;
                if (isRescued)
                {
                    gonnaDestroy.Add(d);
                }

                if (string.IsNullOrEmpty(d.textureName))
                {
                    d.textureName = d.texture.name;
                }
            }

            foreach (var texture2D in gonnaDestroy)
            {
                data.Remove(texture2D);
            }

            if (data.Count == 0)
            {
                Debug.LogWarning("No more prisons to rescue");
                Destroy(gameObject);
            }

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            var randomData = data[Random.Range(0, data.Count)];
            spriteRenderer.material.SetTexture("_SwapTex", randomData.texture);
            rescuedData = randomData;

            var rescueUi = FindObjectOfType<RescueUI>(true);
            if (rescuedData.textureRescued != null)
            {
                rescueUi.prisonImage.material = new Material(rescueUi.prisonImage.material);
                rescueUi.prisonImage.material.SetTexture("_SwapTex", rescuedData.textureRescued);
            }
            else
            {
                Debug.LogWarning($"Texture not found: {randomData.textureName}");
            }
        }

        /// <summary>
        /// Moves right top
        /// </summary>
        public void StartMove()
        {
            Vector3 cameraRightTopWorldPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.90f, 0.85f, 0));
            cameraRightTopWorldPosition.z = transform.position.z;
            transform.DOMove(cameraRightTopWorldPosition, 1f);
        }

        /// <summary>
        /// Moves right ground, then center ground
        /// </summary>
        public void OnBossDeath()
        {
            Vector3 cameraCenterWorldPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.75f, 0));
            cameraCenterWorldPosition.y = -0.89f;
            cameraCenterWorldPosition.z = transform.position.z;

            transform.DOMove(cameraCenterWorldPosition, 2f).OnComplete(() =>
            {
                GetComponent<Animator>().SetTrigger(Explode);
                PlayerPrefs.SetInt($"{PRISON_KEY}_{rescuedData.textureName}", 1);
            });
        }

        public void OnDeathAnimationEnd()
        {
            Destroy(gameObject);
            GameUiManager.Instance.ShowAndHideGotchiRescuedPanel();
        }
    }
}