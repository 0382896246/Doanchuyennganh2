using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepVisual : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsGround;

    [SerializeField] private TrailRenderer leftFoot;
    [SerializeField] private TrailRenderer rightFoot;

    [Range(0.001f,0.3f)]
    [SerializeField] private float checkRadius=0.05f;
    [Range(-.15f, .15f)]
    [SerializeField] private float raycastDistance = -0.05f;

    private void Update()
    {
        CheckFootStep(rightFoot);
        CheckFootStep(leftFoot);
    }

    private void CheckFootStep(TrailRenderer foot)
    {
        Vector3 checkPoisition =foot.transform.position+ Vector3.down*raycastDistance;

        bool touchingGround= Physics.CheckSphere(checkPoisition, checkRadius,whatIsGround);

        // bật hiệu ứng dấu chân nếu kiểm tra vị trí dưới chân là đất
        foot.emitting= touchingGround;
    }

    private void OnDrawGizmos()
    {
        DrawFootGizmos(leftFoot.transform);
        DrawFootGizmos(rightFoot.transform);
    }

    private void DrawFootGizmos(Transform foot)
    {
        if (foot == null) return;

        Gizmos.color = Color.blue;
        Vector3 checkPosition= foot.transform.position + Vector3.down * raycastDistance;

        Gizmos.DrawWireSphere(checkPosition, checkRadius);
    }
}
