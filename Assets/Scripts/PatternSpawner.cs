using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class BossPatternInfo
{
    public GameObject prefab;
    public int patternCount;
}

public class PatternSpawner : MonoBehaviour
{
    private static float CAMERA_HALF_WIDTH = 9f;
    [SerializeField] D_LevelConfig levelConfig;
    private List<IPattern> patternPool = new();
    private Dictionary<D_Spawnable, List<GameObject>> PrefabsBySpawnables = new();
    [SerializeField] List<GameObject> spawnablePrefabs;
    private List<IPattern> activePatterns = new();
    private List<string> spawnedPatternNames = new();
    private IPattern currentPattern;
    private bool spawnStarterNext = false;
    public int currentPatternCount = 0;
    private bool waitingForBossToFinish = false;
    [SerializeField] GameObject meteorPrefab;
    private DifficultyScaler difficultyScaler;
    [SerializeField] bool testingBoss;

    private void Start()
    {
        difficultyScaler = GetComponent<DifficultyScaler>();
        currentPatternCount = 0;
        levelConfig = PersistentData.Instance.CurrentLevelConfig;
        LoadPrefabs();
        LoadPatternPool();

        Instantiate(levelConfig.BackgroundPrefab);

        if (levelConfig.StarterPrefab) 
        {
            SpawnPattern(levelConfig.StarterPrefab.GetComponent<IPattern>());
            currentPatternCount++;
        }
        else CheckAndSpawn();
    }

    private void Update()
    {
        if (!Player.Instance.gameObject.activeInHierarchy) return;
        if (Player.Instance.BossActive) return;
        if (!BossSpawnManager.Instance.ReadyToSpawn) return;
        if (waitingForBossToFinish) return;
        CheckAndSpawn();

        foreach (var pattern in activePatterns)
            if (Wall.Instance.transform.position.x - pattern.RightAlignmentPoint.x > CAMERA_HALF_WIDTH)
                pattern.Deactivate();
    }

    private void LoadPrefabs()
    {
        PrefabsBySpawnables.Clear();
        spawnablePrefabs = Resources.LoadAll<GameObject>(levelConfig.PrefabFolderPath).ToList();
        var universalPrefabs = Resources.LoadAll<GameObject>("Prefabs/AllLevels");
        foreach (var prefab in universalPrefabs)
            spawnablePrefabs.Add(prefab);

        foreach (var prefab in spawnablePrefabs)
        {
            if (prefab.TryGetComponent<Spawnable>(out var spawnable) && spawnable.SpawnableType != null)
            {
                if (!PrefabsBySpawnables.ContainsKey(spawnable.SpawnableType))
                    PrefabsBySpawnables.Add(spawnable.SpawnableType, new List<GameObject>());
                PrefabsBySpawnables[spawnable.SpawnableType].Add(prefab);
            }
        }
    }

    private void LoadPatternPool()
    {
        List<GameObject> patterns = new();

        GameObject[] patternPrefabs = Resources.LoadAll<GameObject>("PatternPrefabs/AllLevels");
        foreach (var prefab in patternPrefabs)
            patterns.Add(prefab);

        GameObject[] exclusivePatternPrefabs = Resources.LoadAll<GameObject>(levelConfig.ExclusivePatternPath);
        foreach (var prefab in exclusivePatternPrefabs)
            patterns.Add(prefab);

        patternPool.Clear();
        for (int i = 0; i < patterns.Count; i++)
        {
            if (patterns[i].TryGetComponent(out IPattern pattern))
            {
                patternPool.Add(pattern);
            }
        }
    }

    private void CheckAndSpawn()
    {
        //if (prefabPool.Count == 0) return;
        if (currentPattern == null || currentPattern.IsFinished())
        {
            if (spawnStarterNext && levelConfig.StarterPrefab)
            {
                SpawnPattern(levelConfig.StarterPrefab.GetComponent<IPattern>());
                spawnStarterNext = false;
                currentPatternCount++;
            }
            else if (ShouldSpawnBossPattern())
            {
                
                StartCoroutine(Co_SpawnBossPattern());
            }
            else
            {
                SpawnRandomPattern();
                currentPatternCount++;
            }
        }
    }

