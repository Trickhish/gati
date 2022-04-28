using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using System;

public enum ServerToClient : ushort
{
    playerSpawned = 1,
    match = 2,
    matchstatus = 3,
    updatepos = 4,
    rcvplayerupdate = 5,
    launch = 6,
    matchend = 7,
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
    register = 8,
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
