using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MonsterMovement),typeof(Animator))]
public abstract class Monster : MonoBehaviour
{
    public int id { get; private set; }

    [SerializeField] private LayerMask _playerLayerMask;
    [SerializeField] private LayerMask _wallLayerMask;
    [SerializeField] private LayerMask _monsterLayerMask;
    protected MonsterStat monsterStat;

    private Stat rawStat => monsterStat?.getRawStat ?? new Stat();
    public Stat currentStat => _currentStat;
    [SerializeField] private Stat _currentStat;
    protected MonsterMovement _monsterMovement;
    private Animator _animator;
    private List<StatusEffectBase> _activeStatusEffects;

    public enum Type {Kroco, Paskibra, Pramuka, Basket, Satpam, Musisi, TukangSapu, Futsal}
    public enum Target {Statue, Wall, Player}
    public enum TargetMethod {DontAttack, Nearest, Furthest, LowestHp}
    public Origin origin { get; private set; }

    [Serializable]
    public struct TargetSettings
    {
        public TargetSettings(Target target, TargetMethod method) : this()
        {
            this.target = target;
            this.method = method;
        }

        public Target target;
        public TargetMethod method;
    }
    [Serializable]
    public struct Setting
    {
        public Type type;
        public float detectionRange;
        public float attackRange;
        public float minRange;
        public Target priority;
        public List<TargetSettings> attackMethods;
        public Stat defaultStat;
        public bool doEvasion;

        public TargetMethod MethodOf(Target target) =>
            attackMethods.Where(ts => ts.target == target).Select(ts => ts.method).FirstOrDefault();
    }
    [field: SerializeField] public Setting setting { get; protected set; }
    public Stat defaultStat => setting.defaultStat;

    [field: SerializeField] public Wall targetWall { get; private set; }
    private Vector3Int _targetWallCellPos;
    private Wall _previousTargetWall;
    private Player _currentTargetPlayer;
    [field:SerializeField]public Player nearestPlayer { get; private set; }
    [field: SerializeField] public Target currentTarget { get; private set; }

    public static event Action<int> OnMonsterDeath;

    public void Initialize(Origin ori, int id, Stat stat)
    {
        origin = ori;
        this.id = this.id == -1 ? id : this.id;
        monsterStat = new MonsterStat(FindObjectOfType<CooldownSystem>(), stat, this);
        monsterStat.OnHpZero += HpZeroEventHandler;
    }

    protected virtual void Awake()
    {
        id = -1;
        _monsterMovement = GetComponent<MonsterMovement>();
        _activeStatusEffects = new List<StatusEffectBase>();
        _animator = GetComponent<Animator>();
        //_animator.SetBool(IsDeadBool, false);
        _playerLayerMask = LayerMask.GetMask("Player");
        _wallLayerMask = LayerMask.GetMask("Wall");
        _monsterLayerMask = LayerMask.GetMask("Enemy");
        nearestPlayer = UnitManager.Instance.GetNearestPlayer(transform.position, true);

        if (setting.attackMethods.Any(ts => ts.target == setting.priority && ts.method == TargetMethod.DontAttack))
        {
            throw new Exception($"Priority {setting.priority} conflicted with {setting.attackMethods}");
        }
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        if (monsterStat == null) return;
        monsterStat.UpdateStatCooldown();
        ApplyStatusEffects();

        nearestPlayer = UnitManager.Instance.GetNearestPlayer(transform.position, true);

        if (!targetWall) ReRequestWall(_targetWallCellPos);
        
        CheckCanAttack();
        
        //SetAnimation();
    }

    //--------------------------------------

    private const string AttackTrigger = "attack";
    private const string IsDeadBool = "isDead";
    private const string IsMovingBool = "isMoving";

    private void SetAnimation()
    {
        _animator.SetBool(IsMovingBool,_monsterMovement.velocity != Vector3.zero);
    }

    public void RequestNewTargetWall()
    {
        targetWall = TilemapManager.instance.GetWall(transform.position);
        //Debug.Log($"Monster {name} has requested a new targetWall {targetWall}");
    }

