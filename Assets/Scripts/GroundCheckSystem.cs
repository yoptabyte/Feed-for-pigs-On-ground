using UnityEngine;

[RequireComponent(typeof(GroundCheckData))]
public class GroundCheckSystem : MonoBehaviour
{
    private GroundCheckData groundCheckData;
    private Transform checkPoint;
    private Collider[] _hitColliders = new Collider[1];

    void Awake()
    {
        groundCheckData = GetComponent<GroundCheckData>();
        checkPoint = groundCheckData.groundCheckPoint != null ? groundCheckData.groundCheckPoint : transform;
    }

    void Update()
    {
        int hits = Physics.OverlapSphereNonAlloc(
            checkPoint.position,
            groundCheckData.groundCheckDistance,
            _hitColliders,
            groundCheckData.groundLayerMask,
            QueryTriggerInteraction.Ignore
        );

        if (hits > 0)
        {
            groundCheckData.isGrounded = true;
            Collider groundCollider = _hitColliders[0];

            SurfaceData surfaceData = groundCollider.GetComponentInParent<SurfaceData>();

            if (surfaceData != null)
            {
                groundCheckData.CurrentSurfaceType = surfaceData.Type;
                groundCheckData.CurrentSurfaceData = surfaceData;
            }
            else
            {
                groundCheckData.CurrentSurfaceType = SurfaceType.Ground;
                groundCheckData.CurrentSurfaceData = null;
            }
            _hitColliders[0] = null;
        }
        else
        {
            groundCheckData.isGrounded = false;
            groundCheckData.CurrentSurfaceType = SurfaceType.Ground;
            groundCheckData.CurrentSurfaceData = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (checkPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(checkPoint.position, groundCheckData != null ? groundCheckData.groundCheckDistance : GetComponent<GroundCheckData>().groundCheckDistance);
    }
} 