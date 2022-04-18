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
    [SerializeField] protected MonsterStat monsterStat;
    
    public Stat rawStat
    {
        get
        {
            if (monsterStat == null) return new Stat(); 
            return monsterStat.getRawStat;
        }
    }

    public Stat currentStat;
    private MonsterMovement _monsterMovement;
    private List<StatusEffectBase> _activeStatusEffects;

    public enum Origin { Top, Right, Bottom, Left }
    public enum Type {Kroco, Paskibra, Pramuka, Basket, Satpam, Musisi, TukangSapu}
    public enum Target {Wall, Player}
    public enum TargetMethod {DontAttack, Nearest, Furthest, LowestHp}
    public Origin origin { get; private set; }
    [field: SerializeField] public Type type { get; private set; }
    [Serializable]
    protected struct MonsterSetting
    {
        public float attackRange;
        public float collisionRange;
        public float detectionRange;
        public float minRange;
        public Target priority;
        public TargetMethod attackPlayer;
        public TargetMethod attackWall;
        public Stat defaultStat;

        public MonsterSetting(Stat defaultStat, float attackRange, float collisionRange, float detectionRange, float minRange, Target priority, TargetMethod attackPlayer, TargetMethod attackWall)
        {
            this.defaultStat = defaultStat;
            this.attackRange = attackRange;
            this.detectionRange = detectionRange;
            this.priority = priority;
            this.attackPlayer = attackPlayer;
            this.attackWall = attackWall;
            this.collisionRange = collisionRange;
            this.minRange = minRange;
        }

        public static MonsterSetting defaultMonsterSetting = new MonsterSetting(new Stat(), 1, 2, 5, 0, Target.Wall, TargetMethod.Nearest, TargetMethod.Nearest);
    }
    [SerializeField] protected MonsterSetting setting;
    public Stat defaultStat => setting.defaultStat;

    private Wall _targetWall;
    private Wall _previousTargetWall;
    private PlayerController _currentTargetPlayer;
    private PlayerController _nearestPlayer;

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
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (monsterStat == null) return;
        monsterStat.UpdateStatCooldown();
        ApplyStatusEffects();
        _nearestPlayer = UnitManager.Instance.GetNearestPlayer(transform.position);
        SetTargetMovement();
        CheckCanAttack();
    }

    private void SetTargetMovement()
    {
        if (Mathf.Min(DistanceTo(_targetWall), DistanceTo(GameManager.Instance.statue), DistanceTo(_nearestPlayer)) <
            setting.minRange) _monsterMovement.UpdateTarget(transform);
        if (_targetWall.isDestroyed && DistanceTo(_targetWall) < setting.collisionRange ||
            DistanceTo(GameManager.Instance.statue) < DistanceTo(_targetWall))
        {
            _monsterMovement.UpdateTarget(GameManager.Instance.statue.transform);
            return;
        }
        if (setting.priority == Target.Player)
        {
            _monsterMovement.UpdateTarget(_nearestPlayer.transform);
            if (DistanceTo(_targetWall) < setting.attackRange && setting.attackWall != TargetMethod.DontAttack)
            {
                _monsterMovement.UpdateTarget(_targetWall.transform);
            }
        }
        else
        {
            _monsterMovement.UpdateTarget(_targetWall.transform);
            if (DistanceTo(_nearestPlayer) < setting.detectionRange && setting.attackPlayer != TargetMethod.DontAttack)
            {
                _monsterMovement.UpdateTarget(_nearestPlayer.transform);
            }
        }
    }

    private void CheckCanAttack()
    {
        if (monsterStat.isAttackReady)
        {
            var nearestObj = PickNearest(_nearestPlayer, _targetWall);
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
                NetworkClient.Instance.ModifyWallHp(wall.Id, -currentStat.atk);
                break;
            case PlayerController _:
                var players = GetTargetPlayers();
                foreach (var player in players)
                {
                    NetworkClient.Instance.ModifyPlayerHp(player.name, -currentStat.atk);
                }
                break;
            case Statue _:
                NetworkClient.Instance.ModifyStatueHp(-currentStat.atk);
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

        return res;
    }

    private void HpZeroEventHandler()
    {
        Debug.Log($"Monster {id} of type {type} and from {origin} has been killed");
        OnMonsterDeath?.Invoke(id);
        SpawnManager.Instance.ClearIdIndex(id);
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

        currentStat = temp;
    }

    protected virtual List<PlayerController> GetTargetPlayers()
    {
        return UnitManager.Instance.GetObjectsInRadius<PlayerController>(transform.position, setting.attackRange, playerLayerMask);
    }

    public void PlayAttackAnimation()
    {

    }

    private void OnEnable()
    {
        WallManager.BroadcastWallFallen += SetTargetWall;
        WallManager.BroadcastWallRebuilt += SetTargetWallToPrevious;
    }

    private void SetTargetWallToPrevious(Origin obj)
    {
        if (origin != obj) return;
        _targetWall = _previousTargetWall;
    }

    private void OnDisable()
    {
        WallManager.BroadcastWallFallen -= SetTargetWall;
        WallManager.BroadcastWallRebuilt -= SetTargetWallToPrevious;
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
        _monsterMovement.UpdateTarget(wall.transform);
    }
}
