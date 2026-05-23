using System.Collections.Generic;
using System.Linq;
using level1;
using UnityEngine;


public class BossPattern : MonoBehaviour, IPattern
{
    [SerializeField] private Prison prison;

    public D_Spawnable[] spawnables;
    public Vector2[] positions;
    public Quaternion[] rotations;
    

    public Transform rightAlignmentPoint, leftAlignmentpoint;
    void IPattern.SetBoundaries(Transform leftJoinPoint, Transform rightJoinPoint)
    {
        leftAlignmentpoint = leftJoinPoint;
        rightAlignmentPoint = rightJoinPoint;
    }

    Vector2 IPattern.RightAlignmentPoint => rightAlignmentPoint.position;
    Vector2 IPattern.LeftAlignmentPoint => leftAlignmentpoint.position;

    PatternType IPattern.PatternType { get; } = PatternType.Boss;

    [SerializeField] private float difficulty = 0f;
    float IPattern.Difficulty => difficulty;

    private bool playerCameToBossPattern = false;
    public bool finished = false;

    bool IPattern.IsFinished() => finished;

    Dictionary<D_Spawnable, List<(Vector2 pos, Quaternion rot)>> IPattern.PositionsBySpawnable() => Pattern.GetDictionary(spawnables, positions, rotations);

    GameObject IPattern.GetGameObject() => gameObject;

    public void SetUpArray()
    {
        List<D_Spawnable> spawnablesList = new();
        List<Vector2> positionsList = new();
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject && transform.TryGetComponent(out Spawnable spawnable))
            {
                spawnablesList.Add(spawnable.SpawnableType);
                positionsList.Add(transform.GetChild(i).position);
            }
        }
        spawnables = spawnablesList.ToArray();
        positions = positionsList.ToArray();
    }

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Enemy") || child.CompareTag("TouchEnemy") || child.CompareTag("Boss"))
            {
                child.gameObject.SetActive(false);
            }
        }

        BossSpawnManager.OnBossDefeated += OnBossDefeated;
    }

    private void Start()
    {
        var boxCollider = gameObject.AddComponent<BoxCollider2D>();

        boxCollider.size = new Vector2(-leftAlignmentpoint.localPosition.x, 100f);
        boxCollider.isTrigger = true;

        prison = GetComponentInChildren<Prison>();
        BossSpawnManager.Instance.bossesSpawned++;
    }



    private void OnBossDefeated()
    {
        if (playerCameToBossPattern && !finished)
        {
            // Kill all enemies under this pattern
            foreach (Transform child in transform)
            {
                if (child.CompareTag("Enemy") || child.CompareTag("TouchEnemy") || child.CompareTag("Boss"))
                {
                    if (child.TryGetComponent(out IDestroyable enemy))
                    {
                        enemy.Die();
                    }
                }
            }

            BossSpawnManager.OnBossDefeated -= OnBossDefeated;
            if (prison)
            {
                prison.OnBossDeath();
                return;
            }


            BossSpawnManager.Instance.OnContinueToPlayAfterBoss();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Player.Instance.gameObject.activeInHierarchy) return;
        if (Player.Instance.BossActive) return;
        if (Player.Instance.OnStarting) return;
        if (!BossSpawnManager.Instance.ReadyToSpawn) return;

        if (!playerCameToBossPattern && other.CompareTag("Player"))
        {
            playerCameToBossPattern = true;
            BossSpawnManager.Instance.ReadyToSpawn = false;

            // Kill all enemies
            var enemies = FindObjectsOfType<MonoBehaviour>().OfType<IDestroyable>();
            foreach (var enemy in enemies)
            {
                enemy.Die();
            }

            BossSpawnManager.OnBossSpawned?.Invoke();

            foreach (Transform child in transform)  
            {
                if (child.CompareTag("Enemy") || child.CompareTag("TouchEnemy") || child.CompareTag("Boss"))
                {
                    child.gameObject.SetActive(true);
                }
            }

            BossSpawnManager.Instance.CurrentBossPattern = this;

            if (prison)
                prison.StartMove();
        }
    }

    void IPattern.Deactivate() => gameObject.SetActive(false);

    private void OnDisable()
    {
        BossSpawnManager.OnBossDefeated -= OnBossDefeated;
    }

}