    private bool ShouldSpawnBossPattern()
    {
#if UNITY_EDITOR
        if (testingBoss)
            return true;
#endif
            int bossesSpawned = BossSpawnManager.Instance.bossesSpawned;
        return levelConfig.BossPrefab 
            && Player.Instance.DistanceStatistic > ((bossesSpawned + 1) * difficultyScaler.DistanceToBoss) + bossesSpawned * difficultyScaler.DistanceToBoss * .2f;
    }

    private System.Collections.IEnumerator Co_SpawnBossPattern()
    {
        //BossPatternInfo selectedBossPattern = availableBossPatterns[Random.Range(0, availableBossPatterns.Count)];
        Debug.Log("spawning boss pattern");
        SpawnPatternAsPrefab(levelConfig.MeteorPattern);
        yield return new WaitForSeconds(.01f);
        SpawnPatternAsPrefab(levelConfig.BossPrefab);
        waitingForBossToFinish = true;
        StartCoroutine(WaitForBossToFinish());
    }

    private System.Collections.IEnumerator WaitForBossToFinish()
    {
        yield return new WaitUntil(() => currentPattern.IsFinished());
        waitingForBossToFinish = false;
        currentPatternCount++; // Increment after boss pattern finishes
    }

    private void SpawnRandomPattern()
    {
        if (patternPool.Count == 0) return;
        List<IPattern> validPatterns = new();
        List<IPattern> validPatternsWithRepeats = new();

        foreach(IPattern pattern in patternPool)
        {
            //Debug.Log(pattern.GetGameObject().name + " " + pattern.Difficulty);
            
            
            if (difficultyScaler.IsInDifficultyRange(pattern.Difficulty)) // if the pattern is within difficulty ratings
            {
                if (!spawnedPatternNames.Contains(pattern.GetGameObject().name)) // If the pattern has already been spawned, ignore
                    validPatterns.Add(pattern);

                validPatternsWithRepeats.Add(pattern);
            }
                
        }

        IPattern toSpawn = null;
        if (validPatterns.Count == 0) 
            toSpawn = validPatternsWithRepeats[Random.Range(0, validPatternsWithRepeats.Count)];
        else 
            toSpawn = validPatterns[Random.Range(0, validPatterns.Count)];

        if (toSpawn != null)
        {
            SpawnPattern(toSpawn);
            SpawnOptionalMeteors();
        }
    }

    private void SpawnOptionalMeteors()
    {
        if (!levelConfig.IncludeMeteors)
            return;

        for (int i = 0; i < currentPatternCount; i++)
        {
            if (UnityEngine.Random.value < .1f)
                GameObject.Instantiate(levelConfig.MeteorPrefab,
                    activePatterns[^1].GetGameObject().transform.position + (Vector3.up * 10),
                    Quaternion.identity,
                    activePatterns[^1].GetGameObject().transform);
        }
    }

