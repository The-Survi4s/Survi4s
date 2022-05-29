using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AmmoUI2 : MonoBehaviour
{
    [SerializeField] private Text currentAmmoText;
    [SerializeField] private Text maxAmmoText;
    private WeaponRange _weaponRange;
    private Player _localPlayer;

    // Update is called once per frame
    void Update()
    {
        if (!_weaponRange)
        {
            if (_localPlayer)
            {
                var weapon = _localPlayer.weaponManager.weapon;
                if (weapon is WeaponRange wr) _weaponRange = wr;
            }
            else _localPlayer = UnitManager.Instance.GetPlayer();
            return;
        }
        
        currentAmmoText.text = _weaponRange.Ammo.ToString();
        maxAmmoText.text = _weaponRange.MaxAmmo.ToString();
        Debug.Log("masuk" + _weaponRange.Ammo);
    }
}
