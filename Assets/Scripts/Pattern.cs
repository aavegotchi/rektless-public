using System.Collections.Generic;
using UnityEngine;
 
public class Pattern : MonoBehaviour, IPattern
{
    [Range(0f, 1f)] public float DifficultyRating;
    
    public D_Spawnable[] spawnables;
    public Vector2[] positions;
    public Quaternion[] rotations;

    private float startTime;

    private Transform rightAlignmentPoint = null, leftAlignmentpoint = null;

    float IPattern.Difficulty => DifficultyRating;
    void IPattern.SetBoundaries(Transform leftJoinPoint, Transform rightJoinPoint)
    {
        leftAlignmentpoint = leftJoinPoint;
        rightAlignmentPoint = rightJoinPoint;
    }

    Vector2 IPattern.RightAlignmentPoint => rightAlignmentPoint.position;
    Vector2 IPattern.LeftAlignmentPoint => leftAlignmentpoint.position;
    public PatternType PatternType { get; } = PatternType.Default;
    Dictionary<D_Spawnable, List<(Vector2 pos, Quaternion rot)>> IPattern.PositionsBySpawnable() => GetDictionary(spawnables, positions, rotations);
    GameObject IPattern.GetGameObject() => gameObject;

    public bool IsFinished()
    {
        return Player.Instance.transform.position.x - leftAlignmentpoint.position.x > 0;
    }

    private void OnEnable()
    {
        startTime = Time.time;
    }

    private void Update()
    {
        if (!Player.Instance.gameObject.activeInHierarchy) return;
        if (Player.Instance.BossActive) return;
        if (Player.Instance.OnStarting) return;
        if (!BossSpawnManager.Instance.ReadyToSpawn) return;
    }

    void IPattern.Deactivate() => gameObject.SetActive(false);

    public static Dictionary<D_Spawnable, List<(Vector2 pos, Quaternion rot)>> GetDictionary(D_Spawnable[] spawnables, Vector2[] positions, Quaternion[] rotations)
    {
        Dictionary<D_Spawnable, List<(Vector2 pos, Quaternion rot)>> spawnablePositions = new();

        for (int i = 0; i < spawnables.Length; i++)
        {
            if (!spawnablePositions.ContainsKey(spawnables[i]))
                spawnablePositions.Add(spawnables[i], new List<(Vector2 pos, Quaternion rot)>());

            spawnablePositions[spawnables[i]].Add((positions[i], rotations[i]));    
        }

        return spawnablePositions;
    }

    public void SetUpArray()
    {
        var pattern = this;
        // Get the reference to the Pattern component
        List<D_Spawnable> spawnablesList = new();
        List<Vector2> positionsList = new();
        List<Quaternion> rotationsList = new();
        for (int i = 0; i < pattern.transform.childCount; i++)
        {
            if (pattern.transform.GetChild(i).gameObject && pattern.transform.GetChild(i).gameObject.TryGetComponent(out Spawnable spawnable))
            {
                spawnablesList.Add(spawnable.SpawnableType);
                positionsList.Add(pattern.transform.GetChild(i).position);
                rotationsList.Add(pattern.transform.GetChild(i).rotation);
            }
        }
        pattern.spawnables = spawnablesList.ToArray();
        pattern.positions = positionsList.ToArray();
        pattern.rotations = rotationsList.ToArray();

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(transform.position + (Vector3.up * -5.259f), new Vector3(50f, 4.25f, 0));
        Gizmos.DrawWireCube(transform.position + (Vector3.up * 5.08f), new Vector3(50f, .01f, 0));
    }
}

