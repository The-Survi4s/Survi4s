using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(MonsterMovement))]
public class Monster : MonoBehaviour
{
    [field: SerializeField] public int id { get; private set; }

    [SerializeField] protected LayerMask playerLayerMask;
    [SerializeField] protected LayerMask wallLayerMask;
    [SerializeField] protected LayerMask monsterLayerMask;
    protected MonsterStat monsterStat;
    
    private Stat rawStat => monsterStat?.getRawStat ?? new Stat();
    public Stat currentStat => _currentStat;
    [SerializeField] private Stat _currentStat;
    private MonsterMovement _monsterMovement;
    private Animator _animator;
    private List<StatusEffectBase> _activeStatusEffects;

    public enum Origin { Right, Top, Left, Bottom }
    public enum Type {Kroco, Paskibra, Pramuka, Basket, Satpam, Musisi, TukangSapu}
    public enum Target {Wall, Player}
    public enum TargetMethod {DontAttack, Nearest, Furthest, LowestHp}
    public Origin origin { get; private set; }
    [field: SerializeField] public Type type { get; private set; }
    [Serializable]
    public struct Setting
    {
        public float attackRange;
        public float detectionRange;
        public float minRange;
        public Target priority;
        public TargetMethod attackPlayer;
        public TargetMethod attackWall;
        public Stat defaultStat;

        public Setting(Stat defaultStat, float attackRange, float detectionRange, float minRange, Target priority, TargetMethod attackPlayer, TargetMethod attackWall)
        {
            this.defaultStat = defaultStat;
            this.attackRange = attackRange;
            this.detectionRange = detectionRange;
            this.priority = priority;
            this.attackPlayer = attackPlayer;
            this.attackWall = attackWall;
            this.minRange = minRange;
        }

        public static Setting defaultSetting = new Setting(new Stat(), 1, 5, 0, Target.Wall, TargetMethod.Nearest, TargetMethod.Nearest);
    }
    [field: SerializeField] public Setting setting { get; private set; }
    public Stat defaultStat => setting.defaultStat;

    [SerializeField] private Wall _targetWall;
    private Wall _previousTargetWall;
    private PlayerController _currentTargetPlayer;
    private PlayerController _nearestPlayer;
    [SerializeField] private float MaxStationaryTime = 5;

    public static event Action<int> OnMonsterDeath;

    public void Initialize(Origin ori, int id, Stat stat)
    {
        origin = ori;
        this.id = this.id == -1 ? id : this.id;
        monsterStat = new MonsterStat(FindObjectOfType<CooldownSystem>(), stat, this);
        monsterStat.OnHpZero += HpZeroEventHandler;
    }

    private void Awake()
    {
        id = -1;
        _monsterMovement = GetComponent<MonsterMovement>();
        _activeStatusEffects = new List<StatusEffectBase>();
        _animator = GetComponent<Animator>();
        _animator.SetBool(IsDeadBool, false);
    }

    private void Update()
    {
        if (monsterStat == null) return;
        monsterStat.UpdateStatCooldown();
        ApplyStatusEffects();

        _nearestPlayer = UnitManager.Instance.GetNearestPlayer(transform.position);
        if (_monsterMovement.stationaryTime > MaxStationaryTime || !_targetWall) RequestNewTargetWall();
        SetTargetMovement();
        
        CheckCanAttack();
        
        SetAnimation();
    }

    //--------------------------------------

    private const string AttackTrigger = "attack";
    private const string IsDeadBool = "isDead";
    private const string IsMovingBool = "isMoving";

    private void SetAnimation()
    {
        _animator.SetBool(IsMovingBool,_monsterMovement.velocity != Vector3.zero);
    }

    private void RequestNewTargetWall()
    {
        _targetWall = TilemapManager.instance.GetNearestWallFrom(transform.position);
        _monsterMovement.SetTarget(_targetWall.transform);
    }

