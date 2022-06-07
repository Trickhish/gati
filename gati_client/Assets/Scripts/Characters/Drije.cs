using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drije : MonoBehaviour
{
    
    private Movement mvt;
    private Player pl;
    [SerializeField] public static Sprite illu;
    [SerializeField] public static GameObject prefab;

    void Start()
    {
        pl = GetComponent<Player>();
        pl.caraprefab = GameLogic.Singleton.drije_prefab;
        if (pl.IsLocal)
        {
            mvt = gameObject.AddComponent<Movement>();
            mvt.enabled = false;

            mvt.Maxspeed = 15;
            mvt.DefaultMaxspeed = 15;

            mvt.Acceleration = 6;
            mvt.DefaultAcceleration = 5;

            mvt.Agility = 3;
            mvt.DefaultAgility = 3;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
