using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Enum of targets of which a something can attack
/// </summary>
public enum Target { Statue, Wall, Player, Monster }

/// <summary>
/// Base class of all Monsters
/// </summary>
[RequireComponent(typeof(MonsterMovement),typeof(Animator))]
public abstract class Monster : MonoBehaviour
{
    #region LayerMasks
    private LayerMask _playerLayerMask;
    private LayerMask _wallLayerMask;
    private LayerMask _monsterLayerMask;
    #endregion

    #region Components
    protected MonsterMovement _monsterMovement;
    protected AudioManager audioManager;
    private Animator _animator;
    protected SpriteRenderer _renderer;
    #endregion

    #region Data and Containers Definition
    public enum Type { Kroco, Paskibra, Pramuka, Basket, Satpam, Musisi, TukangSapu, Futsal }

    /// <summary>
    /// Method enums on how to attack a <see cref="Target"/>
    /// </summary>
    public enum TargetMethod { DontAttack, Nearest, Furthest, LowestHp }

    /// <summary>
    /// A struct to store <see cref="Target"/> and <see cref="TargetMethod"/> in pairs. 
    /// Used by <see cref="Monster"/> to decide how to attack a <see cref="Target"/>
    /// </summary>
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

    /// <summary>
    /// A struct to store <see cref="Monster"/>'s settings
    /// </summary>
    [Serializable]
    public struct Setting
    {
        public Type type;
        /// <summary>
        /// Distance before this <see cref="Monster"/> gets distracted and attacks <see cref="Player"/> instead. 
        /// <br/>In the future this will be used to attack second priority target instead of always <see cref="Player"/>
        /// </summary>
        public float detectionRange;
        /// <summary>
        /// Distance before this <see cref="Monster"/> can start attacking
        /// </summary>
        public float attackRange;
        /// <summary>
        /// Minimum range before the monster stopped chasing it's target. Or backing off if the target is too close
        /// </summary>
        public float minRange;
        /// <summary>
        /// Which <see cref="Target"/> should this <see cref="Monster"/> prioritize. 
        /// This will be the default <see cref="Target"/> before any changes
        /// </summary>
        public Target priority;
        /// <summary>
        /// List of <see cref="Target"/>s and <see cref="TargetMethod"/>s in pairs. 
        /// Used by <see cref="Monster"/> to decide how to attack a <see cref="Target"/>
        /// </summary>
        public List<TargetSettings> attackMethods;
        public Stat defaultStat;
        /// <summary>
        /// Enables <see cref="Monster"/> to move away when it gets too close (less than <see cref="minRange"/>)
        /// </summary>
        public bool doEvasion;
        /// <summary>
        /// Gets the <see cref="TargetMethod"/> of a <see cref="Target"/>
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public TargetMethod MethodOf(Target target) =>
            attackMethods.Where(ts => ts.target == target).Select(ts => ts.method).FirstOrDefault();
    }

    #endregion

    #region Stat Variables

    public int id { get; private set; }

    protected MonsterStat monsterStat;

    private bool isInitialized;

    /// <summary>
    /// <see cref="Monster"/> stat after processed once. Before applying any <see cref="StatusEffectBase"/>
    /// </summary>
    private Stat rawStat => monsterStat?.getRawStat ?? new Stat();

    /// <summary>
    /// <see cref="Monster"/>'s <see cref="Stat"/> after all <see cref="StatusEffectBase"/>s are applied. Property version
    /// </summary>
    public Stat currentStat => _currentStat;

    /// <summary>
    /// <see cref="Monster"/>'s <see cref="Stat"/> after all <see cref="StatusEffectBase"/>s are applied
    /// </summary>
    [SerializeField] private Stat _currentStat;

    public bool isDead => _currentStat.hp <= 0;

    /// <summary>
    /// List of all currently active status effects
    /// </summary>
    private List<StatusEffectBase> _activeStatusEffects;

    /// <summary>
    /// The direction of which this <see cref="Monster"/> comes from
    /// </summary>
    public Origin origin { get; private set; }

