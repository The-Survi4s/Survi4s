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

    public static async void UpdateNavMesh()
    {
        Debug.Log("Updating NavMesh...");
        await System.Threading.Tasks.Task.Delay(200);
        _surface.BuildNavMesh();
        Debug.Log("Build Completed");
    }
}
