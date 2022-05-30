using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshController : MonoBehaviour
{
    private static NavMeshSurface _surface;
    private void Awake()
    {
        _surface = GetComponent<NavMeshSurface>();
        _surface.hideEditorLogs = true;
        UpdateNavMesh();
    }

    public static void UpdateNavMesh()
    {
        _surface.BuildNavMesh();
    }
}
