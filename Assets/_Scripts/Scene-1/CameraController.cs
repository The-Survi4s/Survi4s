using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Target follow --------------------------------------------------------------------
    private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] [Range(1, 10)] private float smoothFactor; 

    // Eazy access ----------------------------------------------------------------------
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
            Vector3 targetPos = target.position + offset;
            Vector3 smoothPos = Vector3.Lerp(transform.position, targetPos, smoothFactor * Time.fixedDeltaTime);
            transform.position = smoothPos;
        }
    }

    public void SetTargetFollow (Transform target)
    {
        this.target = target;
    }
}
