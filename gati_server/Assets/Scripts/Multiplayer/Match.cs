using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using System;

public class Match : MonoBehaviour
{

    private static Match _singleton;
    public static Match Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(Match)}");
                Destroy(value);
            }
        }
    }

    public static Dictionary<string, Match> mlist = new Dictionary<string, Match>();

    public static List<(string, Vector2, Vector2)> maps = new List<(string, Vector2, Vector2)>
    {
        // first : 2f, second : 201
        ("Etril Sewer", new Vector2(-192f, 0.77f), new Vector2(2f, -3.5f)),    // done
        ("Maya Temple", new Vector2(-192f, 0.77f), new Vector2(201f, -3.5f)),  // done
        ("Niya City", new Vector2(-192f, -4f), new Vector2(2f, -3.5f)),        // 
        ("Ayrith Forest", new Vector2(-192f, -4f), new Vector2(2f, -3.5f)),    //
        ("Bravo Camp", new Vector2(-192f, -4f), new Vector2(2f, -3.5f)),       //
        ("Pirate Beach", new Vector2(-192f, -4f), new Vector2(2f, -3.5f)),     // 
        ("Snowy Mountain", new Vector2(-192f, -4f), new Vector2(2f, -3.5f)),   //
        ("Prehistory", new Vector2(-192f, -4f), new Vector2(2f, -3.5f)),       //
        ("Camda", new Vector2(-192f, -4f), new Vector2(2f, -3.5f))             //
    };

    static System.Random rand = new System.Random();

    public bool isprivate { get; private set; }
    public int capacity { get; private set; }
    public string status { get; private set; }
    public int map { get; private set; }
    public float starttime { get; set; }
    public Dictionary<ushort, Player> players = new Dictionary<ushort, Player>();
    public int awn { get; set; }
    public List<EffectBlock> EffectBlocks { get; set; }

    public string id { get; private set; }

    public Match()
    {
        this.EffectBlocks = new List<EffectBlock>() {};
        this.awn = 0;
    }

    public static string getrandomid(int l)
    {
        string rst = "";

        for (int i = 0; i < l; i++)
        {
            int rnd = rand.Next(0, 36);
            if (rnd < 26)
            {
                rst += (char)(rnd + 65);
            }
            else
            {
                rst += (rnd - 26).ToString();
            }
        }

        return (rst);
    }

    private void Awake()
    {
        Singleton = this;
    }
    public static string creatematch(bool isprivate, int capacity)
    {
        string id = getrandomid(5);
        int i = 0;
        while (mlist.ContainsKey(id))
        {
            id = getrandomid(5 + (i / 50));
            i++;
        }

        Match match = new Match();
        match.id = id;
        match.capacity = capacity;
        match.isprivate = isprivate;
        match.players = new Dictionary<ushort, Player>();
        match.status = "filling";
        match.map = rand.Next(0, 1);
        match.awn = 0;

        mlist.Add(id, match);

        return (id);
    }

    public static string findmatch()
    {
        foreach (Match m in mlist.Values)
        {
            if (m.players.Count < m.capacity && !m.isprivate && m.status=="filling")
            {
                return (m.id);
            }
        }
        return (null);
    }

    public void launch()
    {
        if (this.capacity == this.players.Count)
        {
            this.status = "started";
            Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.launch);
            foreach (ushort pid in players.Keys)
            {
                Player.plist[pid].lastcapacityuse = (Time.realtimeSinceStartup * 1000) + 5000f;
                Player.plist[pid].lastitemuse = (Time.realtimeSinceStartup * 1000) + 5000f;
                NetworkManager.Singleton.Server.Send(message, pid);
            }
            this.starttime = Time.realtimeSinceStartup;
            NetworkManager.log("match " + this.id + " starting", "M");
        }
    }

    public static void sendmatch(ushort clientid, string mid)
    {
        Player.plist[clientid].status = "waiting";

        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.match);
        message.AddString(mid);
        message.AddInt(mlist[mid].players.Count);
        message.AddInt(mlist[mid].capacity);
        message.AddVector2(maps[mlist[mid].map].Item2);
        message.AddVector2(maps[mlist[mid].map].Item3);
        message.AddInt(mlist[mid].map);

        foreach (Player value in mlist[mid].players.Values)
        {
            message.AddUShort(value.Id);
            message.AddString(value.Username);
            message.AddString(value.cara);
        }
        NetworkManager.Singleton.Server.Send(message, clientid);
    }

    public void sendmatchstatus(ushort pid, string username, string cara, bool joined)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.matchstatus);
        message.AddUShort(pid);
        message.AddString(username);
        message.AddString(cara);
        message.AddBool(joined);

        foreach (Player value in players.Values)
        {
            NetworkManager.Singleton.Server.Send(message, value.Id);
        }
        if (joined && players.Count == capacity)
        {
            this.launch();
        }
    }

    public int ExpOfRank(int rank)
    {
        switch(rank)
        {
            case 1:
                return(5);
            case 2:
                return(4);
            case 3:
                return(3);
            default:
                return(1);
        }
    }

    [MessageHandler((ushort)ClientToServerId.createprivate)]
    private static void privatematch(ushort cid, Message message)
    {
        Player.plist[cid].rlitems();

        int pc = message.GetInt();
        int map = message.GetInt();
        string username = message.GetString();
        string cara = message.GetString();

        string id = getrandomid(5);
        int i = 0;
        while (mlist.ContainsKey(id))
        {
            id = getrandomid(5 + (i / 50));
            i++;
        }

        Match match = new Match();
        match.id = id;
        match.capacity = pc;
        match.isprivate = true;
        match.players = new Dictionary<ushort, Player>();
        match.status = "filling";

        if (map!=0 && map!=1)
        {
            map = 0;
        }
        match.map = map;

        mlist.Add(id, match);

        if (!Player.plist.ContainsKey(cid))
        {
            Player player = new Player(cid, id);
            player.Username = username;
            player.cara = cara;
            Player.plist.Add(cid, player);
        }
        else
        {
            Player.plist[cid].matchid = id;
            Player.plist[cid].position = maps[mlist[id].map].Item2;
            Player.plist[cid].cara = cara;
        }

        mlist[id].players.Add(cid, Player.plist[cid]);

        sendmatch(cid, id);

        if (mlist[id].players.Count == mlist[id].capacity)
        {
            mlist[id].launch();
            NetworkManager.log("test match, launching right away", "M");
        }
        else
        {
            NetworkManager.log("match " + id + " created", "M");
        }
    }

    [MessageHandler((ushort)ClientToServerId.joinprivate)]
    private static void joinprivatematch(ushort clientid, Message message)
    {
        string username = message.GetString();
        string mid = message.GetString().ToUpper();
        string cara = message.GetString();

        if (Match.mlist.ContainsKey(mid) && Match.mlist[mid].status=="filling")
        {
            NetworkManager.log(username + " (" + cara + ") joined match " + mid, "M");

            if (!Player.plist.ContainsKey(clientid))
            {
                Player ap = new Player(clientid, mid);
                ap.Username = username;
                ap.cara = cara;
                ap.status = "waiting";
                Player.plist.Add(clientid, ap);
            }
            else
            {
                Player.plist[clientid].matchid = mid;
                Player.plist[clientid].position = maps[mlist[mid].map].Item2;
                Player.plist[clientid].cara = cara;
                Player.plist[clientid].status = "waiting";
            }

            Player.plist[clientid].rlitems();

            mlist[mid].players.Add(clientid, Player.plist[clientid]);

            sendmatch(clientid, mid);

            mlist[mid].sendmatchstatus(clientid, username, cara, true);
        }
        else
        {
            NetworkManager.log(username + " tried to join inexisting match " + mid, "M");
        }
    }

    [MessageHandler((ushort)ClientToServerId.findmatch)]
    private static void getorcreatematch(ushort clientid, Message message)
    {
        Player.plist[clientid].rlitems();

        string username = message.GetString();
        string cara = message.GetString();

        string mid = findmatch();

        if (mid == null)
        {
            mid = creatematch(false, 5);
            if (Player.plist.ContainsKey(clientid) && Player.plist[clientid].Username != "")
            {
                NetworkManager.log("New match (" + mid + ") by " + Player.plist[clientid].Username + " (" + cara + ")", "M");
            }
            else
            {
                NetworkManager.log("New match (" + mid + ") by Player " + clientid + " (" + cara + ")", "M");
            }
        }
        else
        {
            if (Player.plist.ContainsKey(clientid) && Player.plist[clientid].Username != "")
            {
                NetworkManager.log("Match " + mid + " found for " + Player.plist[clientid].Username + " (" + cara + ")", "M");
            }
            else
            {
                NetworkManager.log("Match " + mid + " found for Player " + clientid + " (" + cara + ")", "M");
            }
        }

        if (!Player.plist.ContainsKey(clientid))
        {
            Player ap = new Player(clientid, mid);
            ap.Username = username;
            ap.cara = cara;
            ap.status = "waiting";
            Player.plist.Add(clientid, ap);
        }
        else
        {
            //Player.plist[clientid].Username = username;
            Player.plist[clientid].matchid = mid;
            Player.plist[clientid].position = maps[mlist[mid].map].Item2;
            Player.plist[clientid].cara = cara;
            Player.plist[clientid].status = "waiting";
        }

        mlist[mid].players.Add(clientid, Player.plist[clientid]);

        sendmatch(clientid, mid);

        mlist[mid].sendmatchstatus(clientid, username, cara, true);
    }

    [MessageHandler((ushort)ClientToServerId.leavematch)]
    private static void leaverequest(ushort pid, Message message) // player left his match
    {
        if (Player.plist.ContainsKey(pid))
        {
            Player.plist[pid].status = "none";
            Player.plist[pid].matchid = "";
        }

        string mid = Player.plist[pid].matchid;

        if (mlist.ContainsKey(mid) && mlist[mid].players.ContainsKey(pid))
        {
            mlist[mid].players.Remove(pid);
        }

        Message msg = Message.Create(MessageSendMode.reliable, ServerToClient.matchstatus);
        msg.AddUShort(pid);
        msg.AddString(Player.plist[pid].Username);
        msg.AddString(Player.plist[pid].cara);
        msg.AddBool(false);

        foreach (ushort cid in mlist[mid].players.Keys)
        {
            NetworkManager.Singleton.Server.Send(msg, cid);
        }

        if (Player.plist.ContainsKey(pid) && Player.plist[pid].Username != "")
        {
            NetworkManager.log(Player.plist[pid].Username + " left match " + mid, "M");
        }
        else
        {
            NetworkManager.log("Player " + pid + " left match " + mid, "M");
        }

        if (mlist.ContainsKey(mid) && mlist[mid].players.Count == 0)
        {
            mlist.Remove(mid);
            NetworkManager.log("no player in match " + mid + ", match removed", "M");
        }
    }

    [MessageHandler((ushort)ClientToServerId.playerposupdate)]
    private static void playerupdate_forward(ushort pid, Message message)
    {
        if (!Player.plist.ContainsKey(pid))
        {
            NetworkManager.log("Client ID unregistered ("+pid+")", "M");
            return;
        } else if (!Match.mlist.ContainsKey(Player.plist[pid].matchid))
        {
            NetworkManager.log("User Match unregistered ("+ Player.plist[pid].matchid + ")", "M");
            return;
        } else if (mlist[Player.plist[pid].matchid].map >= maps.Count)
        {
            NetworkManager.log("Unregistered map ("+ mlist[Player.plist[pid].matchid].map + ")", "M");
            return;
        }

        string mid = Player.plist[pid].matchid;

        if (!Match.mlist.ContainsKey(mid))
        {
            Player.plist[pid].matchid = "";
            return;
        }

        string status = message.GetString();
        Vector3 ppos = message.GetVector3();
        mlist[mid].players[pid].position = ppos;

        Vector3 arp = maps[mlist[mid].map].Item3;

        if (ppos.x >= arp.x) // #playerfinished
        {
            Player.plist[pid].status = "finished";

            Message msgg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.matchend);
            msgg.AddUShort(pid);
            msgg.AddInt(Mathf.RoundToInt(Time.realtimeSinceStartup - Match.mlist[Player.plist[pid].matchid].starttime));

            int rank = mlist[mid].awn+1;
            int gxp = mlist[mid].ExpOfRank(rank);
            msgg.AddString(NetworkManager.endofmatch(pid, rank, gxp)+"|"+gxp.ToString()+","+Math.Max(5, Mathf.RoundToInt(gxp*1.5f)).ToString());

            NetworkManager.Singleton.Server.Send(msgg, pid);
            

            Message msg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.matchend);
            msg.AddUShort(pid);
            msg.AddInt(Mathf.RoundToInt(Time.realtimeSinceStartup - Match.mlist[Player.plist[pid].matchid].starttime));

            foreach (Player pl in mlist[mid].players.Values)
            {
                if (pl.Id != pid) NetworkManager.Singleton.Server.Send(msg, pl.Id);
            }

            NetworkManager.log(Player.plist[pid].Username + " (" + pid + ") finished "+rank.ToString(), "PA");

            mlist[mid].awn++;

            foreach(Player p in mlist[mid].players.Values)
            {
                if (p.status!="finished")
                {
                    return;
                }
            }
            mlist.Remove(mid);
            NetworkManager.log("Match "+mid+" is finished.", "M");
        }
        else
        {
            Player.plist[pid].UpdateEffects();

            EffectBlock.UpdateAll(mlist[mid]);
            foreach(EffectBlock eb in mlist[mid].EffectBlocks)
            {
                eb.AffectPlayer(pid);
            }

            if (!Player.plist[pid].canmove)
            {
                return;
            }

            if (!Player.plist[pid].visible)
            {
                return;
            }

            Message msg = Message.Create(MessageSendMode.unreliable, (ushort)ServerToClient.rcvplayerposupdate);

            msg.AddString(status);
            msg.AddUShort(pid);
            msg.AddVector3(ppos);

            foreach (Player pl in mlist[mid].players.Values)
            {
                if (pl.Id != pid && Vector3.Distance(ppos, pl.position)<13f)
                {
                    NetworkManager.Singleton.Server.Send(msg, pl.Id);
                }
            }
        }
    }
}