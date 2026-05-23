using UnityEngine;

public class MorningStart : MonoBehaviour, IDestroyable
{
    public float rotationSpeed = 45f; // Degrees per second
    private float minRotation = 90f;
    private float maxRotation = 270f;
    private bool movingClockwise = true;
    private bool facingUp;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        // Determine the initial facing direction
        float startRotation = transform.rotation.eulerAngles.z;
        facingUp = Mathf.Approximately(startRotation, 0f);

        if (!facingUp && !Mathf.Approximately(startRotation, 180f))
        {
            Debug.LogWarning("Start rotation should be 0 or 180 degrees.");
        }
    }

    private void Update()
    {
        float currentRotation = transform.rotation.eulerAngles.z;
        float newRotation;

        if (facingUp)
        {
            // For objects facing up, we need to adjust our perspective
            if (currentRotation > 180f) currentRotation -= 360f;

            if (movingClockwise)
            {
                newRotation = currentRotation + rotationSpeed * Time.deltaTime;
                if (newRotation > 90f)
                {
                    newRotation = 90f;
                    movingClockwise = false;
                }
            }
            else
            {
                newRotation = currentRotation - rotationSpeed * Time.deltaTime;
                if (newRotation < -90f)
                {
                    newRotation = -90f;
                    movingClockwise = true;
                }
            }
        }
        else
        {
            // For objects facing down, we keep the original logic
            if (movingClockwise)
            {
                newRotation = currentRotation + rotationSpeed * Time.deltaTime;
                if (newRotation > maxRotation)
                {
                    newRotation = maxRotation;
                    movingClockwise = false;
                }
            }
            else
            {
                newRotation = currentRotation - rotationSpeed * Time.deltaTime;
                if (newRotation < minRotation)
                {
                    newRotation = minRotation;
                    movingClockwise = true;
                }
            }
        }

        transform.rotation =
            Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, newRotation);
    }

    public void Die()
    {
        boxCollider.enabled = false;
        Destroy(gameObject);
    }
}