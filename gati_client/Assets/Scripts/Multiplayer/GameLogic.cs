using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RiptideNetworking;
using System.Linq;
using System.IO;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] public GameObject local_prefab;

    [SerializeField] public GameObject gati_prefab;
    [SerializeField] public GameObject drije_prefab;

    [SerializeField] public GameObject gamescene;
    [SerializeField] public TMP_Text gameidtext;

    [SerializeField] public GameObject finishflag;

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

    public Dictionary<ushort, Player> matchplayers = new Dictionary<ushort, Player>();
    public int capacity;
    public string id="";
    public int pcount;
    public string status;
    public static int stct = -2;
    public float t = -1;
    public float maxpos;
    public Vector3 startpos = new Vector3(0,0,0);
    public Vector3 endpos;
    public static int choosenitem=0;
    public static Dictionary<string, int> playersitems = new Dictionary<string, int>(){
        {"bomb", 0},
        {"adrenaline", 0}
    };

    public Player localplayer => (matchplayers.ContainsKey(NetworkManager.Singleton.Client.Id) ? matchplayers[NetworkManager.Singleton.Client.Id] : matchplayers.FirstOrDefault().Value);

    //public GameObject Gati_prefab => gati_prefab;
    //public GameObject Localplayerprefab => local_prefab;


    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        if (!File.Exists(Application.dataPath + "/config"))
        {
            using (StreamWriter sw = new StreamWriter(Application.dataPath + "/config"))
            {
                string config = "Left:276\nRight:275\nJump:273\nSneak:274\nCapacity:101\nPrevious Object:108\nNext Object:109\nUse Item:114\nPlayers List:9\nEscape Menu:27";

                sw.Write(config);
            }
        }
        else
        {
            using (StreamReader sr = new StreamReader(Application.dataPath + "/config"))
            {
                loadconfig(sr.ReadToEnd());
            }
        }
    }
    private void Update()
    {
        if (Time.realtimeSinceStartup - t > 1 && stct >= -1)
        {
            t = Time.realtimeSinceStartup;
            if (stct == 0)
            {
                localplayer.GetComponent<Movement>().enabled = true;
                UIManager.Singleton.stcounter.text = "GO";
            }
            else if (stct <= -1)
            {
                UIManager.Singleton.stcounter.text = "";
                stct = -2;
                UIManager.Singleton.start_ui.SetActive(false);
            }
            else
            {
                UIManager.Singleton.stcounter.text = stct.ToString();
            }
            stct -= 1;
        }

        if (Input.GetKeyDown((KeyCode)Movement.keys["Next Object"])) // NEXT OBJECT
        {
            nextitem();
        }
        else if (Input.GetKeyDown((KeyCode)Movement.keys["Previous Object"])) // PREVIOUS OBJECT
        {
            previtem();
        }
        // ITEM USAGE
        if (Input.GetKeyDown((KeyCode)Movement.keys["Use Item"]) && choosenitem>=0 && choosenitem<UIManager.Singleton.item_bar.transform.childCount-1 && playersitems[UIManager.Singleton.item_bar.transform.GetChild(choosenitem+1).name] > 0)
        {
            Debug.Log("using item "+ UIManager.Singleton.item_bar.transform.GetChild(choosenitem + 1).name);

            Message msg = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.useitem);
            msg.AddString(UIManager.Singleton.item_bar.transform.GetChild(choosenitem + 1).name);
            msg.AddVector2(localplayer.transform.position);
            NetworkManager.Singleton.Client.Send(msg);

            //NetworkManager.Singleton.rlitems();
            loaditems();
        }
    }

    public static void setitemkey(string name, KeyCode kc)
    {
        for (int i = 0; i < UIManager.Singleton.item_bar.transform.childCount; i++)
        {
            GameObject cgo = UIManager.Singleton.item_bar.transform.GetChild(i).gameObject;

            if (cgo.name == name)
            {
                cgo.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = kc.ToString(); // key
                break;
            }
        }
    }

    public static void setitemstock(string name, int stock)
    {
        for (int i = 0; i < UIManager.Singleton.item_bar.transform.childCount; i++)
        {
            GameObject cgo = UIManager.Singleton.item_bar.transform.GetChild(i).gameObject;

            if (cgo.name == name)
            {
                cgo.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = stock.ToString(); // stock
                break;
            }
        }
    }

    public static void selectitem(string name)
    {
        for (int i = 0; i < UIManager.Singleton.item_bar.transform.childCount; i++)
        {
            GameObject cgo = UIManager.Singleton.item_bar.transform.GetChild(i).gameObject;

            if (cgo.name == name)
            {
                choosenitem = i;
                UIManager.Singleton.item_outline.transform.position = cgo.transform.position;
                break;
            }
        }
    }

    public static void selectitem(int i)
    {
        if (i<0 || i>=UIManager.Singleton.item_bar.transform.childCount-1)
        {
            choosenitem = -1;
            UIManager.Singleton.item_outline.SetActive(false);
        } else
        {
            UIManager.Singleton.item_outline.SetActive(true);

            GameObject cgo = UIManager.Singleton.item_bar.transform.GetChild(i + 1).gameObject;

            choosenitem = i;
            UIManager.Singleton.item_outline.transform.position = cgo.transform.position;
        }
    }

    public static void nextitem()
    {
        int i = choosenitem + 1;
        while (i<UIManager.Singleton.item_bar.transform.childCount-1 && playersitems[UIManager.Singleton.item_bar.transform.GetChild(i + 1).name] <= 0)
        {
            i++;
        }

        if (i < UIManager.Singleton.item_bar.transform.childCount - 1 && playersitems[UIManager.Singleton.item_bar.transform.GetChild(i+1).name] > 0)
        {
            choosenitem = i;
        }

        if (choosenitem >= UIManager.Singleton.item_bar.transform.childCount-1)
        {
            choosenitem = UIManager.Singleton.item_bar.transform.childCount-2;
        }
        selectitem(choosenitem);
    }

    public static void previtem()
    {
        int i = choosenitem-1;
        while (i>=0 && playersitems[UIManager.Singleton.item_bar.transform.GetChild(i+1).name] <= 0)
        {
            i--;
        }

        if (i>=0 && playersitems[UIManager.Singleton.item_bar.transform.GetChild(i + 1).name] > 0)
        {
            choosenitem=i;
        }

        if (choosenitem < 0)
        {
            choosenitem = 0;
        }
        selectitem(choosenitem);
    }

    public static int getfirstitem()
    {
        int i = 0;
        foreach(string k in playersitems.Keys)
        {
            if (playersitems[k] > 0)
            {
                return(i);
            }
            i++;
        }
        return(-1);
    }

    public static void loaditems()
    {
        for (int i = 1; i < UIManager.Singleton.item_bar.transform.childCount; i++)
        {
            GameObject cgo = UIManager.Singleton.item_bar.transform.GetChild(i).gameObject;

            if (cgo.name != "selected_item" && cgo.transform.childCount>0)
            {
                //Debug.Log(cgo.name+" : "+ cgo.transform.childCount.ToString()+" : "+string.Join(", ", playersitems.Keys));

                if (Movement.keys.ContainsKey(cgo.name.ToLower()))
                {
                    cgo.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = ((KeyCode)Movement.keys[cgo.name.ToLower()]).ToString(); // key
                } else
                {
                    cgo.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = ""; // key
                }
                TMP_Text tt = cgo.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();

                tt.text = playersitems[cgo.name.ToLower()].ToString();  // stock
            }
        }
    }

    public static void applykeys()
    {
        for (int i = 0; i < UIManager.Singleton.keysgroup.transform.childCount; i++)
        {
            GameObject ko = UIManager.Singleton.keysgroup.transform.GetChild(i).gameObject;

            string nm = ((KeyCode)Movement.keys[ko.name]).ToString().ToUpper();
            //🡨🡪🡩🡫
            if (nm == "LEFTARROW")
            {
                nm = "LEFT";
            }
            else if (nm == "RIGHTARROW")
            {
                nm = "RIGHT";
            }
            else if (nm == "UPARROW")
            {
                nm = "UP";
            }
            else if (nm == "DOWNARROW")
            {
                nm = "DOWN";
            } else
            {
                nm = nm.Replace("KEYPAD", "KP").Replace("MOUSE", "M").Replace("CAPSLOCK", "CPS.LK");
            }

            ko.GetComponent<TMP_Text>().text = ko.name + " : " + nm;
        }

        using (StreamWriter sw = new StreamWriter(Application.dataPath + "/config"))
        {
            string ct = "";
            foreach(string k in Movement.keys.Keys)
            {
                if (ct!="")
                {
                    ct += "\n";
                }

                ct += k + ":" + Movement.keys[k].ToString();
            }
            sw.Write(ct);
        }
    }

    public static void loadconfig(string config)
    {
        foreach(string l in config.Split("\n"))
        {
            string key = l.Split(":")[0];
            string val = l.Split(":")[1];

            if (Movement.keys.ContainsKey(key))
            {
                int kc = Movement.keys[key];
                int.TryParse(val, out kc);
                Movement.keys[key] = kc;

                //Debug.Log(key+" : "+val+" | "+kc.ToString());
            }
        }

        applykeys();
    }

    public static GameObject prefabofcara(string cara) // #addacara
    {
        switch(cara.ToLower())
        {
            case "gati":
                return(GameLogic.Singleton.gati_prefab);
            case "drije":
                return(GameLogic.Singleton.drije_prefab);
            default:
                return(GameLogic.Singleton.local_prefab);
        }
    }

    [MessageHandler((ushort)ServerToClient.matchend)]
    private static void endofmatch(Message message)
    {
        string winname = message.GetString();
        ushort winid = message.GetUShort();
        
        if (winid == NetworkManager.Singleton.Client.Id) // WON
        {
            Debug.Log("WON");
            UIManager.Singleton.leavematch();
        } else // LOST
        {
            Debug.Log("LOST");
            UIManager.Singleton.leavematch();
        }
    }
    [MessageHandler((ushort)ServerToClient.launch)]
    private static void launchmatch(Message message)
    {
        NetworkManager.Singleton.rlitems();
        loaditems();
        selectitem(getfirstitem());

        GameLogic.Singleton.maxpos = Vector3.Distance(GameLogic.Singleton.startpos, GameLogic.Singleton.endpos);

        Debug.Log("Match Starting");

        GameLogic.Singleton.gamescene.SetActive(true);
        UIManager.Singleton.pgr_slider.SetActive(true);

        UIManager.Singleton.item_bar.SetActive(true);
        choosenitem = 0;

        UIManager.Singleton.connectUI.SetActive(false);
        UIManager.Singleton.menuUI.SetActive(false);
        UIManager.Singleton.waitUI.SetActive(false);

        cam.trans.position = new Vector3(-4.2f, 3.7f, 0f);

        foreach (Player p in GameLogic.Singleton.matchplayers.Values)
        {
            p.gameObject.SetActive(true);
        }

        UIManager.Singleton.start_ui.SetActive(true);

        stct = 5;
    }

    [MessageHandler((ushort)ServerToClient.rcvplayerupdate)]
    private static void updateplayerpos(Message message)
    {
        string status = message.GetString();
        ushort pid = message.GetUShort();
        Vector2 ppos = message.GetVector2();

        Animator amt = GameLogic.Singleton.matchplayers[pid].GetComponent<Animator>();
        SpriteRenderer spr = GameLogic.Singleton.matchplayers[pid].GetComponent<SpriteRenderer>();

        //Debug.Log(status);
        switch(status)
        {
            case "running_left":
                GameLogic.Singleton.matchplayers[pid].GetComponent<SpriteRenderer>().flipX = true;
                amt.SetBool("Idle", false);
                amt.SetInteger("AnimState", 1);
                break;
            case "running_right":
                GameLogic.Singleton.matchplayers[pid].GetComponent<SpriteRenderer>().flipX = false;
                amt.SetBool("Idle", false);
                amt.SetInteger("AnimState", 1);
                break;
            case "jumping":
                amt.SetTrigger("Jump");
                amt.SetFloat("AirSpeedY", 1f);
                amt.SetBool("Idle", false);
                break;
            case "rolling":
                amt.SetTrigger("Roll");
                amt.SetBool("Idle", false);
                break;
            case "sliding":
                amt.SetTrigger("Slide");
                amt.SetBool("Idle", false);
                break;
            case "idle":
                amt.SetBool("Idle", true);
                amt.SetInteger("AnimState", 0);
                amt.SetFloat("AirSpeedY", 0f);
                break;
            case "capacity":
                amt.SetTrigger("Capacity");
                break;
            case "falling":
                amt.SetFloat("AirSpeedY", -1f);
                amt.SetBool("Idle", false);
                break;
            default:
                amt.SetBool("Idle", true);
                break;
        }

        Vector2 lpos = GameLogic.Singleton.matchplayers[pid].gameObject.transform.position;

        //Debug.Log("position update : " + lpos.x.ToString() + " > " + ppos.x.ToString());

        GameLogic.Singleton.matchplayers[pid].gameObject.transform.position = ppos;
    }

    [MessageHandler((ushort)ServerToClient.itemused)] // server response after item use
    private static void ItemUseCallback(Message msg)
    {
        string item = msg.GetString();
        bool succ = msg.GetBool();
        int rem = msg.GetInt();
        int tch = msg.GetInt();

        // maybe apply effects such as adrenaline with particle so that the player know he's still under effect

        if (succ)
        {
            Debug.Log(item + " use response, touched " + tch.ToString() + ", "+rem.ToString()+" remaining");
        } else
        {
            Debug.Log(item + " use failed");
        }
        

        playersitems[item]=rem;

        loaditems();
    }

    // effects client sides are just visual indicator, the real effect is being applied server sides

    [MessageHandler((ushort)ServerToClient.effect)] // in the radius of someone's capacity
    private static void affected(Message msg)
    {
        string eid = msg.GetString();
        int edur = msg.GetInt();

        Debug.Log("You have been touched by \""+eid+"\" for "+edur.ToString()+"s");

        GameLogic.Singleton.localplayer.effects.Add(new Effect(eid, edur));
    }
}
