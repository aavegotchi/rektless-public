using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace GameUi
{
    public class Statistics : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI distanceText, killsText, gemsText, weaponsText;

        [SerializeField] private Image gemsUIImage, distanceUIImage;

        private void Start()
        {
            gemsUIImage.sprite = PersistentData.Instance.CurrentLevelConfig.GemUISprite;
            distanceUIImage.sprite = PersistentData.Instance.CurrentLevelConfig.DistanceFlagUISprite;
        }

        private void FixedUpdate()
        {
            distanceText.text = $"{Player.Instance.DistanceStatistic:F2}";
            killsText.text = $"{Player.Instance.KillsStatistic}";
            gemsText.text = $"{Player.Instance.GemsStatistic}";
            weaponsText.text = $"{Player.Instance.CurrentWeapons}";
        }
    }
}