using UnityEngine;

public class CloudMovementBackwardFirst : MonoBehaviour
{
    public float speed = 2.0f; 
    public float forwardZLimit = 130.0f;
    public float backwardZLimit = -80.0f;

    private bool movingForward = false;

    void Update()
    {
        Vector3 currentPosition = transform.position;
        float targetZ = movingForward ? forwardZLimit : backwardZLimit;
        Vector3 targetPosition = new Vector3(currentPosition.x, currentPosition.y, targetZ);

        transform.position = Vector3.MoveTowards(currentPosition, targetPosition, speed * Time.deltaTime);

        if (movingForward && transform.position.z >= forwardZLimit)
        {
            movingForward = false; 
        }
        else if (!movingForward && transform.position.z <= backwardZLimit)
        {
            movingForward = true; 
        }
    }
}