using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviourSingleton<CameraFollow>
{
    [SerializeField] private float leftThreshold = 0.25f;
    [SerializeField] private float rightThreshold = 0.35f;
    [SerializeField] float topThreshold = .45f, bottomThreshold = .15f, yMax = 0f, yMin = 0f;   

    // Debug variables
    private float viewportX;
    private float playerDeltaX;

    private bool isMoving;
    public bool IsMoving => isMoving;
    private Vector2 shakeOffset = new();

    public Vector3 Offset => new Vector3(Camera.main.orthographicSize * Camera.main.aspect * .5f, 0, -20f);

    private void LateUpdate()
    {
        if (Player.Instance.BossActive)
        {
            transform.position += (Vector3)shakeOffset;
            return;
        }
        Vector3 cameraPosition = transform.position;
        
        viewportX = Camera.main.WorldToViewportPoint(Player.Instance.transform.position).x;
        playerDeltaX = Player.Instance.transform.position.x - Player.Instance.LastPosition.x;

        bool shouldMoveCamera = viewportX < leftThreshold || viewportX > rightThreshold;
        
        isMoving = shouldMoveCamera;
        if (shouldMoveCamera)
        {
            float targetX = Player.Instance.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect * (leftThreshold + rightThreshold - 1);
            float targetY = Player.Instance.transform.position.y - (Camera.main.orthographicSize / Camera.main.aspect) * (topThreshold + bottomThreshold - 1);
            cameraPosition.x = Mathf.Lerp(cameraPosition.x, targetX, Time.deltaTime * 5f); // Smooth camera movement
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetY, Time.deltaTime * 5f);
            cameraPosition.y = Mathf.Clamp(cameraPosition.y, yMin, yMax);
        }

        transform.position = new Vector3(cameraPosition.x, cameraPosition.y, -20f) + (Vector3)shakeOffset;
    }

    public void Screenshake(float duration, Vector2 intensity)
    {
        StartCoroutine(Co_Screenshake(duration, intensity));
    }

    private IEnumerator Co_Screenshake(float duration, Vector2 intensity)
    {
        var startPosition = transform.position;
        

        float timer = 0f;
        while(timer < duration)
        {
            shakeOffset.x = Mathf.Sin(timer * 100f) * intensity.x;
            shakeOffset.y = Mathf.Sin(timer * 100f) * intensity.y;
            shakeOffset *= (duration - timer) / duration;
            timer += Time.deltaTime;
            yield return null;
        }
        shakeOffset = Vector2.zero;

        if (Player.Instance.BossActive)
            transform.position = startPosition;
    }

}
