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

    public static Dictionary<string, Tuple<string, float, int>> effects = new Dictionary<string, Tuple<string, float, int>>() {
        {"ulti_drije", new Tuple<string, float, int>("stun", 5f, 4)}, //id  name, distance, duration
        {"ulti_gati", new Tuple<string, float, int>("stun", 5f, 4)},
        {"bomb", new Tuple<string, float, int>("stun", 5f, 4)},
        {"adrenaline", new Tuple<string, float, int>("stun", 5f, 4)},
    };

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
