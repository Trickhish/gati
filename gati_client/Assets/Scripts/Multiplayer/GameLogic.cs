using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RiptideNetworking;
using System.Linq;

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
    public Vector3 startpos;
    public Vector3 endpos;

    public Player localplayer => (matchplayers.ContainsKey(NetworkManager.Singleton.Client.Id) ? matchplayers[NetworkManager.Singleton.Client.Id] : matchplayers.FirstOrDefault().Value);

    //public GameObject Gati_prefab => gati_prefab;
    //public GameObject Localplayerprefab => local_prefab;


    private void Awake()
    {
        Singleton = this;
    }

    private void Update()
    {
        if (Time.realtimeSinceStartup-t>1 && stct>=-1)
        {
            t = Time.realtimeSinceStartup;
            if (stct==0)
            {
                GameLogic.Singleton.localplayer.GetComponent<Movement>().enabled = true;
                UIManager.Singleton.stcounter.text = "GO";
            }
            else if (stct<=-1)
            {
                UIManager.Singleton.stcounter.text = "";
                stct = -2;
                UIManager.Singleton.start_ui.SetActive(false);
            } else
            {
                UIManager.Singleton.stcounter.text = stct.ToString();
            }
            stct -= 1;
        }
    }

    public static GameObject prefabofcara(string cara)
    {
        switch(cara)
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
        GameLogic.Singleton.maxpos = Vector3.Distance(GameLogic.Singleton.startpos, GameLogic.Singleton.endpos);

        Debug.Log("Match Starting");

        GameLogic.Singleton.gamescene.SetActive(true);
        UIManager.Singleton.pgr_slider.SetActive(true);
        UIManager.Singleton.item_bar.SetActive(true);

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

        switch(status)
        {
            case "running_left":
                GameLogic.Singleton.matchplayers[pid].GetComponent<SpriteRenderer>().flipX = true;
                amt.SetBool("Idle", false);
                break;
            case "running_right":
                GameLogic.Singleton.matchplayers[pid].GetComponent<SpriteRenderer>().flipX = false;
                amt.SetBool("Idle", false);
                break;
            case "jumping":
                amt.SetTrigger("Jump");
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
                break;
            case "capacity":
                amt.SetTrigger("Capacity");
                break;
            case "falling":
                amt.SetFloat("AirSpeedY", 1f);
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
}