    public void ReRequestWall(Vector3Int cellPos)
    {
        targetWall = TilemapManager.instance.GetWall(cellPos);
        if(!targetWall)
        {
            RequestNewTargetWall();
        }
        //Debug.Log($"Monster {name} has re-requested targetWall [{targetWall}]");
    }

    private void CheckCanAttack()
    {
        if (monsterStat.isAttackReady)
        {
            var nearestObj = PickNearest(nearestPlayer, targetWall, TilemapManager.instance.statue);
            if (DistanceTo(nearestObj) < setting.attackRange)
            {
                SendAttackMessage(nearestObj);
            }
        }
    }

    private void SendAttackMessage(Component nearestObj)
    {
        monsterStat.StartCooldown();
        NetworkClient.Instance.StartMonsterAttackAnimation(id);
        Attack(nearestObj);
    }

    protected abstract void Attack(Component nearestObj);

    private float DistanceTo(Component obj) => Vector3.Distance(obj.transform.position, transform.position);

    private Component PickNearest(params Component[] components)
    {
        float temp = float.MaxValue;
        Component res = null;
        foreach (var component in components)
        {
            if (DistanceTo(component) < temp)
            {
                temp = DistanceTo(component);
                res = component;
            }
        }

        if (res != null) return res;
        else return components[0];
    }

    private void HpZeroEventHandler()
    {
        Debug.Log($"Monster {id} of type {setting.type} and from {origin} has been killed");
        _animator.SetBool(IsDeadBool, true);
        OnMonsterDeath?.Invoke(id);
        SpawnManager.instance.ClearIdIndex(id);
        UnitManager.Instance.DeleteMonsterFromList(id);
        Destroy(gameObject);
    }

    public void ModifyHitPoint(float amount) => monsterStat.hitPoint += Mathf.RoundToInt(amount);

    public void AddStatusEffect(StatusEffectBase statusEffect) => _activeStatusEffects.Add(statusEffect);

    private void ApplyStatusEffects()
    {
        Stat temp = rawStat;
        for (int i = _activeStatusEffects.Count - 1; i >= 0; i--)
        {
            _activeStatusEffects[i].UpdateStat(temp);
            temp = _activeStatusEffects[i].newStat;
            if (_activeStatusEffects[i].remainingTime <= 0) _activeStatusEffects.RemoveAt(i);
        }

        _currentStat = temp;
    }

    protected virtual List<Player> GetTargetPlayers()
    {
        return UnitManager.Instance.GetObjectsInRadius<Player>(transform.position, setting.attackRange, _playerLayerMask);
    }

    public void PlayAttackAnimation()
    {
        //_animator.SetTrigger(AttackTrigger);
    }

    private void OnEnable()
    {
        TilemapManager.BroadcastWallFallen += WallFallenEventHandler;
        TilemapManager.BroadcastWallRebuilt += SetTargetWallToPrevious;
    }

    private void SetTargetWallToPrevious(Origin obj)
    {
        if (origin != obj) return;
        targetWall = _previousTargetWall;
    }

    private void OnDisable()
    {
        TilemapManager.BroadcastWallFallen -= WallFallenEventHandler;
        TilemapManager.BroadcastWallRebuilt -= SetTargetWallToPrevious;
    }

    private void WallFallenEventHandler(Wall obj)
    {
        
    }

    private void OnDestroy()
    {
        monsterStat.OnHpZero -= HpZeroEventHandler;
    }

    public void SetTargetWall(Wall wall)
    {
        if (origin != wall.origin) return;
        if (targetWall)
        {
            _previousTargetWall = targetWall;
        }
        targetWall = wall;
        _targetWallCellPos = wall.cellPos;
        //_monsterMovement.SetTarget(wall.transform);
        currentTarget = Target.Wall;
    }

    [ContextMenu(nameof(DamageMonsterBy10))]
    private void DamageMonsterBy10()
    {
        NetworkClient.Instance.ModifyMonsterHp(id, -10);
    }
}
