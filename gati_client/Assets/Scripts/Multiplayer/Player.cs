using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using TMPro;
using System;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; set; }
    public bool IsLocal { get; set; }
    public string username;
    public GameObject listitem;
    public GameObject caraprefab;
    public string cara;
    public bool ismoving { get; set; }
    public bool isjumping { get; set; }
    public bool isGrounded = false;
    public bool canmove = true;
    public bool canuseobjects = false;
    public string status { get; set; }

    public float maxpos;
    public Vector3 lpos = new Vector3(0, 0, 0);
    public List<Effect> effects = new List<Effect>();
    public Collider2D ccld;
    public Movement mov;
    public static string localcara="drije";
    
    [SerializeField] public Transform isGroundedChecker;
    [SerializeField] public float checkGroundRadius;
    [SerializeField] public LayerMask groundLayer;
    [SerializeField] public LayerMask plateformlayer;

    public static int money;
    public static string mail;

    private static Player _singleton;

    public static Player Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(Player)}");
                Destroy(value);
            }
        }
    }
    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public Player(ushort pid, string username, bool islocal)
    {
        this.Id = pid;
        this.username = username;
        this.IsLocal = islocal;
        this.ismoving = false;
        this.isjumping = false;
        this.canmove = true;
        //this.lpos = new Vector3(0,0,0);
        this.cara = "drije";
        this.mov = GetComponent<Movement>();
        this.status = "idle";
        this.effects = new List<Effect>(){new Effect("", 0)};
    }

    public void updatepos() // called when the players is moving. update the position of the player for each other player in the match.
    {
        Message message = Message.Create(MessageSendMode.unreliable, (ushort)ClientToServerId.playerposupdate);
        message.AddString((this.status=="" || this.status==null) ? "idle" : this.status);

        message.AddVector3(this.transform.position);
        NetworkManager.Singleton.Client.Send(message);
    }

    public void UpdateEffects()
    {
        int i = 0;
        while (i < this.effects.Count)
        {
            Effect ef = this.effects[i];
            if (!ef.Update())
            {
                i++;
            }
            if (i >= this.effects.Count)
            {
                break;
            }
        }
    }

    public void ClearEffects()
    {
        for (int i=0; i<this.effects.Count; i++)
        {
            Effect ef = this.effects[0];
            ef.Clear();
        }
    }

    [MessageHandler((ushort)ServerToClient.match)] // match found
    private static void matchjoined(Message message)
    {
        GameLogic.Reset();
        NetworkManager.Singleton.rlitems();
        
        UIManager.Singleton.pgr_slider.GetComponent<Slider>().value = 0;

        string mid = message.GetString();
        int pc = message.GetInt();
        int cap = message.GetInt();
        Vector3 spos = message.GetVector2();
        Vector3 epos = message.GetVector2();
        epos.z = 13.8f;
        int map = message.GetInt();
        Debug.Log("received map "+map.ToString());

        GameLogic.Singleton.gameidtext.text = "Match: " + mid;
        GameLogic.Singleton.startpos = spos;
        GameLogic.Singleton.endpos = epos;
        GameLogic.mapid = map;
        GameLogic.Singleton.status = "filling";
        GameLogic.Singleton.id = mid;
        GameLogic.Singleton.pcount = pc;
        GameLogic.Singleton.capacity = cap;
        GameLogic.Singleton.matchplayers.Clear();

        UIManager.Singleton.connectUI.SetActive(false);
        UIManager.Singleton.menuUI.SetActive(false);
        UIManager.Singleton.waitUI.SetActive(true);

        UIManager.Singleton.wait_match_id.text = mid;
        UIManager.Singleton.wait_pln.text = pc.ToString()+"/"+cap.ToString();
        foreach (Transform child in UIManager.Singleton.wait_players.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        string ftab_text = mid+ "  :  "+pc.ToString()+"/"+cap.ToString()+"\n";

        TMP_Text tab_text = UIManager.Singleton.tabUI.transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
        TMP_InputField esc_mid = UIManager.Singleton.escUI.transform.GetChild(3).gameObject.GetComponent<TMP_InputField>();
        esc_mid.text = mid;

        //Debug.Log(pc.ToString()+" players to spawn");



        GameObject pt = Instantiate(UIManager.Singleton.wait_player_prefab);
        pt.transform.parent = UIManager.Singleton.wait_players.transform;
        pt.GetComponent<TMP_Text>().text = UIManager.localusername;

        Debug.Log("new player " + Player.localcara);
        GameObject pl = Instantiate(GameLogic.prefabofcara(Player.localcara), new Vector3(-192, 0, 0), Quaternion.identity);
        TMP_Text pltext = pl.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>();
        pltext.text = UIManager.localusername;
        pl.name = UIManager.localusername;
        pl.SetActive(false);

        Player rpl = pl.GetComponent<Player>();

        rpl.listitem = pt;

        rpl.IsLocal = true;

        rpl.Id = NetworkManager.Singleton.Client.Id;
        rpl.cara = Player.localcara;
        rpl.username = UIManager.localusername;

        GameLogic.Singleton.matchplayers.Add(rpl.Id, rpl);

        ftab_text += UIManager.localusername + "\n";



        for (int i=0;i<pc;i++) // adding match's players
        {
            ushort pid = message.GetUShort();
            string username = message.GetString();
            string cara = message.GetString();

            //Debug.Log(username+" spawned");

            if (username != "" && !GameLogic.Singleton.matchplayers.ContainsKey(pid))
            {
                pt = Instantiate(UIManager.Singleton.wait_player_prefab);
                pt.transform.parent = UIManager.Singleton.wait_players.transform;
                pt.GetComponent<TMP_Text>().text = username;

                Debug.Log("new player "+cara);
                pl = Instantiate(GameLogic.prefabofcara(cara), new Vector3(-192,0,0), Quaternion.identity);
                pltext = pl.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>();
                pltext.text = username;
                pl.name = username;
                pl.SetActive(false);

                rpl = pl.GetComponent<Player>();

                rpl.listitem = pt;
                
                rpl.IsLocal = (pid == NetworkManager.Singleton.Client.Id);
                
                rpl.Id = pid;
                rpl.cara = cara;
                rpl.username = username;


                if (!rpl.IsLocal)
                {
                    rpl.GetComponent<Rigidbody2D>().simulated = false;
                    pl.transform.position = spos;
                }

                GameLogic.Singleton.matchplayers.Add(pid, rpl);

                ftab_text += username + "\n";
            }
        }

        tab_text.text = ftab_text;

        Debug.Log("joined "+mid.ToString());
    }

    [MessageHandler((ushort)ServerToClient.matchstatus)] // a player joined or leaved the match
    private static void receivedmatchstatus(Message message)
    {
        ushort pid = message.GetUShort();
        string username = message.GetString();
        string cara = message.GetString();
        bool joined = message.GetBool();

        if (joined)
        {
            if (!GameLogic.Singleton.matchplayers.ContainsKey(pid))
            {
                //GameLogic.Singleton.matchplayers.Add(pid, new Player(pid, username, false));
                GameLogic.Singleton.pcount++;

                GameObject pt = Instantiate(UIManager.Singleton.wait_player_prefab);
                pt.transform.parent = UIManager.Singleton.wait_players.transform;
                pt.GetComponent<TMP_Text>().text = username;


                GameObject pl = Instantiate(GameLogic.prefabofcara(cara), new Vector3(-192, 0f, 0f), Quaternion.identity);
                TMP_Text pltext = pl.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
                pltext.text = username;
                pl.name = username;
                pl.transform.position = GameLogic.Singleton.startpos;
                pl.SetActive(false);

                Player rpl = pl.GetComponent<Player>();

                rpl.listitem = pt;

                rpl.IsLocal = (pid == NetworkManager.Singleton.Client.Id);
                if (!rpl.IsLocal)
                {
                    rpl.GetComponent<Rigidbody2D>().simulated = false;
                    pl.transform.position = new Vector3(0f, 0f, 1f) ;
                }
                rpl.Id = pid;
                rpl.cara = cara;
                rpl.username = username;

                GameLogic.Singleton.matchplayers.Add(pid, rpl);
                
                TMP_Text tab_text = UIManager.Singleton.tabUI.transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
                tab_text.text += username+"\n";
                tab_text.text = tab_text.text.Replace(GameLogic.Singleton.id + "  :  " + (GameLogic.Singleton.pcount-1).ToString() + "/" + GameLogic.Singleton.capacity.ToString(), GameLogic.Singleton.id + "  :  " + (GameLogic.Singleton.pcount).ToString() + "/" + GameLogic.Singleton.capacity.ToString());
            }
            Debug.Log(username+" joined, "+GameLogic.Singleton.pcount.ToString()+"/"+GameLogic.Singleton.capacity.ToString());
        } else
        {
            if (GameLogic.Singleton.matchplayers.ContainsKey(pid))
            {
                Destroy(GameLogic.Singleton.matchplayers[pid].listitem.gameObject);
                Destroy(GameLogic.Singleton.matchplayers[pid].gameObject);
                GameLogic.Singleton.matchplayers.Remove(pid);

                TMP_Text tab_text = UIManager.Singleton.tabUI.transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
                tab_text.text = tab_text.text.Replace("\n"+username+"\n", "\n").Replace(GameLogic.Singleton.id+ "  :  "+ GameLogic.Singleton.pcount.ToString()+"/"+GameLogic.Singleton.capacity.ToString(), GameLogic.Singleton.id + "  :  " + (GameLogic.Singleton.pcount-1).ToString() + "/" + GameLogic.Singleton.capacity.ToString());

                GameLogic.Singleton.pcount--;
            }
            Debug.Log(username + " left, " + GameLogic.Singleton.pcount.ToString() + "/" + GameLogic.Singleton.capacity.ToString());
        }
        UIManager.Singleton.wait_pln.text = GameLogic.Singleton.matchplayers.Count.ToString() + "/" + GameLogic.Singleton.capacity.ToString();

        if (GameLogic.Singleton.matchplayers.Count == GameLogic.Singleton.capacity)
        {
            GameLogic.Singleton.status = "starting";
            
            Debug.Log("requested number of players");
        } else if (GameLogic.Singleton.status == "starting")
        {
            Debug.Log("Aborting start");
            GameLogic.Singleton.status = "aborted";
        }
    }

}
