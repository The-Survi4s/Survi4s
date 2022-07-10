using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Target follow --------------------------------------------------------------------
    private Transform target;
    private Vector3 targetDir;
    [SerializeField] private Vector3 offset;
    [SerializeField, Range(1, 10)] private float smoothFactor;
    [SerializeField, Range(0, 10)] private float directionDistance = 5f;
    Vector3 oppositePos = Vector3.zero;

    // Easy access ----------------------------------------------------------------------
    public static CameraController Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void FixedUpdate()
    {
        if(target != null)
        {
            oppositePos = Vector3.Lerp(
                Vector3.Lerp(oppositePos, targetDir * directionDistance, smoothFactor * Time.fixedDeltaTime),
                oppositePos, 
                smoothFactor * Time.fixedDeltaTime);
            Vector3 targetPos = target.position + offset + oppositePos;
            Vector3 smoothPos = Vector3.Lerp(transform.position, targetPos, smoothFactor * Time.fixedDeltaTime);
            transform.position = smoothPos;
        }
    }

    public void SetTargetFollow (Transform target)
    {
        this.target = target;
    }

    public void SetTargetDir(Vector3 dir)
    {
        targetDir = dir;
    }
}
