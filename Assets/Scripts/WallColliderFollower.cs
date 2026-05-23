using UnityEngine;

public class WallColliderFollower : MonoBehaviour
{
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - Wall.Instance.transform.position;
    }

    void Update()
    {
        var position = Wall.Instance.transform.position;
        transform.position = new Vector3(position.x + offset.x, 0, -20f);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Player.Instance.BossActive) return;

        if (other.gameObject.CompareTag("Player"))
        {
            Wall.Instance.StartAttack();
        }
    }
}