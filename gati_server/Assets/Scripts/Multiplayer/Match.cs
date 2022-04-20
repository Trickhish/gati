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
    public static List<(string, (float, float), (float, float))> maps = new List<(string, (float, float), (float, float))>() {
        ("Etril Sewer", (-192f, 0f), (2f, -3.5f)),
        ("Niya City", (-192f, 0f), (2f, -3.5f)),
        ("Ayrith Forest", (-192, 0), (2f, -3.5f)),
        ("Bravo Camp", (-192f, 0f), (2f, -3.5f)),
        ("Pirate Beach", (-192f, 0f), (2f, -3.5f)),
        ("Maya Temple", (-192f, 0f), (2f, -3.5f)),
        ("Snowy Mountain", (-192f, 0f), (2f, -3.5f)),
        ("Prehistory", (-192f, 0f), (2f, -3.5f)),
        ("Camda", (-192f, 0f), (2f, -3.5f)),
    };

    static System.Random rand = new System.Random();

    public bool isprivate { get; private set; }
    public int capacity { get; private set; }
    public string status { get; private set; }
    public int map { get; private set; }

    public Dictionary<ushort, Player> players = new Dictionary<ushort, Player>();

    public string id { get; private set; }

    public Match()
    {

    }

    public static string getrandomid(int l)
    {
        string rst = "";

        for (int i=0;i<l;i++)
        {
            int rnd = rand.Next(0,36);
            if (rnd < 26)
            {
                rst += (char)(rnd+65);
            } else
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
            id = getrandomid(5+(i/50));
            i++;
        }

        Match match = new Match();
        match.id = id;
        match.capacity = capacity;
        match.isprivate = isprivate;
        match.players = new Dictionary<ushort, Player>();
        match.status = "filling";
        match.map = rand.Next(0,1);

        mlist.Add(id, match);

        return (id);
    }

    public static string findmatch()
    {
        foreach(Match m in mlist.Values)
        {
            if (m.players.Count < m.capacity && !m.isprivate)
            {
                return(m.id);
            }
        }
        return(null);
    }

    public void launch()
    {
        if (this.capacity == this.players.Count)
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.launch);

            foreach (ushort pid in players.Keys)
            {
                NetworkManager.Singleton.Server.Send(message, pid);
            }
        }
    }

    public static void sendmatch(ushort clientid, string mid)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.match);
        message.AddString(mid);
        message.AddInt(mlist[mid].players.Count);
        message.AddInt(mlist[mid].capacity);

        foreach(Player pl in mlist[mid].players.Values)
        {
            Debug.Log("player "+pl.Username+" spawn request");
            message.AddUShort(pl.Id);
            message.AddString(pl.Username);
            //message.AddVector3(pl.position);
        }

        NetworkManager.Singleton.Server.Send(message, clientid);
    }

    public void sendmatchstatus(ushort pid, string username, bool joined)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.matchstatus);
        message.AddUShort(pid);
        message.AddString(username);
        message.AddBool(joined);

        foreach (Player pl in this.players.Values)
        {
            NetworkManager.Singleton.Server.Send(message, pl.Id);
        }

        if (joined && this.players.Count == this.capacity)
        {
            this.launch();
        }
    }

    [MessageHandler((ushort)ClientToServerId.createprivate)]
    private static void privatematch(ushort cid, Message message)
    {

        int pc = message.GetInt();
        int map = message.GetInt();
        string username = message.GetString();

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
        match.map = map;

        mlist.Add(id, match);

        if (!Player.plist.ContainsKey(cid))
        {
            Player.plist.Add(cid, new Player(cid, username, id));
        }
        else
        {
            Player.plist[cid].Username = username;
            Player.plist[cid].matchid = id;
        }

        mlist[id].players.Add(cid, Player.plist[cid]);

        sendmatch(cid, id);

        if (mlist[id].players.Count == mlist[id].capacity)
        {
            mlist[id].launch();
            Debug.Log("test match, launching right away");
        }

        Debug.Log("match "+id+" created");
    }

    [MessageHandler((ushort)ClientToServerId.joinprivate)]
    private static void joinprivatematch(ushort clientid, Message message)
    {
        string username = message.GetString();
        string mid = message.GetString().ToUpper();

        if (Match.mlist.ContainsKey(mid))
        {
            if (!Player.plist.ContainsKey(clientid))
            {
                Player.plist.Add(clientid, new Player(clientid, username, mid));
            }
            else
            {
                Player.plist[clientid].Username = username;
                Player.plist[clientid].matchid = mid;
            }

            mlist[mid].players.Add(clientid, Player.plist[clientid]);

            sendmatch(clientid, mid);

            mlist[mid].sendmatchstatus(clientid, username, true);
        }
    }

    [MessageHandler((ushort)ClientToServerId.findmatch)]
    private static void getorcreatematch(ushort clientid, Message message)
    {
        string username = message.GetString();

        string mid = findmatch();

        if (mid == null)
        {
            mid = creatematch(false, 5);
            Debug.Log("New match created with id "+mid);
        } else
        {
            Debug.Log("Match found with id "+mid);
        }

        if (!Player.plist.ContainsKey(clientid))
        {
            Player.plist.Add(clientid, new Player(clientid, username, mid));
        } else
        {
            Player.plist[clientid].Username = username;
            Player.plist[clientid].matchid = mid;
        }

        mlist[mid].players.Add(clientid, Player.plist[clientid]);

        sendmatch(clientid, mid);

        mlist[mid].sendmatchstatus(clientid, username, true);
    }

    [MessageHandler((ushort)ClientToServerId.leavematch)]
    private static void leaverequest(ushort pid, Message message)
    {
        string mid = Player.plist[pid].matchid;

        if (mlist[mid].players.ContainsKey(pid))
        {
            mlist[mid].players.Remove(pid);
        }

        Message msg = Message.Create(MessageSendMode.reliable, ServerToClient.matchstatus);
        msg.AddUShort(pid);
        msg.AddString(Player.plist[pid].Username);
        msg.AddBool(false);

        foreach(ushort cid in mlist[mid].players.Keys)
        {
            NetworkManager.Singleton.Server.Send(msg, cid);
        }

        if (mlist[mid].players.Count == 0)
        {
            mlist.Remove(mid);
            Debug.Log("no player in match " + mid + ", match removed");
        }
    }

    [MessageHandler((ushort)ClientToServerId.playerposupdate)]
    private static void playerupdate_forward(ushort pid, Message message)
    {
        string mid = Player.plist[pid].matchid;

        string status = message.GetString();
        Vector3 ppos = message.GetVector3();

        //Debug.Log("pos update : "+ mlist[mid].players[pid].position.x.ToString()+" > "+ppos.x.ToString());

        (float, float) ar = maps[mlist[mid].map].Item3;
        Vector3 arp = new Vector3(ar.Item1, ar.Item2, 0);

        if (Vector3.Distance(ppos, arp) < 5)
        {
            Message msg = Message.Create(MessageSendMode.unreliable, (ushort)ServerToClient.matchend);

            msg.AddString(mlist[mid].players[pid].Username);
            msg.AddUShort(pid);

            foreach (Player pl in mlist[mid].players.Values)
            {
                NetworkManager.Singleton.Server.Send(msg, pl.Id);
            }

            mlist.Remove(mid);

        } else
        {
            Message msg = Message.Create(MessageSendMode.unreliable, (ushort)ServerToClient.rcvplayerposupdate);

            msg.AddString(status);
            msg.AddUShort(pid);
            msg.AddVector3(ppos);

            mlist[mid].players[pid].position = ppos;

            foreach (Player pl in mlist[mid].players.Values)
            {
                if (pl.Id != pid)
                {
                    NetworkManager.Singleton.Server.Send(msg, pl.Id);
                }
            }
        }
    }
}
