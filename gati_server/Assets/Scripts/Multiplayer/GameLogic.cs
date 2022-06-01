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

    public static Dictionary<ushort, Tuple<string, float, int>> effects = new Dictionary<ushort, Tuple<string, float, int>>() {
        {1, new Tuple<string, float, int>("stun", 5f, 4)}, //id  name, distance, duration
        {0, new Tuple<string, float, int>("unknown", 5f, 4)},
    };

    public static void sendeffect(ushort pid, ushort eid)
    {
        Player ap = Player.plist[pid];

        Tuple<string, float, int> eat = effects[eid];
        string ename = eat.Item1;
        float edist = eat.Item2;
        int edur = eat.Item3;

        foreach (Player p in Match.mlist[ap.matchid].players.Values) // envoyer l'effet a tout les joueurs dans le rayon
        {
            if (Vector3.Distance(ap.transform.position, p.transform.position) < edist && p.Id!=pid)
            {
                p.effects.Add(new Effect(ename, edur));

                Message msg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.effect);
                msg.AddUShort(eid);
                msg.AddInt(edur);
                NetworkManager.Singleton.Server.Send(msg, p.Id);
            }
        }
    }

    public static void ulti(ushort pid)
    {
        NetworkManager.log(Player.plist[pid].Username + " ULTI", "MD");

        Player ap = Player.plist[pid];
        string cara = ap.cara;

        switch (cara)
        {
            case "gati":
                sendeffect(pid, 0); // 0 = ? ~~
                break;
            case "drije":
                sendeffect(pid, 1); // 1 = stunned ~~
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
