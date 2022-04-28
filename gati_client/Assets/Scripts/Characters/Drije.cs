using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drije : MonoBehaviour
{
    
    private Movement mvt;
    private Player pl;

    void Start()
    {
        pl = GetComponent<Player>();
        if (pl.IsLocal)
        {
            mvt = gameObject.AddComponent<Movement>();
            mvt.Maxspeed = 15;
            mvt.Acceleration = 6;
            mvt.Agility = 3;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
