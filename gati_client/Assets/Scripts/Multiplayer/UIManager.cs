using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Security.Cryptography;
using System;
using System.Text;
using UnityEngine.Networking;
using System.Net;
using System.IO;
using System.Web;
using System.Linq;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class UIManager : MonoBehaviour
{
    private static UIManager _singleton;

    public static UIManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(UIManager)}");
                Destroy(value);
            }
        }
    }

    public static string localusername;
    public static List<gradient> gradients = new List<gradient>() {};
    public static float fstp = 0;
    public static int training_map = 0;

    [Header("UI panels")]
    [SerializeField] public GameObject connectUI;
    [SerializeField] public GameObject menuUI;
    [SerializeField] public GameObject escUI;
    [SerializeField] public GameObject tabUI;
    [SerializeField] public GameObject waitUI;
    [SerializeField] public GameObject shopUI;
    [SerializeField] public GameObject userUI;

    [Header("UI subpanels")]
    [SerializeField] public GameObject privatematchUI;
    [SerializeField] public GameObject login_form;
    [SerializeField] public GameObject register_form;
    [SerializeField] public GameObject choose_auth;
    [SerializeField] public GameObject start_ui;
    [SerializeField] public GameObject trainingUI;
    [SerializeField] public GameObject endtrainingUI;

    [Header("Caracter Choice")]
    [SerializeField] public Animator car_illu_anim;
    [SerializeField] public Image car_illu;
    [SerializeField] public TMP_Text car_pres;
    [SerializeField] public Image car_stats;
    [SerializeField] public TMP_Text car_name;
    [SerializeField] public Button prev_car;
    [SerializeField] public Button next_car;
    [SerializeField] public Sprite gati_illu;
    [SerializeField] public Sprite gati_illu2;
    [SerializeField] public Sprite gati_stats;
    [SerializeField] public String gati_pres;
    [SerializeField] public Sprite drije_illu;
    [SerializeField] public Sprite drije_stats;
    [SerializeField] public String drije_pres;

    [Header("Authentification")]
    [SerializeField] public GameObject authbuttons;
    [SerializeField] public TMP_Text statustext;
    [SerializeField] public GameObject auth_backbt;
    [SerializeField] public Toggle remember_me;
    [SerializeField] public Button enter_login;
    [SerializeField] public Button enter_register;
    [SerializeField] public Button enter_guest;
    [SerializeField] public TMP_InputField login_email;
    [SerializeField] public gradient login_email_gradient;
    [SerializeField] public TMP_InputField login_password;
    [SerializeField] public gradient login_password_gradient;
    [SerializeField] public TMP_InputField register_username;
    [SerializeField] public gradient register_username_gradient;
    [SerializeField] public TMP_InputField register_email;
    [SerializeField] public gradient register_email_gradient;
    [SerializeField] public TMP_InputField register_password;
    [SerializeField] public gradient register_password_gradient;

    [Header("Shop UI")]
    [SerializeField] public TMP_Text shop_money;
    [SerializeField] public TMP_Text shop_itemdesc;

    [Header("User UI")]
    [SerializeField] public TMP_Text user_name;
    [SerializeField] public TMP_Text user_stats;

    [Header("Network")]
    [SerializeField] public InputField ipfield;
    [SerializeField] public NetworkManager nwm;
    [SerializeField] public GameObject serverstatus;

    [Header("Match UI")]
    [SerializeField] public Button public_match;
    [SerializeField] public Button private_match;
    [SerializeField] public GameObject rankUI;
    [SerializeField] public GameObject flashobj;

    [Header("Wait UI")]
    [SerializeField] public TMP_Text wait_match_id;
    [SerializeField] public GameObject wait_player_prefab;
    [SerializeField] public GameObject wait_players;
    [SerializeField] public TMP_Text wait_pln;

    [Header("Private Match Creation")]
    [SerializeField] public TMP_Dropdown private_plc;
    [SerializeField] public TMP_Dropdown private_map;
    [SerializeField] public TMP_InputField private_mid;
    [SerializeField] public gradient private_mid_gradient;

    [Header("Universal Canvas")]
    [SerializeField] public GameObject pgr_slider;
    [SerializeField] public TMP_Text stcounter;
    [SerializeField] public GameObject levelupUI;

    [Header("Audio")]
    [SerializeField] public AudioSource clickaudio;
    [SerializeField] public AudioSource hoveraudio;

    [Header("Items")]
    [SerializeField] public GameObject item_bar;
    [SerializeField] public GameObject item_outline;

    [Header("Misc")]
    [SerializeField] public Button connectbt;
    [SerializeField] public TMP_Text usertext_lo;
    [SerializeField] public GameObject keysgroup;

    EventSystem system;
    public static GameObject waitforkey = null;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        system = EventSystem.current;

        connectUI.SetActive(true);
        menuUI.SetActive(false);
        escUI.SetActive(false);
        tabUI.SetActive(false);
        waitUI.SetActive(false);
        pgr_slider.SetActive(false);
        item_bar.SetActive(false);
        serverstatus.SetActive(true);
        shopUI.SetActive(false);

        //UIManager.Singleton.connectbt.interactable = false;
        UIManager.Singleton.enter_login.interactable = false;
        UIManager.Singleton.enter_register.interactable = false;

        GameLogic.Singleton.gameidtext.text = "";

        foreach (GameObject s in GameLogic.Singleton.maps)
        {
            s.SetActive(false);
        }

        GameLogic.Singleton.id = "";
    }

    private void Update()
    {
        if (fstp!=0 && Time.realtimeSinceStartup>=fstp)
        {
            flashobj.SetActive(false);
            fstp = 0;
        }

        if (Input.anyKeyDown)
        {
            if (waitforkey != null)
            {
                List<int> aud = new List<int>();
                foreach(string k in Movement.keys.Keys)
                {
                    if (k!=waitforkey.name)
                    {
                        aud.Add(Movement.keys[k]);
                    }
                }

                foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKey(kcode) && !new List<int>(){323,324}.Contains((int)kcode) && !aud.Contains((int)kcode))
                    {
                        waitforkey.GetComponent<TMP_Text>().color = new Color(221f/255f, 221f/255f, 221f/255f);
                        //waitforkey.GetComponent<TMP_Text>().text = waitforkey.name+" : "+kcode.ToString();

                        Movement.keys[waitforkey.name] = (int) kcode;
                        GameLogic.applykeys();
                        waitforkey = null;
                        break;
                    }
                }
            }

            /*
            if (login_form.activeSelf)
            {
                login_email_gradient.ApplyGradient();
                login_password_gradient.ApplyGradient();
            } if (register_form.activeSelf)
            {
                register_email_gradient.ApplyGradient();
                register_username_gradient.ApplyGradient();
                register_password_gradient.ApplyGradient();
            } if (privatematchUI.activeSelf)
            {
                private_mid_gradient.ApplyGradient();
            }
            */
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

            if (next != null)
            {

                TMP_InputField inputfield = next.GetComponent<TMP_InputField>();
                if (inputfield != null)
                    inputfield.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret

                system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
            }
            //else Debug.Log("next nagivation element not found");

        } else if (Input.GetKeyDown(KeyCode.Return))
        {
            
            if (privatematchUI.activeSelf)
            {
                if (private_mid.text == "")
                {
                    creatematch_clicked();
                } else
                {
                    joinprivate_clicked();
                }
            } else if (login_form.activeSelf)
            {
                login_clicked();
            } else if (register_form.activeSelf)
            {
                register_clicked();
            }
        }
    }

    public void training_clicked()
    {
        trainingUI.SetActive(true);
    }

    public void training_map_select(string map)
    {
        switch(map)
        {
            case "etril_sewer":
                training_map = 0;
                break;
            case "maya_temple":
                training_map = 1;
                break;
            default:
                training_map = 0;
                break;
        }
        trainingUI.transform.Find("select").transform.position = trainingUI.transform.Find(map).transform.position;
    }

    public void training_play(int level)
    {
        GameLogic.Reset();

        switch (level)
        {
            case 0:
                GameLogic.FantomData = GameLogic.fantoms[training_map].Item1.Split("|");
                break;
            case 1:
                GameLogic.FantomData = GameLogic.fantoms[training_map].Item2.Split("|");
                break;
            case 2:
                GameLogic.FantomData = GameLogic.fantoms[training_map].Item3.Split("|");
                break;
            case 3:
                break;
            default:
                GameLogic.FantomData = GameLogic.fantoms[training_map].Item1.Split("|");
                break;
        }
        GameLogic.FantomProgress = 0;

        switch(training_map)
        {
            case 0:
                GameLogic.Singleton.startpos = new Vector3(-192f, 0.77f, 0f);
                GameLogic.Singleton.endpos = new Vector3(2f, -3.5f, 0f);
                break;
            case 1:
                GameLogic.Singleton.startpos = new Vector3(-192f, 0.77f, 0f);
                GameLogic.Singleton.endpos = new Vector3(201f, -3.5f, 0f);
                break;
            default:
                GameLogic.Singleton.startpos = new Vector3(-192f, 0.77f, 0f);
                GameLogic.Singleton.endpos = new Vector3(2f, -3.5f, 0f);
                break;
        }

        // start -192, 0.77

        GameLogic.Singleton.id = "training";
        GameLogic.mapid = training_map;
        GameLogic.Singleton.finishflag.GetComponentInChildren<ParticleSystem>().Stop();

        UIManager.Singleton.pgr_slider.SetActive(true);
        UIManager.Singleton.item_bar.SetActive(false);

        UIManager.Singleton.connectUI.SetActive(false);
        UIManager.Singleton.menuUI.SetActive(false);
        UIManager.Singleton.waitUI.SetActive(false);
        UIManager.Singleton.trainingUI.SetActive(false);

        cam.trans.position = new Vector3(-4.2f, 3.7f, 0f);

        for (int i = 0; i < GameLogic.Singleton.maps.Count; i++)
        {
            if (i == training_map)
            {
                GameLogic.Singleton.maps[i].SetActive(true);
            }
            else
            {
                GameLogic.Singleton.maps[i].SetActive(false);
            }
        }

        GameObject fp = Instantiate(GameLogic.prefabofcara(Player.localcara), new Vector3(-192, 0, 0), Quaternion.identity);
        GameLogic.fantom = fp.GetComponent<Player>();
        GameLogic.fantom.Id = 1;
        GameLogic.fantom.IsLocal = false;
        GameLogic.fantom.username = "";
        GameLogic.fantom.GetComponent<Rigidbody2D>().simulated = false;
        GameLogic.fantom.GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, 0.784f);
        GameLogic.fantom.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = "";
        GameLogic.fantom.transform.position = GameLogic.Singleton.startpos;
        fp.SetActive(true);

        GameObject lp = Instantiate(GameLogic.prefabofcara(Player.localcara), new Vector3(-192, 0, 0), Quaternion.identity);
        Player rpl = lp.GetComponent<Player>();
        rpl.Id = 0;
        rpl.IsLocal = true;
        rpl.username = (Player.mail!="" ? localusername : "");
        rpl.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = rpl.username;
        rpl.canuseobjects = false;
        rpl.canulti = false;
        rpl.transform.position = GameLogic.Singleton.startpos;
        lp.SetActive(true);

        GameLogic.Singleton.matchplayers.Clear();
        GameLogic.Singleton.matchplayers.Add(1, GameLogic.fantom);
        GameLogic.Singleton.matchplayers.Add(0, rpl);

        GameLogic.Singleton.localplayer.canuseobjects = false;
        GameLogic.Singleton.localplayer.canulti = false;

        GameLogic.Singleton.finishflag.transform.position = GameLogic.Singleton.endpos+new Vector3(5f, 0f, 0f);
        GameLogic.Singleton.maxpos = Vector3.Distance(GameLogic.Singleton.startpos, GameLogic.Singleton.endpos);

        GameLogic.starttime = Time.realtimeSinceStartup;

        UIManager.Singleton.start_ui.SetActive(true);
        GameLogic.stct = 5;
        GameLogic.PlaySound("countdown");
    }

    public static void flash(float ms=100f)
    {
        UIManager.Singleton.flashobj.SetActive(true);
        fstp = Time.realtimeSinceStartup + (ms / 1000f);
    }

    public void setkey(GameObject kgo)
    {
        if (waitforkey != null)
        {
            //waitforkey.GetComponent<TMP_Text>().text = waitforkey.name + " : " + ((KeyCode) Movement.keys[waitforkey.name]).ToString();
            waitforkey.GetComponent<TMP_Text>().color = new Color(221f / 255f, 221f / 255f, 221f / 255f);
            GameLogic.applykeys();
        }

        TMP_Text kt = kgo.GetComponent<TMP_Text>();
        string key = kgo.name;
        kt.text = key+" : ";
        kt.color = new Color(104f/255f, 32f/255f, 32f/255f);

        waitforkey = kgo;
    }

    public void load_car_select()
    {
        car_name.text = Char.ToUpper(Player.localcara[0])+Player.localcara.Substring(1);

        if (Player.localcara=="gati")
        {
            prev_car.interactable = false;
            next_car.interactable = true;

            car_illu_anim.SetInteger("cara", 0);
            car_illu.sprite = gati_illu;
            car_pres.text = gati_pres;
            car_stats.sprite = gati_stats;
        } else
        {
            prev_car.interactable = true;
            next_car.interactable = false;

            car_illu_anim.SetInteger("cara", 1);
            car_illu.sprite = drije_illu;
            car_pres.text = drije_pres;
            car_stats.sprite = drije_stats;
        }
    }

    public void prev_car_clicked()
    {
        if (Player.localcara=="gati")
        {
            Player.localcara = "drije";
        } else
        {
            Player.localcara = "gati";
        }
        load_car_select();
    }

    public void next_car_clicked()
    {
        if (Player.localcara == "gati")
        {
            Player.localcara = "drije";
        }
        else
        {
            Player.localcara = "gati";
        }
        load_car_select();
    }

    public void profile_clicked()
    {
        List<int> sts = NetworkManager.Singleton.getstats();

        decimal dd = sts[3];
        decimal dv = sts[2];
        int pct = (int)Decimal.Round(Decimal.Divide(dd, dv) * 100);

        string s = "Level : " + sts[0] + "\n\nMatch Count : " + sts[2] + "\n\nWin Percentage : " + pct + "%\nWin : " + sts[3] + "\nLose: " + (sts[2] - sts[3]) + "";

        UIManager.Singleton.user_stats.text = s;
        UIManager.Singleton.userUI.SetActive(true);
    }

    public void website_clicked()
    {
        Application.OpenURL("http://gati.games");
    }

    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

    public void leavematch()
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.leavematch);
        NetworkManager.Singleton.Client.Send(message);

        GameLogic.Singleton.capacity = 0;
        GameLogic.Singleton.pcount = 0;

        escUI.SetActive(false);
        pgr_slider.SetActive(false);
        item_bar.SetActive(false);
        privatematchUI.SetActive(false);
        tabUI.SetActive(false);
        waitUI.SetActive(false);
        start_ui.SetActive(false);
        GameLogic.stct = -2;
        GameLogic.Singleton.SoundEffector.Stop();

        if (GameLogic.Singleton.id == "test")
        {
            menuUI.SetActive(false);
            connectUI.SetActive(true);
        } else
        {
            menuUI.SetActive(true);
            connectUI.SetActive(false);
        }
        GameLogic.Singleton.id = "";

        foreach (Player p in GameLogic.Singleton.matchplayers.Values)
        {
            Destroy(p.gameObject);
        }
        GameLogic.Reset();
        GameLogic.Singleton.matchplayers.Clear();
    }

    public void ranking_continue()
    {
        leavematch();

        if (GameLogic.NewStats!="")
        {
            string[] l = GameLogic.NewStats.Split(',');

            int level = int.Parse(l[0]);
            int exp = int.Parse(l[1]);
            int mcount = int.Parse(l[2]);
            int wcount = int.Parse(l[3]);
            int wpct = (int)Decimal.Round(Decimal.Divide(wcount, mcount) * 100);
            int lvlpct = (int)Decimal.Round(Decimal.Divide(exp, GameLogic.NexpOfLvl(level)) * 100);
            user_stats.transform.Find("progress").GetComponent<Slider>().value = lvlpct;
            user_stats.text = "Level : " + level.ToString() + "\n\nWin Ratio : " + wpct.ToString() + "%";

            if (GameLogic.local_level < level)
            {
                levelupUI.transform.Find("msg").GetComponent<TMP_Text>().text = "Congratulations !\nYou're now level "+level.ToString();
                levelupUI.SetActive(true);
            }

            GameLogic.local_level = level;
            GameLogic.local_wcount = wcount;
            GameLogic.local_mcount = mcount;
            GameLogic.local_exp = exp;
        }
    }

    public void setip()
    {
        NetworkManager.Singleton.ip = ipfield.text;
    }

    public void quit_clicked()
    {
        Application.Quit();
    }

    public void findmatch_clicked()
    {
        Debug.Log("Searching for a match");

        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.findmatch);

        message.AddString(localusername);

        NetworkManager.Singleton.Client.Send(message);

        GameLogic.Singleton.gameidtext.text = "Searching for a match";
    }

    public void creatematch_clicked()
    {
        int pc = int.Parse(private_plc.options[private_plc.value].text.Split(' ')[0]);
        int map = private_map.value;
        Debug.Log("match map : "+map.ToString());

        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.createprivate);
        message.AddInt(pc);
        message.AddInt(map);
        message.AddString(localusername);
        message.AddString(Player.localcara);

        NetworkManager.Singleton.Client.Send(message);
    }

    public void joinprivate_clicked()
    {
        if (private_mid.text != "")
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.joinprivate);
            message.AddString(localusername);
            message.AddString(private_mid.text);
            message.AddString(Player.localcara);
            NetworkManager.Singleton.Client.Send(message);
        }
    }

    public void offlinetest_clicked()
    {
        GameLogic.Reset();

        GameLogic.Singleton.capacity = 1;
        GameLogic.Singleton.pcount = 1;
        GameLogic.Singleton.matchplayers.Clear();

        GameObject pl = Instantiate(GameLogic.prefabofcara(Player.localcara), new Vector3(-192, 0, 0), Quaternion.identity);
        TMP_Text pltext = pl.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>();
        pltext.text = "Guest";

        pl.name = pltext.text;
        pl.SetActive(true);

        Player rpl = pl.GetComponent<Player>();
        rpl.Id = NetworkManager.Singleton.Client.Id;
        rpl.IsLocal = true;
        rpl.username = pltext.text;

        GameLogic.Singleton.matchplayers.Add(NetworkManager.Singleton.Client.Id, rpl);

        // define a way to choose a map

        pgr_slider.SetActive(true);
        item_bar.SetActive(true);

        connectUI.SetActive(false);
        menuUI.SetActive(false);
        waitUI.SetActive(false);

        TMP_Text tab_text = UIManager.Singleton.tabUI.transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
        tab_text.text = "TEST MATCH\n"+pltext.text;

        GameLogic.Singleton.id = "test";

        cam.trans.position = new Vector3(-4.2f, 3.7f, 0f);
    }

    public static string[] hashstring(string input, int ss)
    {
        int SALT_SIZE = ss;
        int HASH_SIZE = 24;
        int ITERATIONS = 100000;

        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        byte[] salt = new byte[SALT_SIZE];
        provider.GetBytes(salt);

        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(input, salt, ITERATIONS);

        return (new string[] { System.Text.Encoding.UTF8.GetString(salt), System.Text.Encoding.UTF8.GetString(pbkdf2.GetBytes(HASH_SIZE)) });
    }

    public static void stc(string token)
    {
        using (StreamWriter sr = new StreamWriter(Application.dataPath + "/stc"))
        {
            sr.Write(token);
        }
    }

    public static void clearstc()
    {
        
        if (File.Exists(Application.dataPath + "/stc"))
        {
            File.Delete(Application.dataPath + "/stc");
        }
    }

    public static string stc_load()
    {
        if (File.Exists(Application.dataPath+"/stc"))
        {
            using (StreamReader sr = new StreamReader(Application.dataPath + "/stc"))
            {
                return(sr.ReadToEnd());
            }
        }
        return("");
    }

    public static bool stc_ver(string token)
    {
        string r = NetworkManager.getreq("https://trickhisch.alwaysdata.net/gati/?a=login&st="+HttpUtility.UrlEncode(token));
        if (r!="error" && r!="unreachable" && r!="false" && r!="" && r!="confirm")
        {
            NetworkManager.Singleton.token = r;
            return(true);
        }
        return(false);
    }

    public void login_initiated()
    {
        if (stc_ver(stc_load()))
        {
            UIManager.Singleton.trainingUI.transform.Find("fantom").GetComponent<Button>().interactable = true;

            statustext.text = "Connected";

            NetworkManager.Singleton.rldt();
            load_car_select();

            usertext_lo.text = localusername;

            Message msg = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.login);
            msg.AddString(localusername);
            msg.AddString(Player.mail);
            NetworkManager.Singleton.Client.Send(msg);

            serverstatus.GetComponent<Image>().enabled = false;
            connectUI.SetActive(false);
            menuUI.SetActive(true);
            NetworkManager.rlstats();

            public_match.GetComponent<gradient>().ApplyGradient();
            private_match.GetComponent<gradient>().ApplyGradient();
        } else
        {
            auth_backbt.gameObject.SetActive(true);
            choose_auth.gameObject.SetActive(false);
            login_form.gameObject.SetActive(true);
        }
    }

    public void login_clicked()
    {
        string mail = login_email.text;
        string pass = login_password.text;

        if ( mail == "" || pass == "")
        {
            statustext.text = "Empty Field";
        }
        else
        {
            string r = NetworkManager.getreq("https://trickhisch.alwaysdata.net/gati/?a=login&m=" + HttpUtility.UrlEncode(mail) + "&p=" + HttpUtility.UrlEncode(pass)+(remember_me.isOn ? "&stc=true" : ""));

            if (r == "false")
            {
                statustext.text = "Invalid Credentials";
            }
            else if (r == "error")
            {
                statustext.text = "Server Error, try again later";
            }
            else if (r == "unreachable")
            {
                statustext.text = "Network Error, try again later";
            }
            else if (r == "confirm")
            {
                statustext.text = "Confirm your email to continue";
            }
            else
            {
                UIManager.Singleton.trainingUI.transform.Find("fantom").GetComponent<Button>().interactable = true;

                // SUCCESS LOGIN
                if (remember_me.isOn)
                {
                    NetworkManager.Singleton.token = r.Split('|')[0];
                    stc(r.Split('|')[1]);
                } else
                {
                    NetworkManager.Singleton.token = r;
                }
                statustext.text = "Connected";

                NetworkManager.Singleton.rldt();
                load_car_select();

                usertext_lo.text = localusername;

                Message msg = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.login);
                msg.AddString(localusername);
                msg.AddString(Player.mail);
                NetworkManager.Singleton.Client.Send(msg);

                serverstatus.GetComponent<Image>().enabled = false;
                connectUI.SetActive(false);
                menuUI.SetActive(true);

                public_match.GetComponent<gradient>().ApplyGradient();
                private_match.GetComponent<gradient>().ApplyGradient();
            }
        }

        /*
        Message m = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.login);
        m.AddString(mail);
        m.AddString(pass);

        NetworkManager.Singleton.Client.Send(m);
        */
    }

    public void register_clicked()
    {
        string username = register_username.text;
        string mail = register_email.text;
        string pass = register_password.text;

        if (username=="" || mail=="" || pass=="")
        {
            statustext.text = "Empty Field";
        } else
        {
            string r = NetworkManager.getreq("https://trickhisch.alwaysdata.net/gati/?a=register&m=" + HttpUtility.UrlEncode(mail) + "&p=" + HttpUtility.UrlEncode(pass) + "&n=" + HttpUtility.UrlEncode(username));

            if (r == "false")
            {
                statustext.text = "Unknown Error, try again later";
            }
            else if (r == "error")
            {
                statustext.text = "Server Error, try again later";
            }
            else if (r == "unreachable")
            {
                statustext.text = "Network Error, try again later";
            }
            else if (r == "mail")
            {
                login_email.text = "";
                login_password.text = "";
                statustext.text = "Email already taken";
            }
            else if (r == "true")
            {
                // SUCCESS REGISTER

                statustext.text = "Confirm your email to continue";

                login_email.text = mail;
                login_password.text = pass;

                register_form.SetActive(true);
                login_form.SetActive(true);
            }
            else
            {
                statustext.text = "Unexpected server response";
            }
        }
        

        /*
        Message m = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.register);
        m.AddString(mail);
        m.AddString(pass);
        m.AddString(salt);

        NetworkManager.Singleton.Client.Send(m);
        */
    }

    public void BackToMain()
    {
        UIManager.Singleton.trainingUI.transform.Find("fantom").GetComponent<Button>().interactable = false;

        escUI.SetActive(false);
        tabUI.SetActive(false);
        login_form.SetActive(false);
        register_form.SetActive(false);
        auth_backbt.SetActive(false);
        authbuttons.SetActive(true);
        pgr_slider.SetActive(false);

        if (NetworkManager.Singleton.Client.IsConnected && Player.mail!="" && NetworkManager.Singleton.token!="")
        {
            menuUI.SetActive(true);
            connectUI.SetActive(false);
        } else
        {
            menuUI.SetActive(false);
            connectUI.SetActive(true);
        }

        foreach(GameObject s in GameLogic.Singleton.maps)
        {
            s.SetActive(false);
        }
        GameLogic.Reset();

        GameLogic.Singleton.id = "";

        statustext.text = "";
        GameLogic.Singleton.gameidtext.text = "";
        serverstatus.GetComponent<Image>().enabled = true;
    }
}
