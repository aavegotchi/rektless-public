using System.Collections.Generic;
using System.Linq;
using Prefabs.level2;
using UnityEngine;

public class MeteorPattern : MonoBehaviour, IPattern
{
    public D_Spawnable[] spawnables;
    public Vector2[] positions;
    public Quaternion[] rotations;

    public Transform rightAlignmentPoint, leftAlignmentpoint;

    [SerializeField] private float difficulty = 0f;
    float IPattern.Difficulty => difficulty;
    void IPattern.SetBoundaries(Transform leftJoinPoint, Transform rightJoinPoint)
    {
        leftAlignmentpoint = leftJoinPoint;
        rightAlignmentPoint = rightJoinPoint;
    }

    Vector2 IPattern.RightAlignmentPoint => rightAlignmentPoint.position;
    Vector2 IPattern.LeftAlignmentPoint => leftAlignmentpoint.position;


    PatternType IPattern.PatternType => PatternType.Meteor;

    bool IPattern.IsFinished() => finished;

    private bool playerCameToPattern = false;
    private bool finished = false;

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

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Player.Instance.gameObject.activeInHierarchy) return;
        if (Player.Instance.BossActive) return;
        if (Player.Instance.OnStarting) return;
        if (!BossSpawnManager.Instance.ReadyToSpawn) return;

        if (!playerCameToPattern && other.CompareTag("Player"))
        {
            playerCameToPattern = true;

            InvokeRepeating(nameof(CheckIfAllMeteorsGone), 0f, 1f);
        }
    }

    public void CheckIfAllMeteorsGone()
    {
        var meteors = FindObjectsOfType<Meteor>();
        if (meteors.Length == 0)
        {
            finished = true;
            CancelInvoke(nameof(CheckIfAllMeteorsGone));
        }
    }

    void IPattern.Deactivate() => gameObject.SetActive(false);

}