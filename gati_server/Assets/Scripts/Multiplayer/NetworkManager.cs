using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Web;

public enum ServerToClient : ushort
{
    playerSpawned = 1,
    match = 2,
    matchstatus = 3,
    updatepos = 4,
    rcvplayerposupdate = 5,
    launch = 6,
    matchend = 7,
    effect = 8,
    itemused = 9,
    effectblock = 10,
}

public enum ClientToServerId : ushort
{
    name = 1,
    createprivate = 2,
    findmatch = 3,
    leavematch = 4,
    playerposupdate = 5,
    joinprivate = 6,
    login = 7,
    effect = 8,
    useitem = 9,
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                //Debug.Log($"{nameof(NetworkManager)}");
                Destroy(value);
            }
        }
    }

    public Server Server { get; private set; }
    public static string sat = "ShOi3HUwJ1BdRfj5eFrQcsM40PGaT82Vk9Imluy7xo6vnXLCbKWq";
    public static string version;

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxclientcount;

    private void Awake()
    {
        Singleton = this;
    }

    public static void log(string msg, string elv)
    {
        string dt = ((DateTime)DateTime.Now).ToString("HH:mm:ss");

        Debug.Log("[" + dt + "]" + elv + ": " + msg);
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        //RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        if (File.Exists(Application.dataPath + "/version"))
        {
            StreamReader sr = new StreamReader(Application.dataPath + "/version");
            version = sr.ReadToEnd();
        }
        else
        {
            version = "Debug";
        }

        Server = new Server();
        Server.Start(port, maxclientcount);
        if (Server.IsRunning)
        {
            log("Server Started with version " + version, "SS");
        }
        Server.ClientDisconnected += PlayerLeft;
        Server.ClientConnected += PlayerJoined;
    }

    private void FixedUpdate()
    {
        Server.Tick();
    }

    private void OnApplicationQuit()
    {
        log("Server Stopped.", "SS");
        Server.Stop();
    }

    public static string getreq(string uri)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return ("unreachable");
                }

                if (!new List<int>() { 200, 201, 202, 203, 204, 205, 206, 207, 208, 210, 226 }.Contains((int)response.StatusCode))
                {
                    return ("error");
                }

                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        catch
        {
            return ("unreachable");
        }
    }

    public static void renew(ushort pid)
    {
        string mail = Player.plist[pid].Mail;

        string r = getreq("https://trickhisch.alwaysdata.net/gati/?a=renew&m=" + HttpUtility.UrlEncode(mail) + "&sat=" + sat);
    }

    public static string endofmatch(ushort pid, int rank, int xp)
    {
        string r = getreq("https://trickhisch.alwaysdata.net/gati/?a=matchend&m=" + HttpUtility.UrlEncode(Player.plist[pid].Mail) + "&sat=" + sat + "&rk=" + HttpUtility.UrlEncode(rank.ToString()) + "&xp=" + HttpUtility.UrlEncode(xp.ToString()));

        if (r == "false" || r == "unreachable" || r == "error")
        {
            return("false");
        }

        return(r);
    }

    public static void setstatus(ushort pid, bool ingame)
    {
        string mail = Player.plist[pid].Mail;
        string r="false";

        if (ingame)
        {
            r = getreq("https://trickhisch.alwaysdata.net/gati/?a=sig&m=" + HttpUtility.UrlEncode(mail) + "&sat=" + sat);
        }
        else
        {
            r = getreq("https://trickhisch.alwaysdata.net/gati/?a=snig&m=" + HttpUtility.UrlEncode(mail) + "&sat=" + sat);
        }

        if (r == "false" || r=="unreachable" || r=="error")
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);

                setstatus(pid, ingame);
            });
        }
    }

    public static bool useitem(ushort pid, string item)
    {
        string mail = Player.plist[pid].Mail;

        string r = getreq("https://trickhisch.alwaysdata.net/gati/?a=use&m=" + HttpUtility.UrlEncode(mail) + "&sat=" + sat+"&i="+ HttpUtility.UrlEncode(item));

        return(r=="true");
    }

    private void PlayerJoined(object sender, ServerClientConnectedEventArgs e)
    {
        ushort pid = e.Client.Id;

        if (!Player.plist.ContainsKey(pid))
        {
            Player.plist.Add(pid, new Player(pid));
        }

        log("Player " + pid + " joined.", "J");
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        if (Player.plist.ContainsKey(e.Id))
        {
            if (Player.plist[e.Id].Username != "")
            {
                log("Player " + e.Id + " (" + Player.plist[e.Id].Username + ") left.", "L");
            }
            else
            {
                log("Player " + e.Id + " left.", "L");
            }

            setstatus(e.Id, false);

            string mid = Player.plist[e.Id].matchid;

            if (mid != "" && Match.mlist.ContainsKey(mid))
            {
                if (Match.mlist[mid].players.ContainsKey(e.Id))
                {
                    Match.mlist[mid].players.Remove(e.Id);
                }

                if (Match.mlist[mid].players.Count == 0)
                {
                    Match.mlist.Remove(mid);
                    log("no player in match " + mid + ", match removed", "M");
                }
            }

            Player.plist.Remove(e.Id);
        }
        else
        {
            log("Player " + e.Id + " left.", "L");
        }

    }

    public static string[] hashstring(string input)
    {
        int SALT_SIZE = 24;
        int HASH_SIZE = 24;
        int ITERATIONS = 100000;

        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        byte[] salt = new byte[SALT_SIZE];
        provider.GetBytes(salt);

        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(input, salt, ITERATIONS);

        return (new string[] { System.Text.Encoding.UTF8.GetString(salt), System.Text.Encoding.UTF8.GetString(pbkdf2.GetBytes(HASH_SIZE)) });
    }

    [MessageHandler((ushort)ClientToServerId.login)]
    private static void login(ushort cid, Message m)
    {
        string username = m.GetString();
        string mail = m.GetString();

        log("Player " + cid + ", signed in: " + username + ", " + mail, "S");

        if (Player.plist.ContainsKey(cid))
        {
            Player.plist[cid].Mail = mail;
            Player.plist[cid].rlitems();
            Player.plist[cid].Username = username;
        }
        else
        {
            Player.plist.Add(cid, new Player(cid, mail, username));
        }

        setstatus(cid, true);
    }
}