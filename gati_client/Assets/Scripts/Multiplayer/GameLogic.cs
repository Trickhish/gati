using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RiptideNetworking;
using System.Linq;
using System.IO;
using UnityEngine.UI;
using System;

public class GameLogic : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] public GameObject gati_prefab;
    [SerializeField] public GameObject drije_prefab;
    
    [Header("Prefabs")]
    [SerializeField] public GameObject local_prefab;
    [SerializeField] public GameObject finishflag;
    [SerializeField] public GameObject web_prefab;

    [Header("Maps")]
    [SerializeField] public List<GameObject> maps;

    [Header("Misc")]
    [SerializeField] public TMP_Text gameidtext;

    [Header("Sounds")]
    [SerializeField] public AudioSource SoundEffector;
    [SerializeField] public List<AudioClip> sounds;

    

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
    public static List<EffectBlock> EffectBlocks = new List<EffectBlock>() { };
    public int capacity;
    public string id="";
    public int pcount;
    public string status;
    public static int stct = -2;
    public float t = -1;
    public float maxpos;
    public Vector3 startpos = new Vector3(0,0,0);
    public Vector3 endpos = new Vector3(0,0,0);
    public static int choosenitem=0;
    public static int mapid;
    public static int alwin = 0;
    public static string NewStats = "";
    public static int local_level;
    public static int local_wcount;
    public static int local_mcount;
    public static int local_exp;
    public static Dictionary<string, int> playersitems = new Dictionary<string, int>(){ // #additem
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
                string config = "Left:276\nRight:275\nJump:273\nSneak:274\nCapacity:101\nPrevious Object:108\nNext Object:109\nUse Item:114\nPlayers List:9\nEscape Menu:27\nBomb:257\nWeb:258\nLightning:259\nFlash:260\nSyringe:261\nShield:262\nFeather:263\nBoots:264\nCape:265";
                
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
        if (Time.realtimeSinceStartup - t >= 1 && stct >= -1)
        {
            t = Time.realtimeSinceStartup;
            if (stct == 0)
            {
                localplayer.GetComponent<Movement>().enabled = true;
                localplayer.canuseobjects = true;
                t = Time.realtimeSinceStartup+0.5f;
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
        if (Input.GetKeyDown((KeyCode)Movement.keys["Bomb"]))
        {
            selectitem("Bomb");
        } else if (Input.GetKeyDown((KeyCode)Movement.keys["Web"]))
        {
            selectitem("Web");
        }
        else if (Input.GetKeyDown((KeyCode)Movement.keys["Lightning"]))
        {
            selectitem("Lightning");
        }
        else if (Input.GetKeyDown((KeyCode)Movement.keys["Flash"]))
        {
            selectitem("Flash");
        }
        else if (Input.GetKeyDown((KeyCode)Movement.keys["Syringe"]))
        {
            selectitem("Syringe");
        }
        else if (Input.GetKeyDown((KeyCode)Movement.keys["Shield"]))
        {
            selectitem("Shield");
        }
        else if (Input.GetKeyDown((KeyCode)Movement.keys["Feather"]))
        {
            selectitem("Feather");
        }
        else if (Input.GetKeyDown((KeyCode)Movement.keys["Boots"]))
        {
            selectitem("Boots");
        }
        else if (Input.GetKeyDown((KeyCode)Movement.keys["Cape"]))
        {
            selectitem("Cape");
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
        if (Input.GetKeyDown((KeyCode)Movement.keys["Use Item"]) && choosenitem>=0 && choosenitem<UIManager.Singleton.item_bar.transform.childCount-1 && playersitems[UIManager.Singleton.item_bar.transform.GetChild(choosenitem+1).name.ToLower().Replace("lightning", "lightningbolt").Replace("syringe", "adrenaline")] > 0)
        {
            if (!GameLogic.Singleton.localplayer.canuseobjects) { return; }
            //if ((Time.realtimeSinceStartup*1000)-GameLogic.Singleton.localplayer.lastobjectuse < 3000f) { return; }

            Debug.Log("using item "+ UIManager.Singleton.item_bar.transform.GetChild(choosenitem + 1).name.ToLower().Replace("lightning", "lightningbolt").Replace("syringe", "adrenaline"));

            Message msg = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.useitem);
            msg.AddString(UIManager.Singleton.item_bar.transform.GetChild(choosenitem + 1).name.ToLower().Replace("lightning", "lightningbolt").Replace("syringe", "adrenaline"));
            msg.AddVector2(localplayer.transform.position);
            NetworkManager.Singleton.Client.Send(msg);

            //NetworkManager.Singleton.rlitems();
            loaditems();
        }
    }

    public static void Reset()
    {
        GameLogic.alwin = 0;
        foreach(EffectBlock eb in GameLogic.EffectBlocks)
        {
            Destroy(eb.gameObject);
        }
        GameLogic.EffectBlocks.Clear();
        GameLogic.Singleton.id = "";
        GameLogic.Singleton.matchplayers.Clear();
        GameLogic.Singleton.pcount = 0;
        GameLogic.Singleton.status = "none";
    }

    public static void PlaySound(string sn)
    {
        int n;
        switch(sn)
        {
            case "countdown":
                n = 0;
                break;
            case "bomb":
                n = 1;
                break;
            case "cape":
                n = 2;
                break;
            case "shield":
                n = 9;
                break;
            case "success":
                n = 6;
                break;
            case "failed":
                n = 5;
                break;
            case "flash":
                n = 3;
                break;
            case "flash2":
                n = 4;
                break;
            case "eclair":
                n = 7;
                break;
            case "seringue":
                n = 8;
                break;
            case "slowed":
                n = 10;
                break;
            case "speed":
                n = 11;
                break;
            default:
                n = -1;
                break;
        }
        if (n>=0)
        {
            GameLogic.Singleton.SoundEffector.clip = GameLogic.Singleton.sounds[n];
            GameLogic.Singleton.SoundEffector.Play();
        }
    }

    public static int NexpOfLvl(int lvl)
    {
        int nexp = 0;
        float inc = 0.2f;
        for (int lv = 0; lv <=lvl; lv++) {
            if (lv == 0) {
                nexp = 1;
            } else if (lv == 1) {
                nexp = 3;
            } else {
                nexp = Mathf.RoundToInt(inc+nexp);
                inc += 0.1f;
            }
        }
        return (nexp);
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
            GameObject cgo = UIManager.Singleton.item_bar.transform.GetChild(i+1).gameObject;

            if (cgo.name == name)
            {
                if (playersitems[UIManager.Singleton.item_bar.transform.GetChild(i + 1).name.ToLower().Replace("lightning", "lightningbolt").Replace("syringe", "adrenaline")] > 0)
                {
                    choosenitem = i;
                    UIManager.Singleton.item_outline.transform.position = cgo.transform.position;
                }
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
        while (i<UIManager.Singleton.item_bar.transform.childCount-1 && playersitems[UIManager.Singleton.item_bar.transform.GetChild(i + 1).name.ToLower().Replace("lightning", "lightningbolt").Replace("syringe", "adrenaline")] <= 0)
        {
            i++;
        }

        if (i < UIManager.Singleton.item_bar.transform.childCount - 1 && playersitems[UIManager.Singleton.item_bar.transform.GetChild(i+1).name.ToLower().Replace("lightning", "lightningbolt").Replace("syringe", "adrenaline")] > 0)
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
        while (i>=0 && playersitems[UIManager.Singleton.item_bar.transform.GetChild(i+1).name.ToLower().Replace("lightning", "lightningbolt").Replace("syringe", "adrenaline")] <= 0)
        {
            i--;
        }

        if (i>=0 && playersitems[UIManager.Singleton.item_bar.transform.GetChild(i + 1).name.ToLower().Replace("lightning", "lightningbolt").Replace("syringe", "adrenaline")] > 0)
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
                    cgo.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = ((KeyCode)Movement.keys[cgo.name.ToLower().Replace("lightning", "lightningbolt").Replace("syringe", "adrenaline")]).ToString(); // key
                } else
                {
                    cgo.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = ""; // key
                }
                TMP_Text tt = cgo.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();

                tt.text = playersitems[cgo.name.ToLower().Replace("lightning", "lightningbolt").Replace("syringe", "adrenaline")].ToString();  // stock
            }
        }
    }

    public static void applykeys()
    {
        for (int i = 0; i < UIManager.Singleton.keysgroup.transform.childCount; i++)
        {
            GameObject ko = UIManager.Singleton.keysgroup.transform.GetChild(i).gameObject;

            string nm = ((KeyCode)Movement.keys[ko.name.Replace("lightning", "lightningbolt").Replace("syringe", "adrenaline")]).ToString().ToUpper();
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
            string key = l.Split(":")[0].Replace("lightning", "lightningbolt").Replace("syringe", "adrenaline");
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

    public static Sprite illuofcara(string cara) // #addcara
    {
        switch (cara.ToLower())
        {
            case "gati":
                return(UIManager.Singleton.gati_illu2);
            case "drije":
                return (UIManager.Singleton.drije_illu);
            default:
                return (UIManager.Singleton.gati_illu2);
        }
    }

    [MessageHandler((ushort)ServerToClient.matchend)] // a player finished the race
    private static void endofmatch(Message message)
    {
        ushort winid = message.GetUShort();

        //Debug.Log(winid.ToString() + ", " + GameLogic.Singleton.localplayer.Id.ToString()+", "+NetworkManager.Singleton.Client.Id.ToString());

        int ftm = message.GetInt();
        string winname = (winid == GameLogic.Singleton.localplayer.Id) ? GameLogic.Singleton.localplayer.username : GameLogic.Singleton.matchplayers[winid].username;

        GameObject rui = UIManager.Singleton.rankUI;
        int wnb = GameLogic.alwin;
        string mn = (ftm / 60).ToString();
        string tm = (mn.Length==1 ? "0"+mn : mn)+":"+(ftm%60).ToString();

        if (wnb==0)
        {
            rui.transform.Find("ranks").GetComponent<TMP_Text>().text = "1 : " + winname + " (" + tm + ")";
            rui.transform.Find("nick1").GetComponent<TMP_Text>().text = winname;
            rui.transform.Find("nick2").GetComponent<TMP_Text>().text = "";
            rui.transform.Find("nick3").GetComponent<TMP_Text>().text = "";

            rui.transform.Find("illu1").GetComponent<Image>().sprite = illuofcara(GameLogic.Singleton.matchplayers[winid].cara);

            rui.transform.Find("time1").GetComponent<TMP_Text>().text = tm;
            rui.transform.Find("time2").GetComponent<TMP_Text>().text = "";
            rui.transform.Find("time3").GetComponent<TMP_Text>().text = "";

            rui.transform.Find("illu1").gameObject.SetActive(true);
            rui.transform.Find("illu2").gameObject.SetActive(false);
            rui.transform.Find("illu3").gameObject.SetActive(false);
        } else
        {
            rui.transform.Find("ranks").GetComponent<TMP_Text>().text = (wnb+1).ToString()+" : " + winname + " (" + tm + ")";

            if (wnb==1)
            {
                rui.transform.Find("illu2").GetComponent<Image>().sprite = illuofcara(GameLogic.Singleton.matchplayers[winid].cara);
                rui.transform.Find("illu2").gameObject.SetActive(true);
                rui.transform.Find("nick2").GetComponent<TMP_Text>().text = winname;
                rui.transform.Find("time2").GetComponent<TMP_Text>().text = tm;
            } else if (wnb==2)
            {
                rui.transform.Find("illu3").GetComponent<Image>().sprite = illuofcara(GameLogic.Singleton.matchplayers[winid].cara);
                rui.transform.Find("illu3").gameObject.SetActive(true);
                rui.transform.Find("nick3").GetComponent<TMP_Text>().text = winname;
                rui.transform.Find("time3").GetComponent<TMP_Text>().text = tm;
            }
        }

        if (winid == NetworkManager.Singleton.Client.Id) // you finished the map
        {
            string s = message.GetString();
            NewStats = s == "" ? "" : s.Split("|")[0];
            string[] l = s.Split("|")[1].Split(",");

            UIManager.Singleton.rankUI.transform.Find("coins").GetComponent<TMP_Text>().text = "+ "+l[1];
            UIManager.Singleton.rankUI.transform.Find("exp").GetComponent<TMP_Text>().text = "+ "+l[0]+" EXP";

            GameLogic.Singleton.localplayer.ClearEffects();
            GameLogic.Singleton.localplayer.GetComponent<Movement>().enabled = false;
            GameLogic.Singleton.localplayer.GetComponent<Animator>().SetBool("Idle", true);
            GameLogic.Singleton.localplayer.GetComponent<Animator>().SetInteger("AnimState", 0);
            GameLogic.Singleton.localplayer.GetComponent<Animator>().SetTrigger("Roll");
            GameLogic.Singleton.localplayer.canuseobjects = false;

            GameLogic.Singleton.finishflag.GetComponentInChildren<ParticleSystem>().Play();
            if (GameLogic.alwin > 0)
            {
                Debug.Log("finished");
            } else
            {
                Debug.Log("won");
            }

            if (wnb == 0)
            {
                rui.transform.Find("msg").GetComponent<TMP_Text>().text = "YOU RANKED FIRST";
            }
            else if (wnb == 1)
            {
                rui.transform.Find("msg").GetComponent<TMP_Text>().text = "YOU RANKED SECOND";
            }
            else if (wnb == 2)
            {
                rui.transform.Find("msg").GetComponent<TMP_Text>().text = "YOU RANKED THIRD";
            }
            else
            {
                rui.transform.Find("msg").GetComponent<TMP_Text>().text = "YOU RANKED " + (wnb + 1).ToString() + "th ("+tm+")";
            }

            UIManager.Singleton.item_bar.SetActive(false);
            UIManager.Singleton.pgr_slider.SetActive(false);
            rui.SetActive(true);
        }
        GameLogic.Singleton.status = "finishing";
        GameLogic.alwin++;
    }
    [MessageHandler((ushort)ServerToClient.launch)]
    private static void launchmatch(Message message)
    {
        NetworkManager.Singleton.rlitems();
        loaditems();
        selectitem(getfirstitem());

        GameLogic.Singleton.maxpos = Vector3.Distance(GameLogic.Singleton.startpos, GameLogic.Singleton.endpos);
        GameLogic.Singleton.status = "started";
        GameLogic.alwin = 0;

        Debug.Log("Match Starting");

        for (int i=0; i<GameLogic.Singleton.maps.Count; i++)
        {
            if (i==GameLogic.mapid)
            {
                GameLogic.Singleton.maps[i].SetActive(true);
            } else
            {
                GameLogic.Singleton.maps[i].SetActive(false);
            }
        }
        
        

        GameLogic.Singleton.finishflag.transform.position = new Vector3(GameLogic.Singleton.endpos.x+5, GameLogic.Singleton.endpos.y, 0f);
        GameLogic.Singleton.finishflag.GetComponentInChildren<ParticleSystem>().Stop();

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

        GameLogic.Singleton.localplayer.canuseobjects = false;

        TMP_Text pltext = GameLogic.Singleton.localplayer.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>();
        pltext.color = new Color(174f/255f, 45f/255f, 93f/255f);

        stct = 5;
        GameLogic.PlaySound("countdown");
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

    [MessageHandler((ushort)ServerToClient.itemused)] // server response after item/capacity use
    private static void ItemUseCallback(Message msg)
    {
        string item = msg.GetString();
        bool succ = msg.GetBool();
        int rem = msg.GetInt();
        int tch = msg.GetInt();

        // maybe apply effects such as adrenaline with particle so that the player know he's still under effect

        if (item=="capacity")
        {
            if (succ)
            {
                Debug.Log("Capacity Use Response");

                switch (GameLogic.Singleton.localplayer.cara)
                {
                    case "gati":
                        GameLogic.Singleton.localplayer.effects.Add(new Effect("dash", 500));
                        break;
                    case "drije":
                        break;
                    default:
                        break;
                }
            } else
            {
                Debug.Log("Capacity on cooldown");
            }

            return;
        }

        if (succ)
        {
            if (tch==-1)
            {
                Debug.Log(item + " use response, " + rem.ToString() + " remaining");
                bool fnd = false;
                switch (item)
                {
                    case "feather":
                        fnd = false;
                        foreach (Effect ef in GameLogic.Singleton.localplayer.effects)
                        {
                            if (ef.Name == "agility")
                            {
                                ef.Duration = 2000;
                                ef.StartTime = Time.realtimeSinceStartup;
                                fnd = true;
                                break;
                            }
                        }
                        if (!fnd)
                        {
                            GameLogic.PlaySound("flash2");
                            GameLogic.Singleton.localplayer.effects.Add(new Effect("agility", 2000));
                        }
                        break;
                    case "boots":
                        fnd = false;
                        foreach (Effect ef in GameLogic.Singleton.localplayer.effects)
                        {
                            if (ef.Name == "speed")
                            {
                                ef.Duration = 2000;
                                ef.StartTime = Time.realtimeSinceStartup;
                                fnd = true;
                                break;
                            }
                        }
                        if (!fnd)
                        {
                            GameLogic.PlaySound("speed");
                            GameLogic.Singleton.localplayer.effects.Add(new Effect("speed", 2000));
                        }
                        break;
                    case "cape":
                        fnd = false;
                        foreach (Effect ef in GameLogic.Singleton.localplayer.effects)
                        {
                            if (ef.Name == "invisibility")
                            {
                                ef.Duration = 2000;
                                ef.StartTime = Time.realtimeSinceStartup;
                                fnd = true;
                                break;
                            }
                        }
                        if (!fnd)
                        {
                            GameLogic.PlaySound("cape");
                            GameLogic.Singleton.localplayer.effects.Add(new Effect("invisibility", 2000));
                        }
                        break;
                    case "adrenaline":
                        GameLogic.PlaySound("seringue");
                        break;
                    case "shield":
                        GameLogic.PlaySound("shield");
                        break;
                    default:
                        GameLogic.PlaySound("success");
                        break;
                }
            } else
            {
                GameLogic.PlaySound("success");
                Debug.Log(item + " use response, touched " + tch.ToString() + ", " + rem.ToString() + " remaining");
            }
        } else
        {
            GameLogic.PlaySound("failed");
            Debug.Log(item + " use failed");
        }


        if (succ) {
            playersitems[item] = rem;
            loaditems();
        }
    }

    // effects client sides are just visual indicator, the real effect is being applied server sides

    [MessageHandler((ushort)ServerToClient.effect)] // in the radius of someone's capacity
    private static void affected(Message msg)
    {
        string eid = msg.GetString();
        int edur = msg.GetInt();

        Debug.Log("You have been touched by \""+eid+"\" for "+edur.ToString()+"s");

        bool fnd = false;
        foreach(Effect ef in GameLogic.Singleton.localplayer.effects)
        {
            if (ef.Name == eid)
            {
                ef.Duration = edur;
                ef.StartTime = Time.realtimeSinceStartup;
                fnd = true;
                break;
            }
        }
        if (!fnd)
        {
            GameLogic.Singleton.localplayer.effects.Add(new Effect(eid, edur));
        }
    }

    [MessageHandler((ushort)ServerToClient.effectblock)]
    private static void effectblock_change(Message msg)
    {
        bool status = msg.GetBool();
        Vector2 bpos = msg.GetVector2();
        string blockname = msg.GetString();

        if (status)
        {
            switch (blockname)
            {
                case "web":
                    EffectBlock eb = Instantiate(GameLogic.Singleton.web_prefab, new Vector3(bpos.x, bpos.y, 0), Quaternion.identity).GetComponent<EffectBlock>();
                    break;
                default:
                    break;
            }
        } else
        {
            foreach(EffectBlock eb in GameLogic.EffectBlocks)
            {
                if (eb.BlockName==blockname && (Vector3) bpos==eb.transform.position)
                {
                    GameLogic.EffectBlocks.Remove(eb);
                    break;
                }
            }
        }
    }
}
