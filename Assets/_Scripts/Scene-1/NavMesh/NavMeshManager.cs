using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using System.Threading.Tasks;

public class NavMeshManager : MonoBehaviour
{
    [SerializeField] private GameObject _navMeshSurfacePrefab;
    [SerializeField] private GameObject _navMeshLinkPrefab;
    [SerializeField, Min(0)] private float _pointDiff = 0.2f;
    [SerializeField] private Tilemap _tilemap;
    [Header("Debug")]
    [SerializeField] private Vector3 _size;
    [SerializeField] private Bounds _bounds;

    private Dictionary<Vector2, NavMeshSurface> _navMeshList = new Dictionary<Vector2, NavMeshSurface>();
    private List<NavMeshLink> _navLinkList = new List<NavMeshLink>();

    public static NavMeshManager Instance { get; private set; }
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

    private void Start()
    {
        Draw();

        if (!(_navMeshSurfacePrefab && _navMeshSurfacePrefab.TryGetComponent(out NavMeshSurface surface))) return;
        if(!(_navMeshLinkPrefab && _navMeshLinkPrefab.TryGetComponent(out NavMeshLink link))) return;

        _size = surface.size;
        SprinkleNavMeshes();
    }

    private void Update()
    {
    }

    private async void SprinkleNavMeshes()
    {
        for (float x = _bounds.min.x + _size.x / 2; x < _bounds.max.x; x += _size.x)
        {
            for (float y = _bounds.min.y + _size.y / 2; y < _bounds.max.y; y += _size.y)
            {
                // Instantiate NavMesh
                var loc = new Vector2(x, y);
                var go = Instantiate(_navMeshSurfacePrefab, loc, Quaternion.identity, this.transform);
                go.transform.rotation = Quaternion.Euler(-90, 0, 0);
                var navMesh = go.GetComponent<NavMeshSurface>();
                navMesh.collectObjects = CollectObjects.Volume;
                navMesh.BuildNavMeshAsync();
                _navMeshList.Add(loc, navMesh);

                if(x < _bounds.max.x - _size.x)
                {
                    InstantiateLink(loc, go.transform, true);
                }
                if (y < _bounds.max.y - _size.y / 2)
                {
                    InstantiateLink(loc, go.transform, false);
                }
                await Task.Delay(100);
            }
        }
    }

    private void InstantiateLink(Vector2 loc, Transform parent, bool facingRight)
    {
        var go2 = Instantiate(_navMeshLinkPrefab, loc, Quaternion.identity, parent);
        go2.transform.rotation = Quaternion.Euler(-90, -90, 90);
        var link = go2.GetComponent<NavMeshLink>();
        _navLinkList.Add(link);

        link.startPoint = new Vector3(
            facingRight ? _size.x / 2 - _pointDiff / 2 : 0, 
            0, 
            facingRight ? 0 : _size.y / 2 - _pointDiff / 2);
        link.endPoint = new Vector3(
            facingRight ? _size.x / 2 + _pointDiff / 2 : 0,
            0,
            facingRight ? 0 : _size.y / 2 + _pointDiff / 2);
        link.width = _size.x;
    }

    public void UpdateNavMesh(Vector2 point)
    {
        var currentPos = point;
        var newPos = new Vector2(
            Mathf.Round(currentPos.x / _size.x) * _size.x,
            Mathf.Round(currentPos.y / _size.y) * _size.y);
        Debug.Log("Is there a NavMesh at " + newPos + "? " + _navMeshList.ContainsKey(newPos));
        if (_navMeshList.ContainsKey(newPos))
        {
            _navMeshList[newPos].BuildNavMeshAsync();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Draw();
    }

    void Draw()
    {
        if (_tilemap == null)
            return;

        // tilemap position
        var tp = _tilemap.transform.position;

        // bounds + offset
        var tBounds = _tilemap.cellBounds;

        // corner points
        var c0 = new Vector3(tBounds.min.x, tBounds.min.y) + tp;
        var c1 = new Vector3(tBounds.min.x, tBounds.max.y) + tp;
        var c2 = new Vector3(tBounds.max.x, tBounds.max.y) + tp;
        var c3 = new Vector3(tBounds.max.x, tBounds.min.y) + tp;

        // draw borders
        Debug.DrawLine(c0, c1, Color.red);
        Debug.DrawLine(c1, c2, Color.red);
        Debug.DrawLine(c2, c3, Color.red);
        Debug.DrawLine(c3, c0, Color.red);

        // draw origin cross
        Debug.DrawLine(new Vector3(tp.x, tBounds.min.y + tp.y), new Vector3(tp.x, tBounds.max.y + tp.y), Color.green);
        Debug.DrawLine(new Vector3(tBounds.min.x + tp.x, tp.y), new Vector3(tBounds.max.x + tp.x, tp.y), Color.green);

        _bounds = new Bounds
        {
            min = c0,
            max = c2
        };

        if (_navMeshSurfacePrefab && _navMeshSurfacePrefab.TryGetComponent(out NavMeshSurface surface))
        {
            _size = surface.size;
        }
    }
}
