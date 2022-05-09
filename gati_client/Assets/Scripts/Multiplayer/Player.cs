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
    public Vector3 lpos = new Vector3(0,0,0);
    public bool isGrounded = false;
    public float maxpos;
    public Collider2D ccld;
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

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {

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
        //this.lpos = new Vector3(0,0,0);
        this.cara = "drije";
    }

    public void updatepos()
    {
        Message message = Message.Create(MessageSendMode.unreliable, (ushort)ClientToServerId.playerposupdate);
        if (this.ismoving)
        {
            if (this.isGrounded)
            {
                if (this.GetComponent<SpriteRenderer>().flipX)
                {
                    message.AddString("walking_left");
                } else
                {
                    message.AddString("walking_right");
                }
                
            } else
            {
                message.AddString("jumping");
            }
        } else
        {
            message.AddString("idle");
        }
        message.AddVector3(this.transform.position);
        NetworkManager.Singleton.Client.Send(message);
    }

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        Player player;
        player = Instantiate(GameLogic.Singleton.gati_prefab, position, Quaternion.identity).GetComponent<Player>();
        player.IsLocal = (id == NetworkManager.Singleton.Client.Id);

        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.username = username;

        list.Add(id, player);
    }

    [MessageHandler((ushort)ServerToClient.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClient.match)]
    //match found
    private static void matchjoined(Message message)
    {
        
        UIManager.Singleton.pgr_slider.GetComponent<Slider>().value = 0;

        string mid = message.GetString();
        int pc = message.GetInt();
        int cap = message.GetInt();
        Vector3 spos = message.GetVector2();

        GameLogic.Singleton.gameidtext.text = "Match: " + mid;


        UIManager.Singleton.connectUI.SetActive(false);
        UIManager.Singleton.menuUI.SetActive(false);
        UIManager.Singleton.waitUI.SetActive(true);

        UIManager.Singleton.wait_match_id.text = mid;
        UIManager.Singleton.wait_pln.text = pc.ToString()+"/"+cap.ToString();
        foreach (Transform child in UIManager.Singleton.wait_players.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //GameLogic.Singleton.gamescene.SetActive(true);

        GameLogic.Singleton.id = mid;
        GameLogic.Singleton.pcount = pc;
        GameLogic.Singleton.capacity = cap;
        GameLogic.Singleton.matchplayers.Clear();

        string ftab_text = mid+ "  :  "+pc.ToString()+"/"+cap.ToString()+"\n";

        TMP_Text tab_text = UIManager.Singleton.tabUI.transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
        TMP_InputField esc_mid = UIManager.Singleton.escUI.transform.GetChild(3).gameObject.GetComponent<TMP_InputField>();
        esc_mid.text = mid;

        Debug.Log(pc.ToString()+" players to spawn");

        for (int i=0;i<pc;i++)
        {
            ushort pid = message.GetUShort();
            string username = message.GetString();
            string cara = message.GetString();

            Debug.Log(username+" spawned");

            if (username != "")
            {
                GameObject pt = Instantiate(UIManager.Singleton.wait_player_prefab);
                pt.transform.parent = UIManager.Singleton.wait_players.transform;
                pt.GetComponent<TMP_Text>().text = username;

                GameObject pl = Instantiate(GameLogic.prefabofcara(cara), new Vector3(-192,0,0), Quaternion.identity);
                TMP_Text pltext = pl.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>();
                pltext.text = username;
                pl.name = username;
                pl.SetActive(false);

                Player rpl = pl.GetComponent<Player>();

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

    [MessageHandler((ushort)ServerToClient.matchstatus)]
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


                GameObject pl = Instantiate(GameLogic.Singleton.gati_prefab, new Vector3(-192, 0f, 0f), Quaternion.identity);
                TMP_Text pltext = pl.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
                pltext.text = username;
                pl.name = username;
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
