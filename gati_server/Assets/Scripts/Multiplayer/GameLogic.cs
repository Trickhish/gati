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
        NetworkManager.log(Player.plist[pid].Username + " ULTI", "MD");

        Player ap = Player.plist[pid];
        string cara = ap.cara;

        switch (cara)
        {
            case "gati":
                ap.SendEffect("ulti_gati");
                break;
            case "drije":
                ap.SendEffect("ulti_drije");
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