    //Ini harusnya pakai state machine
    private void SetTargetMovement()
    {
        // Jika wall sudah hancur, target statue
        if (_targetWall.isDestroyed)
        {
            SetTargetMovementIfTargetWallDestroyed();
        }
        else
        {
            SetTargetMovementIfTargetWallExists();
        }
    }

    private void SetTargetMovementIfTargetWallExists()
    {
        if (setting.priority == Target.Player)
        {
            if (!_nearestPlayer.isDead) _monsterMovement.SetTarget(_nearestPlayer.transform);
            if (DistanceTo(_targetWall) < setting.attackRange && 
                setting.attackWall != TargetMethod.DontAttack)
            {
                if (!_targetWall.isDestroyed) _monsterMovement.SetTarget(_targetWall.transform);
            }
        }
        else
        {
            if (!_targetWall.isDestroyed) _monsterMovement.SetTarget(_targetWall.transform);
            if (DistanceTo(_nearestPlayer) < setting.detectionRange && 
                setting.attackPlayer != TargetMethod.DontAttack)
            {
                if (!_nearestPlayer.isDead) _monsterMovement.SetTarget(_nearestPlayer.transform);
            }
        }
    }

    private void SetTargetMovementIfTargetWallDestroyed()
    {
        if (setting.priority == Target.Player)
        {
            if (DistanceTo(_nearestPlayer) > setting.attackRange && !_nearestPlayer.isDead)
            {
                _monsterMovement.SetTarget(TilemapManager.instance.statue.transform);
            }
            else if (setting.attackPlayer != TargetMethod.DontAttack)
            {
                _monsterMovement.SetTarget(_nearestPlayer.transform);
            }
        }
        else
        {
            _monsterMovement.SetTarget(TilemapManager.instance.statue.transform);
        }
    }

    private void CheckCanAttack()
    {
        if (monsterStat.isAttackReady)
        {
            var nearestObj = PickNearest(_nearestPlayer, _targetWall, TilemapManager.instance.statue);
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
        if(!NetworkClient.Instance.isMaster) return;
        switch (nearestObj)
        {
            case Wall wall:
                NetworkClient.Instance.ModifyWallHp(wall.id, -_currentStat.atk);
                break;
            case PlayerController _:
                var players = GetTargetPlayers();
                foreach (var player in players)
                {
                    NetworkClient.Instance.ModifyPlayerHp(player.name, -_currentStat.atk);
                }
                break;
            case Statue _:
                NetworkClient.Instance.ModifyStatueHp(-_currentStat.atk);
                break;
        }
    }

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
        Debug.Log($"Monster {id} of type {type} and from {origin} has been killed");
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

    protected virtual List<PlayerController> GetTargetPlayers()
    {
        return UnitManager.Instance.GetObjectsInRadius<PlayerController>(transform.position, setting.attackRange, playerLayerMask);
    }

    public void PlayAttackAnimation()
    {
        _animator.SetTrigger(AttackTrigger);
    }

    private void OnEnable()
    {
        TilemapManager.BroadcastWallFallen += SetTargetWall;
        TilemapManager.BroadcastWallRebuilt += SetTargetWallToPrevious;
    }

    private void SetTargetWallToPrevious(Origin obj)
    {
        if (origin != obj) return;
        _targetWall = _previousTargetWall;
    }

    private void OnDisable()
    {
        TilemapManager.BroadcastWallFallen -= SetTargetWall;
        TilemapManager.BroadcastWallRebuilt -= SetTargetWallToPrevious;
    }

    private void OnDestroy()
    {
        monsterStat.OnHpZero -= HpZeroEventHandler;
    }

    public void SetTargetWall(Wall wall)
    {
        if (_targetWall)
        {
            _previousTargetWall = _targetWall;
        }
        if (origin != wall.origin) return;
        _targetWall = wall;
        _monsterMovement.SetTarget(wall.transform);
    }
}
