using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using System;

public class GameLogic : MonoBehaviour
{
    private static GameLogic _singleton;
    public static GameLogic Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameLogic)}");
                Destroy(value);
            }
        }
    }
    public GameObject PlayerPrefab => playerPrefab;

    [Header("Prefabs")]
    [SerializeField] public GameObject playerPrefab;

    private void Awake()
    {
        Singleton = this;
    }

    public static Dictionary<string, Tuple<string, float, int>> effects = new Dictionary<string, Tuple<string, float, int>>() { // #itemproperties
        {"ulti_drije", new Tuple<string, float, int>("stun", 5f, 500)}, //id  name, distance, duration
        {"ulti_gati", new Tuple<string, float, int>("stun", 5f, 500)},
        {"bomb", new Tuple<string, float, int>("stun", 5f, 300)},
        {"flash", new Tuple<string, float, int>("flash", 5f, 80)},
        {"lightningbolt", new Tuple<string, float, int>("disability", 5f, 3000)},
        {"web", new Tuple<string, float, int>("web_slowness", 3f, 80)},
    };

    public static Dictionary<string, int> resistance = new Dictionary<string, int>() {
        {"gati", 2},
        {"drije", 4},
    };

    public static int AfterRes(int v, string cara, int resadd=0)
    {
        int res = resistance[cara]+resadd;

        return(v-(res * 100));
    }

    public static void ulti(ushort pid)
    {
        Player ap = Player.plist[pid];

        if ((Time.realtimeSinceStartup*1000)-ap.lastcapacityuse < 3000)
        {
            NetworkManager.log(ap.Username + " ulti is on colldown", "MD");
            //ap.lastcapacityuse+=100;         // if the player spam his capacity, the cooldown will be longer
            ap.EffectCallback("capacity", false, -1, -1);
            return;
        }

        ap.lastcapacityuse = Time.realtimeSinceStartup * 1000;

        NetworkManager.log(ap.Username + " ULTI", "MD");

        
        string cara = ap.cara;

        switch (cara)
        {
            case "gati":
                ap.SendEffect("ulti_gati");
                ap.EffectCallback("capacity", true, -1, -1);
                break;
            case "drije":
                ap.EffectCallback("capacity", true, ap.SendEffect("ulti_drije"), -1);
                break;
            default:
                break;
        }
    }

    [MessageHandler((ushort)ClientToServerId.effect)]
    private static void Name(ushort cid, Message msg)
    {
        ushort ec = msg.GetUShort();

        switch(ec)
        {
            case 0:
                ulti(cid);
                break;
            default:
                break;
        }
        
    }

}
