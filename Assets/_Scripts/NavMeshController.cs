using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshController : MonoBehaviour
{
    private static NavMeshSurface2d _surface2d;
    private void Awake()
    {
        _surface2d = GetComponent<NavMeshSurface2d>();
        UpdateNavMesh();
    }

    public static void UpdateNavMesh()
    {
        _surface2d.BuildNavMesh();
    }
}