    /// <summary>
    /// Set in inspector. Important <see cref="Setting"/>s required for <see cref="Monster"/> to work properly
    /// </summary>
    [field: SerializeField] public Setting setting { get; protected set; }
    public Stat defaultStat => setting.defaultStat;

    /// <summary>
    /// Miliseconds delay before Monster really attacks
    /// </summary>
    [SerializeField] private int _attackDelay;
    #endregion

    #region Target Variables

    /// <summary>
    /// The current <see cref="Wall"/> as a target candidate to chase or attack. 
    /// <br/>Used when <see cref="MonsterMovement._currentTarget"/> is set to <see cref="Target.Wall"/>. 
    /// </summary>
    /// <remarks>
    /// <br/> If <see langword="null"/>, calls <see cref="ReRequestWall(Vector3Int)"/> to refills it
    /// </remarks>
    [field: SerializeField] public Wall targetWall { get; private set; }

    /// <summary>
    /// The cell position of the last (or current) <see cref="targetWall"/>
    /// </summary>
    private Vector3Int _targetWallCellPos;

    private Wall _previousTargetWall;

    private Player _currentTargetPlayer;

    /// <summary>
    /// Current nearest <see cref="Player"/> as a candidate to chase or attack. 
    /// <br/>Used when <see cref="MonsterMovement._currentTarget"/> is <see cref="Target.Player"/>
    /// </summary>
    [field: SerializeField] public Player nearestPlayer { get; private set; }

    /// <summary>
    /// For visuals only. Checks what is this <see cref="Monster"/> currently targetting
    /// </summary>
    [field: SerializeField] public Target currentTarget { get; private set; }

    #endregion

    #region Initial Setup

    /// <summary>
    /// Initializes Monster. Will only work once
    /// </summary>
    /// <param name="origin">The <see cref="Origin"/> of <see cref="Spawner"/> 
    /// in which this <see cref="Monster"/> was spawned from</param>
    /// <param name="id">The <see cref="Monster"/>'s id assigned by <see cref="SpawnManager"/></param>
    /// <param name="stat">The initial stat to set after processed in <see cref="WaveInfo.CalculateStat(Stat)"/></param>
    public bool Initialize(Origin origin, int id, Stat stat)
    {
        if (isInitialized) return false;
        this.origin = origin;
        this.id = this.id == -1 ? id : this.id;
        monsterStat = new MonsterStat(FindObjectOfType<CooldownSystem>(), stat, this);
        monsterStat.OnHpZero += HpZeroEventHandler;
        isInitialized = true;
        return true;
    }

    protected virtual void Awake()
    {
        // Gets the attached Components
        _monsterMovement = GetComponent<MonsterMovement>();
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
        audioManager = GetComponent<AudioManager>();

        FirstSetup();
        CheckConflictingPriorities();
        nearestPlayer = UnitManager.Instance.GetNearestPlayer(transform.position, true);
    }

    private void FirstSetup()
    {
        _activeStatusEffects = new List<StatusEffectBase>();
        id = -1;
        _animator.SetBool(IsDeadBool, false);
        _playerLayerMask = LayerMask.GetMask("Player");
        _wallLayerMask = LayerMask.GetMask("Wall");
        _monsterLayerMask = LayerMask.GetMask("Enemy");
    }

    /// <summary>
    /// Throws an exception when the <see cref="Setting.priority"/> 
    /// is set to <see cref="TargetMethod.DontAttack"/> in <see cref="Setting.attackMethods"/>
    /// </summary>
    private void CheckConflictingPriorities()
    {
        if (setting.attackMethods.Any(ts => ts.target == setting.priority && ts.method == TargetMethod.DontAttack))
        {
            throw new Exception($"Priority {setting.priority} conflicted with {setting.attackMethods}");
        }
    }

    #endregion

    protected virtual void Update()
    {
        if (monsterStat == null) return;
        monsterStat.UpdateStatCooldown();
        ApplyStatusEffects();

        nearestPlayer = UnitManager.Instance.GetNearestPlayer(transform.position, true);

        if (!targetWall) ReRequestWall(_targetWallCellPos);
        
        if(NetworkClient.Instance.isMaster) CheckCanAttack();
        
        SetAnimation();
    }

