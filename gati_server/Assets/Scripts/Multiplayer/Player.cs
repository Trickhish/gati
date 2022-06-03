using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Web;
using System;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> plist = new Dictionary<ushort, Player>();
    public ushort Id { get; set; }
    public string Username { get; set; }
    public string Mail { get; set; }
    public Dictionary<string, int> items = new Dictionary<string, int>(){
        {"bomb", 0},
        {"adrenaline", 0},

    };
    public string matchid { get; set; }
    public Vector3 position { get; set; }
    public string cara;
    public List<Effect> effects {get; set;}

    private void OnDestroy()
    {
        plist.Remove(Id);
    }

    public Player(ushort id, string mail, string username)
    {
        Id = id;
        Username = username;
        Mail = mail;
        matchid = "";
        cara = "";
        position = new Vector3(-192f, 0f, 0f);
        this.effects = new List<Effect>();
    }

    public Player(ushort id, string mid)
    {
        Id = id;
        matchid = mid;
        Username = "";
        cara = "";
        Mail = "";
        position = new Vector3(-192f, 0f, 0f);
        items = new Dictionary<string, int>();
        this.effects = new List<Effect>();
    }

    public Player(ushort id)
    {
        Id = id;
        matchid = "";
        Username = "";
        Mail = "";
        cara = "";
        position = new Vector3(-192f, 0f, 0f);
        items = new Dictionary<string, int>();
        this.effects = new List<Effect>();
    }

    public bool CanMove()
    {
        foreach(Effect ef in this.effects)
        {
            if (ef.Name == "stun")
            {
                return(false);
            }
        }
        return(true);
    }

    public void rlitems()
    {
        string r = NetworkManager.getreq("https://trickhisch.alwaysdata.net/gati/?a=items&m="+ HttpUtility.UrlEncode(this.Mail) + "&sat="+NetworkManager.sat);

        if (r!="unreachable" && r!="error" && r!="false")
        {
            foreach (string o in r.Split(","))
            {
                string k = o.Split(":")[0];
                int v = int.Parse(o.Split(":")[1]);

                this.items[k] = v;
            }
        }
    }

    public int SendEffect(string eid)
    {
        Tuple<string, float, int> eat = GameLogic.effects[eid];
        string ename = eat.Item1;
        float edist = eat.Item2;
        int edur = eat.Item3;

        int tch = 0;
        foreach (Player p in Match.mlist[this.matchid].players.Values) // envoyer l'effet a tout les joueurs dans le rayon
        {
            if (Vector3.Distance(this.position, p.position) < edist && p.Id != this.Id)
            {
                tch++;

                p.effects.Add(new Effect(ename, edur));

                Message msg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.effect);
                msg.AddString(ename);
                msg.AddInt(edur);
                NetworkManager.Singleton.Server.Send(msg, p.Id);
            }
        }
        return(tch);
    }

    public void EffectCallback(string item, bool succ, int rem=0, int tch=0)
    {
        Message msg = Message.Create(MessageSendMode.reliable, ServerToClient.itemused);

        msg.AddString(item);
        msg.AddBool(succ);
        msg.AddInt(rem);
        msg.AddInt(tch);

        NetworkManager.Singleton.Server.Send(msg, this.Id);
    }

    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message)
    {
        Debug.Log("player " + fromClientId + " added");
        string nck = message.GetString();

        //Player.plist.Add(fromClientId, new Player(fromClientId, nck));

        //Spawn(fromClientId, message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.useitem)]
    private static void ItemUsed(ushort cid, Message msg) // an item was used by a player
    {
        string item = msg.GetString();
        Vector2 pos = msg.GetVector2();

        Player p = Player.plist[cid];

        p.position = pos;

        if (Player.plist[cid].items.ContainsKey(item) && Player.plist[cid].items[item] > 0)
        {
            p.items[item]--;
            int tch=0;
            switch (item)
            {
                case "bomb":
                    tch = p.SendEffect("bomb");
                    break;
                case "adrenaline":
                    p.effects.Add(new Effect("adrenaline", 5f));
                    break;
                default:
                    break;
            }

            p.EffectCallback(item, true, p.items[item], tch);
        } else
        {
            p.EffectCallback(item, false);
        }
    }
}