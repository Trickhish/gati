using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using System.Net;
using System.IO;
using System.Web;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;

public enum ServerToClient : ushort
{
    playerSpawned = 1,
    match = 2,
    matchstatus = 3,
    updatepos = 4,
    rcvplayerupdate = 5,
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
                Debug.Log($"{nameof(NetworkManager)}");
                Destroy(value);
            }
        }
    }

    public Client Client { get; private set; }

    [SerializeField] public string ip;
    [SerializeField] public ushort port;
    [SerializeField] private GameObject connectUI;
    public string status;
    public string token;
    

    private void Awake()
    {
        Singleton = this;
    }

    void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        status = "";
        Client = new Client();
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;
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

    public static async Task<string> getreq_as(string url)
    {
        string result;
        using (var client = new System.Net.Http.HttpClient())
        {
            result = await client.GetStringAsync(url);
        }
        return(result);
    }
    public void rldt()
    {
        string dt = getreq("https://trickhisch.alwaysdata.net/gati/?a=dts&t=" + HttpUtility.UrlEncode(NetworkManager.Singleton.token));

        if (dt == "unreachable" && dt == "false")
        {

        }
        else
        {
            string[] dts = dt.Substring(2, dt.Length - 4).Split(new[] { '\u0022' + "," + '\u0022' }, StringSplitOptions.None);
            //Debug.Log(string.Join(" | ", dts));

            UIManager.Singleton.usertext_lo.text = dts[0];
            UIManager.localusername = dts[0];
            Player.money = int.Parse(dts[2]);
            Player.mail = dts[1];

            UIManager.Singleton.shop_money.text = "Money : " + dts[2] + "$";
            UIManager.Singleton.user_name.text = dts[0];
        }

    }
    public (Dictionary<string, int>, List<string>) getassets()
    {
        string r = getreq("https://trickhisch.alwaysdata.net/gati/?a=assets&t=" + HttpUtility.UrlEncode(NetworkManager.Singleton.token));

        if (r == "unreachable" && r == "false")
        {
            return (new Dictionary<string, int>(), new List<string>());
        }
        else
        {
            string l1 = r.Split('|')[0];
            string l2 = r.Split('|')[1];

            Dictionary<string, int> items = new Dictionary<string, int>();
            foreach (string e in l1.Split(','))
            {
                items[e.Split(':')[0]] = int.Parse(e.Split(':')[1]);
            }

            List<string> skins = l2.Split(',').ToList();

            return ((items, skins));
        }
    }

    public List<int> getstats()
    {
        string r = getreq("https://trickhisch.alwaysdata.net/gati/?a=stats&t=" + HttpUtility.UrlEncode(NetworkManager.Singleton.token));

        if (r == "unreachable" && r == "false")
        {
            return (new List<int>());
        }
        else
        {
            List<int> stats = new List<int>();

            foreach (string e in r.Split(','))
            {
                stats.Add(int.Parse(e));
            }

            return(stats);
        }
    }

    public static void rlstats(string force="")
    {
        string r = (force=="") ? getreq("https://trickhisch.alwaysdata.net/gati/?a=stats&t=" + HttpUtility.UrlEncode(NetworkManager.Singleton.token)) : force;

        if (r != "unreachable" && r != "false" && r!="error")
        {
            string[] l = r.Split(',');
            int level = int.Parse(l[0]);
            int exp = int.Parse(l[1]);
            int mcount = int.Parse(l[2]);
            int wcount = int.Parse(l[3]);
            int wpct = (int)Decimal.Round(Decimal.Divide(wcount, mcount) * 100);
            int lvlpct = (int)Decimal.Round(Decimal.Divide(exp, GameLogic.NexpOfLvl(level)) * 100);

            GameLogic.local_level = level;
            GameLogic.local_wcount = wcount;
            GameLogic.local_mcount = mcount;
            GameLogic.local_exp = exp;

            UIManager.Singleton.user_stats.transform.Find("progress").GetComponent<Slider>().value = lvlpct;

            UIManager.Singleton.user_stats.text = "Level : " + level.ToString() + "\n\nWin Ratio : " + wpct.ToString() + "%";
        }
    }

    public static async Task _rlmoney_as()
    {
        await Task.Run(async () =>
        {
            string r =  await getreq_as("https://trickhisch.alwaysdata.net/gati/?a=money&t=" + HttpUtility.UrlEncode(NetworkManager.Singleton.token));

            if (r == "unreachable" || r == "false")
            {

            }
            else
            {
                int m = int.Parse(r);
                Player.money = m;
                UIManager.Singleton.shop_money.text = "Money : " + m.ToString() + "$";

                foreach (shop_item i in shop_item.items.Values)
                {
                    if (m >= i.price)
                    {
                        i.GetComponent<Button>().interactable = true;
                    }
                    else
                    {
                        i.GetComponent<Button>().interactable = false;
                    }
                }
            }
        });
    }

    public void rlmoney_as()
    {
        _rlmoney_as();
    }
    public void rlmoney()
    {
        string r = getreq("https://trickhisch.alwaysdata.net/gati/?a=money&t=" + HttpUtility.UrlEncode(NetworkManager.Singleton.token));

        if (r == "unreachable" || r == "false")
        {

        }
        else
        {
            int m = int.Parse(r);
            Player.money = m;
            UIManager.Singleton.shop_money.text = ": " + m.ToString();

            foreach (shop_item i in shop_item.items.Values)
            {
                if (m >= i.price && i.realitem)
                {
                    i.GetComponent<Button>().interactable = true;
                }
                else
                {
                    i.GetComponent<Button>().interactable = false;
                }
            }
        }
    }

    public static async Task _rlitems_as()
    {
        await Task.Run(async () =>
        {
            string r = await getreq_as("https://trickhisch.alwaysdata.net/gati/?a=items&t=" + HttpUtility.UrlEncode(NetworkManager.Singleton.token));

            if (r == "unreachable" && r == "false")
            {

            }
            else
            {
                try
                {
                    foreach (string e in r.Split(','))
                    {
                        //Debug.Log(e);
                        if (shop_item.items.ContainsKey(e.Split(':')[0]))
                        {
                            shop_item.items[e.Split(':')[0]].setstock(int.Parse(e.Split(':')[1]));
                        }
                        else
                        {
                            shop_item.items.Add(e.Split(':')[0], new shop_item(e.Split(':')[0], int.Parse(e.Split(':')[1])));
                        }
                    }
                }
                catch
                {
                    UIManager.Singleton.shopUI.SetActive(false);
                }
            }
        });
    }

    public void rlitems_as()
    {
        _rlitems_as();
    }
    public void rlitems()
    {
        string r = getreq("https://trickhisch.alwaysdata.net/gati/?a=items&t=" + HttpUtility.UrlEncode(NetworkManager.Singleton.token));

        if (r == "unreachable" && r == "false")
        {

        }
        else
        {
            try {
                foreach (string e in r.Split(','))
                {
                    //Debug.Log(e);
                    GameLogic.playersitems[e.Split(':')[0].ToLower()] = int.Parse(e.Split(':')[1]);

                    if (shop_item.items.ContainsKey(e.Split(':')[0]))
                    {
                        shop_item.items[e.Split(':')[0]].setstock(int.Parse(e.Split(':')[1]));
                    }
                    else
                    {
                        shop_item.items.Add(e.Split(':')[0], new shop_item(e.Split(':')[0], int.Parse(e.Split(':')[1])));
                    }
                }
            } catch
            {
                UIManager.Singleton.shopUI.SetActive(false);
            }
        }
    }

    public static async Task _rlassets_as()
    {
        await Task.Run(async () =>
        {
            string r = await getreq_as("https://trickhisch.alwaysdata.net/gati/?a=assets&t=" + HttpUtility.UrlEncode(NetworkManager.Singleton.token));

            if (r == "unreachable" && r == "false")
            {

            }
            else
            {
                string l1 = r.Split('|')[0];
                string l2 = r.Split('|')[1];

                Dictionary<string, int> items = new Dictionary<string, int>();
                foreach (string e in r.Split(','))
                {
                    if (shop_item.items.ContainsKey(e.Split(':')[0]))
                    {
                        shop_item.items[e.Split(':')[0]].setstock(int.Parse(e.Split(':')[1]));
                    }
                }

                List<string> skins = l2.Split(',').ToList();
            }
        });
    }

    public void rlassets_as()
    {
        _rlassets_as();
    }
    public void rlassets()
    {
        string r = getreq("https://trickhisch.alwaysdata.net/gati/?a=assets&t=" + HttpUtility.UrlEncode(NetworkManager.Singleton.token));

        if (r == "unreachable" && r == "false")
        {

        }
        else
        {
            string l1 = r.Split('|')[0];
            string l2 = r.Split('|')[1];

            Dictionary<string, int> items = new Dictionary<string, int>();
            foreach (string e in r.Split(','))
            {
                if (shop_item.items.ContainsKey(e.Split(':')[0]))
                {
                    shop_item.items[e.Split(':')[0]].setstock(int.Parse(e.Split(':')[1]));
                }
            }

            List<string> skins = l2.Split(',').ToList();
        }
    }

    private void FixedUpdate()
    {
        Client.Tick();
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
    }

    public void Connect()
    {
        status = "connecting";
        Client.Connect($"{ip}:{port}");
    }

    public void creatematch()
    {

    }

    public void DidConnect(object sender, EventArgs e)
    {
        status = "connected";
        

        //UIManager.Singleton.SendName();
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        status = "failed";
        //connectUI.SetActive(true);
        //UIManager.Singleton.connectbt.interactable = true;
        //UIManager.Singleton.statustext.text = "Connection Failed";
        UIManager.Singleton.statustext.color = new Color(177,50,50);
        //UIManager.Singleton.BackToMain();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        //Destroy(Player.list[e.Id]);

        string username = GameLogic.Singleton.matchplayers[e.Id].username;

        if (GameLogic.Singleton.matchplayers.ContainsKey(e.Id))
        {
            Destroy(GameLogic.Singleton.matchplayers[e.Id].listitem.gameObject);
            Destroy(GameLogic.Singleton.matchplayers[e.Id].gameObject);
            GameLogic.Singleton.matchplayers.Remove(e.Id);
            GameLogic.Singleton.pcount--;
        }
        
        Debug.Log(username + " disconnected, " + GameLogic.Singleton.pcount.ToString() + "/" + GameLogic.Singleton.capacity.ToString());
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
        Debug.Log("disconnected");
        status = "disconnected";
        UIManager.Singleton.BackToMain();
    }
}