    //--------------------------------------

    #region Set Target Wall
    /// <summary>
    /// Sets <see cref="targetWall"/>. Must have the same <see cref="Origin"/> as <see cref="Monster"/>
    /// </summary>
    /// <param name="wall"></param>
    public void SetTargetWall(Wall wall)
    {
        if (origin != wall.origin) return;
        if (targetWall)
        {
            _previousTargetWall = targetWall;
        }
        targetWall = wall;
        _targetWallCellPos = wall.cellPos;
        currentTarget = Target.Wall;
    }

    /// <summary>
    /// Requests a nearest target <see cref="Wall"/> as a candidate to attack or chase. 
    /// Stored in <see cref="targetWall"/>
    /// </summary>
    public void RequestNewTargetWall() => targetWall = TilemapManager.Instance.GetWall(transform.position);

    /// <summary>
    /// Re-requests a target <see cref="Wall"/> on the same <paramref name="cellPos"/>
    /// <br/>in case it gets destroyed because of <see cref="TilemapManager.UpdateWallTilemap(DestroyableTile)"/>
    /// </summary>
    /// <param name="cellPos">enter <see cref="_targetWallCellPos"/>'s here</param>
    protected void ReRequestWall(Vector3Int cellPos)
    {
        targetWall = TilemapManager.Instance.GetWall(cellPos);
        if (!targetWall)
        {
            RequestNewTargetWall();
        }
        //Debug.Log($"Monster {name} has re-requested targetWall [{targetWall}]");
    }
    #endregion

    #region Attack
    /// <summary>
    /// Checks for whenever this <see cref="Monster"/> can attack, if so, attack. (<see cref="NetworkClient.isMaster"/> only)
    /// </summary>
    /// <remarks>
    /// Whenever <see cref="MonsterStat.isAttackReady"/> and one of these 
    /// <br/>objects' distance to this <see cref="Monster"/> 
    /// are less than <see cref="Setting.attackRange"/> before attacking
    /// <br/>- <see cref="nearestPlayer"/>, 
    /// <br/>- <see cref="targetWall"/>, or 
    /// <br/>- <see cref="TilemapManager.statue"/>
    /// </remarks>
    private void CheckCanAttack()
    {
        if (monsterStat.isAttackReady && !isDead)
        {
            var nearestObj = PickNearest(nearestPlayer, targetWall, TilemapManager.Instance.statue);
            if (DistanceTo(nearestObj) < setting.attackRange)
            {
                SendAttackMessage(nearestObj);
            }
        }
    }

    /// <summary>
    /// Sends a message to server through <see cref="NetworkClient"/> 
    /// that this <see cref="Monster"/> attacks <paramref name="nearestObj"/>. 
    /// <br/>(<see cref="NetworkClient.isMaster"/> only)
    /// <br/><br/>The receiving end will only call <see cref="PlayAttackAnimation"/>
    /// </summary>
    /// <param name="nearestObj"></param>
    private async void SendAttackMessage(Component nearestObj)
    {
        if (!nearestObj) return;
        monsterStat.StartCooldown();
        NetworkClient.Instance.StartMonsterAttackAnimation(id);
        await System.Threading.Tasks.Task.Delay(_attackDelay);
        Attack(nearestObj);
    }

    /// <summary>
    /// Must be overridden. Specify how this <see cref="Monster"/> would attack. 
    /// <br/>(<see cref="NetworkClient.isMaster"/> only)
    /// </summary>
    /// <param name="nearestObj">The object to attack</param>
    protected abstract void Attack(Component nearestObj);
    #endregion

    #region Utilities
    /// <returns>
    /// Distance from this <see cref="Monster"/> to <paramref name="obj"/>. 
    /// <br/>Or <see cref="float.MaxValue"/> if obj is null
    /// </returns>
    private float DistanceTo(Component obj) 
    {
        if (obj) return Vector3.Distance(obj.transform.position, transform.position);
        else return float.MaxValue;
    }

    /// <summary>
    /// Returns the nearest <see cref="Component"/> from <paramref name="components"/>. 
    /// <br/>This uses the naive approach
    /// </summary>
    /// <param name="components"></param>
    /// <returns>The first <see cref="Component"/> by default. Else the nearest</returns>
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

