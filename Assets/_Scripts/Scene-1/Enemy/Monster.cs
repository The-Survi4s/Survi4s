using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MonsterMovement))]
public class Monster : MonoBehaviour
{
    [SerializeField] protected Stat defaultStat; // Di edit di inspector
    [SerializeField] protected float attackRange;
    [SerializeField] protected LayerMask playerLayerMask;
    [SerializeField] protected LayerMask wallLayerMask;
    [SerializeField] protected MonsterStat monsterStat;
    public Stat rawStat => monsterStat.getStat;
    private MonsterMovement _monsterMovement;
    private List<StatusEffectBase> _activeStatusEffects;

    public enum Origin { Top, Right, Bottom, Left }
    public enum Type {Kroco, Paskibra, Pramuka, Basket, Satpam, Musisi, TukangSapu}
    public Origin origin { get; private set; }
    [field: SerializeField] public Type type { get; private set; }
    public int id { get; private set; }
    public Wall targetWall { get; private set; }

    public static event Action<int> OnMonsterDeath; 

    public void Init(Origin ori, int Id)
    {
        origin = ori;
        this.id = Id == -1 ? Id : this.id;
    }

    private void Start()
    {
        id = -1;
        monsterStat = new MonsterStat(FindObjectOfType<CooldownSystem>(), defaultStat);
        _monsterMovement = GetComponent<MonsterMovement>();
        _activeStatusEffects = new List<StatusEffectBase>();

        monsterStat.OnHpZero += HpZeroEventHandler;
    }

    private void Update()
    {
        monsterStat.UpdateStat();
        if(monsterStat.isAttackReady && IsPlayerWithinRange()) SendAttackMessage();
    }

    private bool IsPlayerWithinRange()
    {
        return UnitManager.Instance.RangeFromNearestPlayer(transform.position) < attackRange;
    }

    private void HpZeroEventHandler()
    {
        OnMonsterDeath?.Invoke(id);
        SpawnManager.instance.ClearIdIndex(id);
        UnitManager.Instance.DeleteMonsterFromList(id);
        Destroy(gameObject);
    }

    public void ModifyHitPoint(float amount)
    {
        monsterStat.hitPoint += Mathf.RoundToInt(amount);
    }

    public void AddStatusEffect(StatusEffectBase statusEffect)
    {
        _activeStatusEffects.Add(statusEffect);
    }

    public Stat getUpdatedStat
    {
        get
        {
            Stat temp = rawStat;
            foreach (var statusEffect in _activeStatusEffects)
            {
                statusEffect.UpdateStat(temp);
            }

            return temp;
        }
    }

    public void SendAttackMessage()
    {
        monsterStat.StartCooldown();
        NetworkClient.Instance.StartMonsterAttackAnimation(id);
        var players = GetTargetPlayers();
        foreach (var player in players)
        {
            NetworkClient.Instance.ModifyPlayerHp(player.gameObject.name, -monsterStat.attack);
        }
    }

    protected virtual List<PlayerController> GetTargetPlayers()
    {
        return GetPlayersInRadius(attackRange);
    }

    protected List<PlayerController> GetPlayersInRadius(float r)
    {
        List<PlayerController> temp = new List<PlayerController>();
        var colliders = UnitManager.Instance.GetHitObjectInRange(new Vector2(transform.position.x,transform.position.y), r, playerLayerMask);
        foreach (var col in colliders)
        {
            Debug.Log(col.gameObject.name);
            if (col.TryGetComponent(out PlayerController player))
            {
                temp.Add(player);
            }
        }

        return temp;
    }

    protected List<PlayerController> GetNNearestPlayersHit(int count)
    {
        return UnitManager.Instance.GetNearestPlayers(transform.position, count);
    }

    public void PlayAttackAnimation()
    {

    }

    private void OnEnable()
    {
        switch (origin)
        {
            case Origin.Top:
                WallManager.OnWallFallenTop += SetTargetWall;
                break;
            case Origin.Right:
                WallManager.OnWallFallenRight += SetTargetWall;
                break;
            case Origin.Bottom:
                WallManager.OnWallFallenBottom += SetTargetWall;
                break;
            case Origin.Left:
                WallManager.OnWallFallenLeft += SetTargetWall;
                break;
        }
    }
    private void OnDisable()
    {
        switch (origin)
        {
            case Origin.Top:
                WallManager.OnWallFallenTop -= SetTargetWall;
                break;
            case Origin.Right:
                WallManager.OnWallFallenRight -= SetTargetWall;
                break;
            case Origin.Bottom:
                WallManager.OnWallFallenBottom -= SetTargetWall;
                break;
            case Origin.Left:
                WallManager.OnWallFallenLeft -= SetTargetWall;
                break;
        }
    }

    private void OnDestroy()
    {
        monsterStat.OnHpZero -= HpZeroEventHandler;
    }

    public void SetTargetWall(Wall wall)
    {
        targetWall = wall;
    }
}