    public void SpawnPattern(IPattern patternToSpawn)
    {

        Vector2 spawnPosition = Camera.main.transform.position;
        if (currentPattern != null)
            spawnPosition = currentPattern.RightAlignmentPoint;

        GameObject patternParent = new(patternToSpawn.GetGameObject().name);
        patternParent.transform.position = spawnPosition;

        switch (patternToSpawn.PatternType)
        {
            case PatternType.Default: patternParent.AddComponent<Pattern>(); break;
            case PatternType.Boss: patternParent.AddComponent<BossPattern>(); break;
            case PatternType.Meteor: patternParent.AddComponent<MeteorPattern>(); break;
        }
        IPattern newPattern = patternParent.GetComponent<IPattern>();

        var patternDictionary = patternToSpawn.PositionsBySpawnable();

        List<BoxCollider2D> grounds = new();
        foreach (D_Spawnable spawnable in patternDictionary.Keys)
        {
            D_Spawnable thisSpawnable = TrySwapSpawnableRecursive(spawnable);
            if (thisSpawnable == null)
                continue;

            foreach (var transformData in patternDictionary[spawnable])
            {
                if (!PrefabsBySpawnables.ContainsKey(thisSpawnable))
                    continue;

                GameObject toSpawn = PrefabsBySpawnables[thisSpawnable][UnityEngine.Random.Range(0, PrefabsBySpawnables[thisSpawnable].Count)];
                var go = Instantiate(toSpawn,
                        spawnPosition + transformData.pos,
                        transformData.rot, patternParent.transform);
                if (go.TryGetComponent<BoxCollider2D>(out var box) && go.layer == 9)
                    grounds.Add(box);
            }
        }
        ConstructPatternJoinPoints(patternParent, newPattern, grounds);

        if (currentPattern != null)
            patternParent.transform.Translate(currentPattern.RightAlignmentPoint - newPattern.LeftAlignmentPoint);

        currentPattern = newPattern;
        activePatterns.Add(currentPattern);
        spawnedPatternNames.Add(patternToSpawn.GetGameObject().name);

        for (int i = 0; i < patternParent.transform.childCount; i++)
        {
            if (patternParent.transform.GetChild(i).gameObject.layer == 9 
                && patternParent.transform.GetChild(i).gameObject.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr.sortingOrder += i;
            }
        }

    }

    private void ConstructPatternJoinPoints(GameObject patternParent, IPattern newPattern, List<BoxCollider2D> grounds)
    {
        BoxCollider2D leftest = grounds[0], rightest = grounds[0];
        foreach (BoxCollider2D box in grounds)
        {
            if (box.transform.position.x < leftest.transform.position.x)
                leftest = box;
            if (box.transform.position.x > rightest.transform.position.x)
                rightest = box;
        }

        GameObject leftJoinPoint = new GameObject("Left Point");
        leftJoinPoint.transform.parent = patternParent.transform;
        leftJoinPoint.transform.position = new Vector2(leftest.transform.position.x - leftest.size.x, leftest.transform.position.y);
        GameObject rightJoinPoint = new GameObject("RightPoint");
        rightJoinPoint.transform.parent = patternParent.transform;
        rightJoinPoint.transform.position = new Vector2(rightest.transform.position.x + rightest.size.x, rightest.transform.position.y);

        newPattern.SetBoundaries(leftJoinPoint.transform, rightJoinPoint.transform);
    }

    public void SpawnPatternAsPrefab(GameObject prefab)
    {
        Vector2 spawnPosition = Camera.main.transform.position;
        if (currentPattern != null)
        {
            spawnPosition = currentPattern.RightAlignmentPoint;
        }

        var go = Instantiate(prefab,
            new Vector3(spawnPosition.x, spawnPosition.y, prefab.transform.position.z),
            Quaternion.identity, transform);
        if (go.TryGetComponent(out IPattern pattern))
        {
            if (currentPattern != null)
                go.transform.Translate(currentPattern.RightAlignmentPoint - pattern.LeftAlignmentPoint);

            currentPattern = pattern;
            activePatterns.Add(currentPattern);
        }
        else
        {
            Debug.LogError("Prefab does not have a Pattern component: " + prefab.name);
        }
    }

    public D_Spawnable TrySwapSpawnableRecursive(D_Spawnable spawnable)
    {
        D_Spawnable toSpawn = spawnable;
       // Debug.Log(spawnable);
        if (!PrefabsBySpawnables.ContainsKey(spawnable)
                    || PrefabsBySpawnables[spawnable] == null
                    || PrefabsBySpawnables[spawnable].Count <= 0)
        {
            toSpawn = null;
            if (spawnable.substitutes != null && spawnable.substitutes.Length > 0)
            {
                toSpawn = spawnable.substitutes[0];
            }
        }

        return toSpawn;
    }
    
}