    protected virtual List<Player> GetPlayersInRadius()
    {
        return UnitManager.Instance.GetObjectsInRadius<Player>(transform.position, setting.attackRange, _playerLayerMask);
    }
    private async void SetTemporaryColor(Color color, int milisecondDuration)
    {
        Color originalColor = _renderer.color;
        _renderer.color = color;
        await System.Threading.Tasks.Task.Delay(milisecondDuration);
        _renderer.color = originalColor;
    }

    #endregion

    #region Stat Modifications
    /// <summary>
    /// Adds <see cref="MonsterStat.hitPoint"/> by <paramref name="amount"/>, rounded. 
    /// </summary>
    /// <param name="amount"></param>
    private string _lastPlayerHit;
    public void ModifyHitPoint(float amount, string playerName)
    {
        monsterStat.hitPoint += Mathf.RoundToInt(amount);
        if (playerName != null && amount < 0) 
        {
            SetTemporaryColor(Color.red, 400);
            _lastPlayerHit = playerName; 
        }
    }

    /// <summary>
    /// Adds <paramref name="statusEffect"/> to <see cref="_activeStatusEffects"/>
    /// </summary>
    /// <param name="statusEffect"></param>
    public void AddStatusEffect(StatusEffectBase statusEffect) => _activeStatusEffects.Add(statusEffect);

    /// <summary>
    /// Applies <see cref="_activeStatusEffects"/> list to <see cref="rawStat"/>. 
    /// <br/>Results in <see cref="_currentStat"/>. 
    /// </summary>
    /// <remarks>Also removes expired <see cref="StatusEffectBase"/>s from list</remarks>
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
    #endregion

    #region Animation
    private const string AttackTrigger = "attack";
    private const string IsDeadBool = "isDead";
    private const string DeadTrigger = "dead";
    private const string IsMovingBool = "isMoving";

    /// <summary>
    /// All animation logic goes here
    /// </summary>
    private void SetAnimation()
    {
        // Moving
        _animator.SetBool(IsMovingBool, _monsterMovement.velocity != Vector3.zero);
        _renderer.flipX = _monsterMovement.velocity.x < 0;
    }

    public void PlayAttackAnimation()
    {
        _animator.SetTrigger(AttackTrigger);
    }
    #endregion

    #region Event Handlers

    [SerializeField] private GameObject _itemDrop;
    public static event Action<int> OnMonsterDeath;

    private void OnEnable()
    {
        TilemapManager.BroadcastWallFallen += WallFallenEventHandler;
        TilemapManager.BroadcastWallRebuilt += SetTargetWallToPrevious;
    }

    private void OnDisable()
    {
        TilemapManager.BroadcastWallFallen -= WallFallenEventHandler;
        TilemapManager.BroadcastWallRebuilt -= SetTargetWallToPrevious;
    }

    private void SetTargetWallToPrevious(Origin obj)
    {
        if (origin != obj) return;
        targetWall = _previousTargetWall;
    }

    /// <summary>
    /// Handles monster death
    /// </summary>
    private void HpZeroEventHandler()
    {
        var player = UnitManager.Instance.GetPlayer(_lastPlayerHit);
        if(player) player.AddKillCount();

        //Debug.Log($"Monster {id} of type {setting.type} and from {origin} has been killed");
        _animator.SetBool(IsDeadBool, true);
        OnMonsterDeath?.Invoke(id);
        SpawnManager.Instance.ClearIdIndex(id);
        UnitManager.Instance.DeleteMonsterFromList(id);

        // Drop Item
        Instantiate(_itemDrop, transform.position, transform.rotation);

        Destroy(gameObject, 5);
    }

    private void WallFallenEventHandler(Wall obj)
    {

    }

    private void OnDestroy()
    {
        monsterStat.OnHpZero -= HpZeroEventHandler;
    }

    #endregion

    [ContextMenu(nameof(DamageMonsterBy10))]
    private void DamageMonsterBy10()
    {
        NetworkClient.Instance.ModifyHp(this, -10);
    }
}
