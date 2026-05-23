using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="newPatternGroup", menuName = "Data/Patterns")]
public class D_LevelConfig : ScriptableObject
{
    [Header("Leaderboard")]
    public string LeaderboardKey;

    public string PrefabFolderPath;
    public string ExclusivePatternPath;
    public GameObject BackgroundPrefab;
    public GameObject StarterPrefab;
    public GameObject[] ExclusivePatternPrefabs;
    public GameObject MeteorPattern;
    public GameObject BossPrefab;
    public RuntimeAnimatorController GemAnimator;
    public Texture PortalTexture;

    [Header("Meteors")]
    public bool IncludeMeteors;
    public GameObject MeteorPrefab;

    [Header("UI")]
    public Color UIColor = Color.white;
    public Sprite DistanceFlagUISprite;
    public Sprite GemUISprite;
    public GameObject BossHealthBarPrefab;
}
