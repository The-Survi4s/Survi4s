using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBase : MonoBehaviour
{
    private Vector2 whereToGoNext;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector2 Move()
    {
        return whereToGoNext; // hasil harus gerak ke mana di akses dari sini
    }
}
