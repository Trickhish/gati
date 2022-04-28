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
            mvt.Maxspeed = 12;
            mvt.Acceleration = 6;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
