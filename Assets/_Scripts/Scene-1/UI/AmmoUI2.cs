using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AmmoUI2 : MonoBehaviour
{
    Text text;
    [SerializeField] public WeaponRange obj;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        StartCoroutine(OnFindLocalPlayer());
    }

    // Update is called once per frame
    void Update()
    {
        if (obj != null)
            return;
        
        //text.text = obj.Ammo.ToString();
        Debug.Log("masuk" + obj.Ammo);
    }


    public Player LocalPlayer()
    {
        var players = FindObjectsOfType<Player>();

        foreach (Player p in players)
        {
            Debug.Log(p.gameObject.name);
            if (p.isLocal)
                return p;
        }
        return null;
    }
    IEnumerator OnFindLocalPlayer()
    {
        while (LocalPlayer() == null)
        {
            Debug.Log("Local player null");
            yield return new WaitForSeconds(0.2f);
        }
        obj = LocalPlayer().gameObject.GetComponent<WeaponRange>(); 
    }
    }
