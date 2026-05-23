using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyScaler : MonoBehaviour
{
    float difficulty;
    public float DistanceToBoss = 400f;
    public float minDifficulty;
    public float maxDifficulty;
    public float maxDifficultyDistance = 2000f;
    private float oneOverMaxDifDist;
    private bool diffCalculatedThisFrame = false;

    private void Awake()
    {
        oneOverMaxDifDist = 1 / maxDifficultyDistance;
    }
    public void CalculateDifficulty()
    {
        if (diffCalculatedThisFrame)
            return;
        difficulty = Player.Instance.DistanceStatistic * oneOverMaxDifDist;
        minDifficulty = Mathf.Lerp(-.3f,.5f, difficulty);
        maxDifficulty = Mathf.Lerp(0f, 1f, difficulty);
        diffCalculatedThisFrame = true;
    }

    public bool IsInDifficultyRange(float difficultyLevel)
    {
        CalculateDifficulty();
        return difficultyLevel >= minDifficulty && difficultyLevel <= maxDifficulty;
    }

    private void LateUpdate()
    {
        diffCalculatedThisFrame = false;  
    }
}
