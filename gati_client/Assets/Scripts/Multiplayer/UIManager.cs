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

    [Header("UI panels")]
    [SerializeField] public GameObject connectUI;
    [SerializeField] public GameObject menuUI;
    [SerializeField] public GameObject escUI;
    [SerializeField] public GameObject tabUI;
    [SerializeField] public GameObject waitUI;

    [Header("Authentification")]
    [SerializeField] public Button enter_login;
    [SerializeField] public Button enter_register;
    [SerializeField] public TMP_InputField login_email;
    [SerializeField] public TMP_InputField login_password;
    [SerializeField] public TMP_InputField register_username;
    [SerializeField] public TMP_InputField register_email;
    [SerializeField] public TMP_InputField register_password;

    [Header("Wait UI")]
    [SerializeField] public TMP_Text wait_match_id;
    [SerializeField] public GameObject wait_player_prefab;
    [SerializeField] public GameObject wait_players;
    [SerializeField] public TMP_Text wait_pln;

    [Header("Private Match Creation")]
    [SerializeField] public TMP_Dropdown private_plc;
    [SerializeField] public TMP_Dropdown private_map;
    [SerializeField] public TMP_InputField private_mid;

    [Header("Misc")]
    [SerializeField] public TMP_InputField usernameField;
    [SerializeField] public Button connectbt;
    [SerializeField] public TMP_Text statustext;
    [SerializeField] public TMP_Text usertext_lo;
    [SerializeField] public GameObject pgr_slider;
    [SerializeField] public InputField ipfield;
    [SerializeField] public NetworkManager nwm;

    [SerializeField] public TMP_Text stcounter;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        connectUI.SetActive(true);
        menuUI.SetActive(false);
        escUI.SetActive(false);
        tabUI.SetActive(false);
        waitUI.SetActive(false);
        pgr_slider.SetActive(false);
        GameLogic.Singleton.gameidtext.text = "";
        GameLogic.Singleton.gamescene.SetActive(false);
    }

    public void ConnectClicked()
    {
        if (NetworkManager.Singleton.Client.IsConnected)
        {
            usernameField.interactable = false;
            statustext.color = Color.white;
            //connectbt.interactable = false;

            if (usernameField.text == "")
            {
                usernameField.text = "Guest";
            }

            localusername = usernameField.text;
            usertext_lo.text = localusername;
            //statustext.text = "Connecting ...";

            UIManager.Singleton.connectUI.SetActive(false);
            UIManager.Singleton.menuUI.SetActive(true);
        }
    }

    public void leavematch()
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.leavematch);
        NetworkManager.Singleton.Client.Send(message);

        GameLogic.Singleton.id = "";
        GameLogic.Singleton.capacity = 0;
        GameLogic.Singleton.pcount = 0;

        UIManager.Singleton.escUI.SetActive(false);
        pgr_slider.SetActive(false);
        UIManager.Singleton.tabUI.SetActive(false);
        UIManager.Singleton.waitUI.SetActive(false);

        if (GameLogic.Singleton.id == "test")
        {
            UIManager.Singleton.menuUI.SetActive(false);
            UIManager.Singleton.connectUI.SetActive(true);
        } else
        {
            UIManager.Singleton.menuUI.SetActive(true);
            UIManager.Singleton.connectUI.SetActive(false);
        }

        foreach(Player p in GameLogic.Singleton.matchplayers.Values)
        {
            Destroy(p.gameObject);
        }
        GameLogic.Singleton.matchplayers.Clear();
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
        localusername = usernameField.text.ToString();
        message.AddString(usernameField.text.ToString());
        NetworkManager.Singleton.Client.Send(message);

        GameLogic.Singleton.gameidtext.text = "Searching for a match";
    }

    public void creatematch_clicked()
    {
        int pc = int.Parse(private_plc.options[private_plc.value].text.Split(' ')[0]);
        int map = private_map.value+1;

        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.createprivate);
        message.AddInt(pc);
        message.AddInt(map);
        message.AddString(localusername);

        NetworkManager.Singleton.Client.Send(message);
    }

    public void joinprivate_clicked()
    {
        if (private_mid.text != "")
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.joinprivate);
            message.AddString(localusername);
            message.AddString(private_mid.text);
            NetworkManager.Singleton.Client.Send(message);
        }
    }

    public void offlinetest_clicked()
    {
        if (localusername == "")
        {
            
            if (usernameField.text == "")
            {
                usernameField.text = "Guest";
            }

            localusername = usernameField.text;
        }

        GameLogic.Singleton.id = "test";
        GameLogic.Singleton.capacity = 1;
        GameLogic.Singleton.pcount = 1;
        GameLogic.Singleton.matchplayers.Clear();

        GameObject pl = Instantiate(GameLogic.Singleton.Playerprefab, new Vector3(0, 0, 0), Quaternion.identity);
        TMP_Text pltext = pl.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>();
        pltext.text = localusername;
        if (pltext.text == "")
        {
            pltext.text = "Guest";
        }
        pl.name = localusername;
        pl.SetActive(true);

        Player rpl = pl.GetComponent<Player>();
        rpl.Id = NetworkManager.Singleton.Client.Id;
        rpl.IsLocal = true;
        rpl.username = localusername;

        GameLogic.Singleton.matchplayers.Add(NetworkManager.Singleton.Client.Id, rpl);

        GameLogic.Singleton.gamescene.SetActive(true);
        UIManager.Singleton.pgr_slider.SetActive(true);
        UIManager.Singleton.connectUI.SetActive(false);
        UIManager.Singleton.menuUI.SetActive(false);
        UIManager.Singleton.waitUI.SetActive(false);

        TMP_Text tab_text = UIManager.Singleton.tabUI.transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
        tab_text.text = "TEST MATCH\n"+localusername;

        cam.trans.position = new Vector3(-4.2f, 3.7f, 0f);


    }

    IEnumerator Postreq(string url, Dictionary<string, string> p)
    {
        WWWForm form = new WWWForm();
        foreach(string k in p.Keys)
        {
            form.AddField(k, p[k]);
        }

        UnityWebRequest req = UnityWebRequest.Post(url, form);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(req.error);
        }
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

    public void login_clicked()
    {
        string mail = login_email.text;
        string[] hr = hashstring(login_password.text,0);
        string pass = hr[1];
        string salt = hr[0];

        Debug.Log("hash : "+pass+" salt : "+salt);

        Message m = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.login);
        m.AddString(mail);
        m.AddString(pass);
    }

    public void register_clicked()
    {
        string username = register_username.text;
        string mail = register_email.text;
        string[] hr = hashstring(register_password.text, 24);
        string pass = hr[1];
        string salt = hr[0];

        Debug.Log("hash : " + pass + " salt : " + salt);

        Message m = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.register);
        m.AddString(mail);
        m.AddString(pass);
        m.AddString(salt);
    }

    public void BackToMain()
    {
        usernameField.interactable = true;
        connectUI.SetActive(true);
        GameLogic.Singleton.gameidtext.text = "";
    }

    public void SendName()
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort) ClientToServerId.name);
        message.AddString(usernameField.text);
        NetworkManager.Singleton.Client.Send(message);
    }
}
