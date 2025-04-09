using UnityEngine;
using UnityEngine.Events;

public class Tripwire : MonoBehaviour
{
    [Header("Tripwire Settings")]
    [Tooltip("Event triggered when an object with Health component crosses the tripwire.")]
    public UnityEvent<GameObject> OnTripwireTriggered;

    [SerializeField] private bool visualizeInEditor = true;
    [Tooltip("Child object defining the end of the tripwire. If not set, the line won't be drawn.")]
    [SerializeField] private Transform endPoint;
    [SerializeField] private Color lineColor = Color.red;

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsAlive)
        {
            Debug.Log($"Tripwire triggered by {other.gameObject.name}");
            isTriggered = true;
            OnTripwireTriggered?.Invoke(other.gameObject); 
            // gameObject.SetActive(false);
        }
    }

    public void ResetTripwire()
    {
        isTriggered = false;
        // gameObject.SetActive(true);
        Debug.Log("Tripwire Reset");
    }

    private void OnDrawGizmos()
    {
        if (visualizeInEditor && endPoint != null)
        {
            Gizmos.color = lineColor;
            Gizmos.DrawLine(transform.position, endPoint.position);
        }
    }
} 