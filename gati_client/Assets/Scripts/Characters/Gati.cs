using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gati : MonoBehaviour
{
    
    private Movement mvt;
    private Player pl;

    void Start()
    {
        pl = GetComponent<Player>();
        pl.caraprefab = GameLogic.Singleton.gati_prefab;
        if (pl.IsLocal)
        {
            mvt = gameObject.AddComponent<Movement>();
            mvt.Maxspeed = 16;
            mvt.Acceleration = 6;
            mvt.Agility = 6;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
