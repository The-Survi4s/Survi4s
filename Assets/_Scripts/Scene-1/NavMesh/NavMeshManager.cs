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
    [SerializeField, Min(1)] private int _linkDivision = 10;
    [SerializeField] private Vector2 _offset = new Vector2(-9.5f, -8.5f);
    [SerializeField] private Tilemap _tilemap;
    [Header("Debug")]
    [SerializeField] private Vector3 _size;
    [SerializeField] private Bounds _bounds;

    private Dictionary<Vector2, NavMeshSurface> _navMeshList = new Dictionary<Vector2, NavMeshSurface>();
    private List<NavMeshLink> _navLinkList = new List<NavMeshLink>();
    [SerializeField] private List<NavMeshSurface> _surfaceList = new List<NavMeshSurface>();

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

    private async void SprinkleNavMeshes()
    {
        for (float x = (int)(_bounds.min.x + _size.x / 2); x < _bounds.max.x + _size.x / 2; x += _size.x)
        {
            for (float y = (int)(_bounds.min.y + _size.y / 2); y < _bounds.max.y + _size.y / 2; y += _size.y)
            {
                // Instantiate NavMesh
                var loc = new Vector2(x, y);
                var go = Instantiate(_navMeshSurfacePrefab, loc, Quaternion.identity, this.transform);
                go.transform.rotation = Quaternion.Euler(-90, 0, 0);
                go.name = $"NavMeshSurface ({x}, {y})";
                var navMesh = go.GetComponent<NavMeshSurface>();
                navMesh.collectObjects = CollectObjects.Volume;
                navMesh.BuildNavMeshAsync();
                _navMeshList.Add(loc, navMesh);
                _surfaceList.Add(navMesh);

                if(x < _bounds.max.x - _size.x)
                {
                    InstantiateLink(go.transform, true);
                }
                if (y < _bounds.max.y - _size.y / 2)
                {
                    InstantiateLink(go.transform, false);
                }
                await Task.Delay(250);
            }
        }
    }

    private void InstantiateLink(Transform parent, bool facingRight)
    {
        var x = (facingRight ? _size.x / 2 : 0);
        var y = (facingRight ? 0 : _size.y / 2);
        var go2 = Instantiate(_navMeshLinkPrefab, Vector3.zero, Quaternion.identity, parent);
        go2.transform.localPosition = new Vector3(x, 0, y);
        go2.transform.Rotate(0, 90, 90);
        if(facingRight) go2.transform.Rotate(0, 90, 0);

        var link = go2.GetComponent<NavMeshLink>();
        _navLinkList.Add(link);

        link.startPoint = new Vector3(-_pointDiff, 0, 0);
        link.endPoint = new Vector3(_pointDiff, 0, 0);
        
        link.width = _size.x / _linkDivision;

        RaycastHit2D hit = Physics2D.Raycast(go2.transform.position, go2.transform.right);
        int tries = 0;
        while (hit && tries < 1000)
        {
            var dist = Vector2.Distance(go2.transform.position, hit.transform.position);
            if (dist > _size.x / _linkDivision) break;
            //Debug.Log($"Hit {hit.collider.gameObject} sejauh {dist}. R:{facingRight}");
            go2.transform.Translate(go2.transform.right * Random.Range(-10, 11));
            hit = Physics2D.Raycast(go2.transform.position, go2.transform.right);
            tries++;
        }
    }

    public void UpdateNavMesh(Vector2 point)
    {
        var currentPos = point;
        var newPos = new Vector2(
            Mathf.Round(currentPos.x / _size.x) * _size.x + _size.x / 2 + _offset.x,
            Mathf.Round(currentPos.y / _size.y) * _size.y + _size.y / 2 + _offset.y);
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
