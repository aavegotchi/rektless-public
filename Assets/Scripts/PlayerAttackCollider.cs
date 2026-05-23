using UnityEngine;

public class PlayerAttackCollider : MonoBehaviour
{
    private Vector3 offset;

    private void Awake()
    {
        transform.parent = null;
        offset = transform.position - Player.Instance.transform.position;
        offset.y *= -1; // only x-axis needs to be subtracted, when player is flipped, this way y will stay the same
        
    }

    private void Update()
    {
        if (Player.Instance.transform.rotation.y == 0)
        {
            transform.position = Player.Instance.transform.position + offset;
        }
        else
        {
            transform.position = Player.Instance.transform.position - offset;
        }
    }
}