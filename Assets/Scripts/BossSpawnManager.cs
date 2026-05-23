using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Health;

public class BossSpawnManager : MonoBehaviourSingleton<BossSpawnManager>
{
    [SerializeField] private GameObject rightWall, leftWall;
    public HealthBars bossHealthBar;

    public int bossesSpawned = 0;
    public static Action OnBossSpawned;
    public static Action OnBossDefeated;
    public static Action OnReadyToSpawn;

    public bool ReadyToSpawn { get; set; } = true;
    public BossPattern CurrentBossPattern { get; set; }

    private void Start()
    {
        bossesSpawned = 0;
        OnBossSpawned += OnBossSpawnedF;
        OnBossDefeated += OnBossDefeatedF;

        RestartManager.Instance.BeforeRestart += OnDestroy;
        GameUiManager.OnContinueToPlayAfterBoss += OnContinueToPlayAfterBoss;
    }

    private void OnDestroy()
    {
        OnBossSpawned -= OnBossSpawnedF;
        OnBossDefeated -= OnBossDefeatedF;
        GameUiManager.OnContinueToPlayAfterBoss -= OnContinueToPlayAfterBoss;

        RestartManager.Instance.BeforeRestart -= OnDestroy;
    }

    public void OnContinueToPlayAfterBoss()
    {
        rightWall.SetActive(false);
        leftWall.SetActive(false);
        bossHealthBar.Clear();

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        cameraFollow.transform.DOMoveX(Player.Instance.transform.position.x + cameraFollow.Offset.x, 3f)
            .OnComplete(() =>
            {
                Vector3 wallPosition = Wall.Instance.transform.position;
                wallPosition.x = Player.Instance.transform.position.x - cameraFollow.Offset.x - 6f;
                Wall.Instance.transform.position = wallPosition;

                OnReadyToSpawn?.Invoke();
                CurrentBossPattern.finished = true;
                ReadyToSpawn = true;
            });
    }

    public void OnBossSpawnedF()
    {
        rightWall.SetActive(true);
        leftWall.SetActive(true);
    }

    private void OnBossDefeatedF()
    {
        var toUnlock = PersistentData.Instance.BossToUnlockOnDefeat;
        if (string.IsNullOrWhiteSpace(toUnlock)) return;

        var unlockTargets = toUnlock.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var unlockTarget in unlockTargets)
        {
            var key = unlockTarget.Trim();
            if (key.Length == 0) continue;

            PlayerPrefs.SetInt(key, 0);
        }

        PlayerPrefs.Save();
        Debug.Log($"[BossUnlock] Defeated '{PersistentData.Instance.CurrentBossName}'. Unlocked: {toUnlock}");
    }

    public static int GetDifficultyInfluencedInt(float[] thresholds)
    {
        List<int> randomInts = new();
        for (int i = 0; i < Instance.bossesSpawned + 1; i++) // How Many Rolls are given
        {
            float random = UnityEngine.Random.value;
            int randomInt = 1;
            for (int j = 0; j < thresholds.Length; j++) // if the number is less than the threshold, returns the highest
            {
                if (random < thresholds[j])
                {
                    randomInt = j + 1;
                    break;
                }
            }
            if (randomInt >= thresholds.Length) //If it gets the highest possible answer, just return that. No need to run the later sort
                return randomInt;

            randomInts.Add(randomInt);
        }
        int largest = 0;
        foreach (int i in randomInts) // Get Highest int
        {
            if (i > largest)
                largest = i;
        }
        return largest;
    }
}
