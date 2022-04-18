using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statue : MonoBehaviour
{
    [field: SerializeField] public int Hp { get; private set; }
    private int _maxHp;

    public static event Action StatueDestroyed;
    // Start is called before the first frame update
    void Start()
    {
        _maxHp = GameManager.Instance.gameSetting.initialStatueHp;
        Hp = _maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ModifyHp(int amount)
    {
        Hp += amount;
        if (Hp <= 0)
        {
            StatueDestroyed?.Invoke();
        }

        if (Hp > _maxHp)
        {
            Hp = _maxHp;
        }

        ChangeTexture();
    }

    private void ChangeTexture()
    {
        if (Hp == _maxHp)
        {

        }
        else if(Hp > 75/100*_maxHp)
        {
            
        }
        else if (Hp > 50 / 100 * _maxHp)
        {
            
        }
        else if (Hp > 25 / 100 * _maxHp)
        {
            
        }
        else if (Hp > 0)
        {
            
        }
        else
        {
            
        }
    }

    public void PlayDestroyedAnimation()
    {

    }

    // Dipanggil ketika collision
    public void ShowUpgrades(WeaponBase weapon)
    {
        // Upgrade statue, or

        // Upgrade weapon
    }
}
