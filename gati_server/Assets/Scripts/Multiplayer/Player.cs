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
    public Dictionary<string, int> items = new Dictionary<string, int>(){ // #additem
        {"bomb", 0},
        {"adrenaline", 0},
        {"feather", 0},
        {"shield", 0},
        {"web", 0},
        {"boots", 0},
        {"cape", 0},
        {"lightningbolt", 0},
        {"flash", 0},
    };
    public string matchid { get; set; }
    public Vector3 position { get; set; }
    public string cara;
    public List<Effect> effects {get; set;}
    public string status { get; set; }
    public bool canmove { get; set; }
    public bool visible { get; set; }
    public bool canuseobjects { get; set; }
    public int resistanceadd { get; set; }
    public bool invincible { get; set; }
    public float lastcapacityuse { get; set; }
    public float lastitemuse { get; set; }

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
        this.status = "none";
        this.canmove = true;
        this.visible = true;
        this.canuseobjects = true;
        this.resistanceadd = 0;
        this.invincible = false;
        this.lastcapacityuse = 0f;
        this.lastitemuse = 0f;
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
        this.status = "waiting";
        this.canmove = true;
        this.visible = true;
        this.canuseobjects = true;
        this.resistanceadd = 0;
        this.invincible = false;
        this.lastcapacityuse = 0f;
        this.lastitemuse = 0f;
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
        this.status = "none";
        this.canmove = true;
        this.visible = true;
        this.canuseobjects = true;
        this.resistanceadd = 0;
        this.invincible = false;
        this.lastcapacityuse = 0f;
        this.lastitemuse = 0f;
    }

    public void UpdateEffects()
    {
        int i = 0;
        while (i<this.effects.Count)
        {
            Effect ef = this.effects[i];
            if (!ef.Update())
            {
                i++;
            }
            if (i>=this.effects.Count)
            {
                break;
            }
        }
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
            if (!p.invincible && Vector3.Distance(this.position, p.position) < edist && p.Id != this.Id)
            {
                tch++;

                int pedur = Math.Max(50, GameLogic.AfterRes(edur, p.cara, p.resistanceadd));
                p.effects.Add(new Effect(p, ename, pedur));

                Message msg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.effect);
                msg.AddString(ename);
                msg.AddInt(edur);
                NetworkManager.Singleton.Server.Send(msg, p.Id);
            }
        }
        return(tch);
    }

    public void Affect(Effect ef)
    {
        Message msg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.effect);
        msg.AddString(ef.Name);
        msg.AddInt(Mathf.RoundToInt(ef.Duration));
        NetworkManager.Singleton.Server.Send(msg, this.Id);
    }

    public void Affect(string eid)
    {
        Tuple<string, float, int> eat = GameLogic.effects[eid];
        string ename = eat.Item1;
        float edist = eat.Item2;
        int edur = eat.Item3;

        int pedur = Math.Max(50, GameLogic.AfterRes(edur, this.cara, this.resistanceadd));

        Effect ef = new Effect(this, ename, pedur);

        this.effects.Add(ef);

        this.Affect(ef);
    }

    public void EffectCallback(string item, bool succ, int rem=0, int tch=0)
    {
        Message msg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.itemused);

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
    private static void ItemUsed(ushort cid, Message msg) // an item was used by the player
    {
        string item = msg.GetString();
        Vector2 pos = msg.GetVector2();

        Player p = Player.plist[cid];

        p.position = pos;

        if (p.items.ContainsKey(item) && (Time.realtimeSinceStartup*1000f)-p.lastitemuse>3000f && p.items[item] > 0 && p.canuseobjects && NetworkManager.useitem(cid, item))
        {
            p.items[item]--;
            int tch=-1;
            string onw = "on himself";
            switch (item) // #additem
            {
                case "bomb": // done
                    tch = p.SendEffect("bomb");
                    onw = "on "+tch.ToString()+" players";
                    break;
                case "adrenaline": // done
                    p.effects.Add(new Effect(p, "resistance", 2000f));
                    tch = -1;
                    break;
                case "feather": // done
                    p.effects.Add(new Effect(p, "agility", 2000f));
                    tch = -1;
                    break;
                case "shield": // done
                    p.effects.Add(new Effect(p, "invincibility", 2000f));
                    tch = -1;
                    break;
                case "web": // done
                    onw = "(" + pos.x.ToString() +", "+pos.y.ToString()+")";
                    Match.mlist[p.matchid].EffectBlocks.Add(new EffectBlock(Match.mlist[p.matchid], "web", pos.x, pos.y, -1f, 2f, p, "web_slowness", -1f));
                    break;
                case "boots": // done
                    p.effects.Add(new Effect(p, "speed", 2000f));
                    tch = -1;
                    break;
                case "cape": // done
                    p.effects.Add(new Effect(p, "invisibility", 2000f));
                    tch = -1;
                    break;
                case "flash": // done
                    tch = p.SendEffect("flash");
                    onw = tch.ToString() + " players";
                    break;
                case "lightningbolt": // done
                    tch = p.SendEffect("lightningbolt");
                    onw = tch.ToString() + " players";
                    break;
                default:
                    break;
            }

            p.lastitemuse = Time.realtimeSinceStartup * 1000f;

            NetworkManager.log(p.Username + " used " +item+" "+onw, "PA");
            p.EffectCallback(item, true, p.items[item], tch);
        } else
        {
            p.EffectCallback(item, false);
            NetworkManager.log(p.Username + " couldn't use "+item, "PA");
        }
    }
}