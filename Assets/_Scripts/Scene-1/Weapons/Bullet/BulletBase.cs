using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public abstract class BulletBase : MonoBehaviour
{
    [SerializeField] protected float defaultMoveSpeed;
    [SerializeField] protected float maxTravelRange;
    private Vector2 _startPos;
    protected float moveSpeed;
    private bool _rotationIsSet;
    private Animator _animator;
    private int _triggerCount;
    [SerializeField] private int _maxTriggerTimes;

    [SerializeField] protected GameObject particleToSpawn; //Note: Ada ParticleSystem. Coba cari2 tahu tentang itu
    [SerializeField] protected float particleSpawnRate = 0.2f;

    public int id { get; private set; }

    private const string DestroyTrigger = "DestroyTrigger";

    protected void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    protected void Update()
    {
        if (_rotationIsSet)
        {
            // Move bullet
            transform.position += moveSpeed * transform.right * Time.deltaTime;

            if(Vector2.Distance(transform.position, _startPos) > maxTravelRange)
            {
                Destroy(gameObject);
            }
        }
    }

    protected void OnTriggerEnter2D(Collider2D col)
    {
        if (_triggerCount >= _maxTriggerTimes) return;
        PlayAnimation();
        OnHit(col);
    }

    protected virtual void PlayAnimation()
    {
        //_animator.SetTrigger(DestroyTrigger);
    }

    protected virtual void SpawnParticle()
    {
        //Spawn gameobject particle... atau pelajari ParticleSystem dulu lah
    }

    protected abstract void OnHit(Collider2D col);

    protected virtual void OnEndOfTrigger()
    {
        //Dipanggil setelah OnHit
        if (_triggerCount + 1 < _maxTriggerTimes)
        {
            _triggerCount++;
        }
        else if (NetworkClient.Instance.isMaster) NetworkClient.Instance.DestroyBullet(id);
    }

    protected void RotateTowards(Vector2 target)
    {
        var angleDeg = Mathf.Atan2(target.y - transform.position.y, target.x - transform.position.x) * Mathf.Rad2Deg;
        SetRotation(angleDeg);
    }

    protected void SetRotation(float degree)
    {
        transform.rotation = Quaternion.Euler(0, 0, degree);
        _rotationIsSet = true;
    }

    protected void AddRotation(float degree)
    {
        SetRotation(degree + transform.rotation.z);
    }

    protected void Initialize(Vector2 targetPos, int newBulletId)
    {
        RotateTowards(targetPos);
        id = newBulletId;
        moveSpeed = defaultMoveSpeed;
        _startPos = transform.position;
    }

    private void OnDestroy()
    {
        UnitManager.Instance.RemoveBullet(id);
        Destroy(gameObject);
    }
}
