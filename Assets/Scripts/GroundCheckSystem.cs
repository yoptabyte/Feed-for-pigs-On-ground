using UnityEngine;

[RequireComponent(typeof(GroundCheckData))]
public class GroundCheckSystem : MonoBehaviour
{
    private GroundCheckData groundCheckData;
    private Transform checkPoint;

    void Awake()
    {
        groundCheckData = GetComponent<GroundCheckData>();
        checkPoint = groundCheckData.groundCheckPoint != null ? groundCheckData.groundCheckPoint : transform;
    }

    void Update()
    {
        groundCheckData.isGrounded = PerformGroundCheck(
            checkPoint.position,
            groundCheckData.groundCheckDistance,
            groundCheckData.groundLayerMask
        );
    }

    public static bool PerformGroundCheck(Vector3 position, float distance, LayerMask layerMask)
    {
        return Physics.CheckSphere(position, distance, layerMask, QueryTriggerInteraction.Ignore);
        // return Physics.Raycast(position, Vector3.down, distance, layerMask, QueryTriggerInteraction.Ignore);
    }
    void OnDrawGizmosSelected()
    {
        if (checkPoint == null) checkPoint = transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(checkPoint.position, GetComponent<GroundCheckData>()?.groundCheckDistance ?? 0.2f);
    }
} 