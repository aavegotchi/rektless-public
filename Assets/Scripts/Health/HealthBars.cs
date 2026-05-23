using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Health
{
    public class HealthBars : MonoBehaviour
    {
        public GameObject healthBarPrefab;
        [SerializeField] private bool useHalfBars;

        private List<HealthBar> healthBars = new List<HealthBar>();

        public void InitializeHealthBars(int healthBarCount, GameObject healthBarPrefab = null)
        {
            Clear();

            if (this.healthBarPrefab == null) 
                this.healthBarPrefab = healthBarPrefab;

            if (useHalfBars) 
                healthBarCount /= 2;
            for (int i = 0; i < healthBarCount; i++)
            {
                var healthBar = Instantiate(healthBarPrefab, transform).GetComponent<HealthBar>();
                healthBars.Add(healthBar);
            }
        }

        public bool DecreaseHealthBar(int amount)
        {
            if (healthBars.Count == 0)
            {
                return false;
            }

            if (healthBars.Count < amount)
            {
                return false;
            }

            for (int i = amount; i > 0; i--)
            {
                var healthBar = healthBars.LastOrDefault(x => !x.IsEmpty);
                if (healthBar == null)
                {
                    Debug.Log("There is no health bar to decrease.");
                    return false;
                }

                healthBar.DecreaseHealth();
            }

            return true;
        }
        
        public bool IncreaseHealthBar(int amount)
        {
            if (healthBars.Count == 0)
            {
                return false;
            }

            if (healthBars.Count < amount)
            {
                return false;
            }

            for (int i = amount; i > 0; i--)
            {
                var healthBar = healthBars.FirstOrDefault(x => !x.IsFull);
                if (healthBar == null)
                {
                    Debug.Log("There is no health bar to increase.");
                    return false;
                }

                healthBar.IncreaseHealth();
            }

            return true;
        }

        public void Clear()
        {
            List<GameObject> toDestroy = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++)
                toDestroy.Add(transform.GetChild(i).gameObject);
            foreach (GameObject go in toDestroy)
                Destroy(go);
        }
    }
}