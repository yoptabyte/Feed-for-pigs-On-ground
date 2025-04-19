using UnityEngine;

public class WindmillRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Transform rotatingPart;
    public float rotationSpeed = 1.0f; 

    [Header("Secondary Rotation Settings")]
    public Transform movingPart; 
    public float secondaryRotationSpeed = 1.0f; 

    private float totalRotationZ = 0f; 
    private int rotationDirection = 1; 
    private const float rotationLimit = 360.0f; 

    void Update()
    {
        
        if (rotatingPart != null)
        {
            float rotationThisFrame = rotationSpeed * rotationDirection * Time.deltaTime;
            rotatingPart.Rotate(Vector3.forward * rotationThisFrame, Space.Self);
            totalRotationZ += rotationThisFrame;
        }
        else
        {
            Debug.LogWarning("Rotating part (blades) is not assigned in the WindmillRotation script on " + gameObject.name);
            return;
        }

        if (movingPart != null)
        {
            if (rotationDirection == 1)
            {
                movingPart.Rotate(Vector3.up * secondaryRotationSpeed * Time.deltaTime, Space.Self);

                if (totalRotationZ >= rotationLimit)
                {
                    rotationDirection = -1; 
                    totalRotationZ = rotationLimit; 
                }
            }
            else
            {
                movingPart.Rotate(Vector3.down * secondaryRotationSpeed * Time.deltaTime, Space.Self); // Vector3.down для обратного вращения

                if (totalRotationZ <= -rotationLimit)
                {
                    rotationDirection = 1; 
                    totalRotationZ = -rotationLimit; 
                }
            }
        }
         else
        {
            Debug.LogWarning("Moving part is not assigned in the WindmillRotation script on " + gameObject.name);
        }
    }
}