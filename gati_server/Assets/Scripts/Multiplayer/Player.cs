using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> plist = new Dictionary<ushort, Player>();
    public ushort Id { get; set; }
    public string Username { get; set; }
    public string Mail { get; set; }
    public string matchid { get; set; }
    public Vector3 position { get; set; }
    public Dictionary<string, int> items { get; set; }
    public string cara;

    private void OnDestroy()
    {
        plist.Remove(Id);
    }

    public Player(ushort id, string mail, string username)
    {
        Id = id;
        Username = username;
        Mail = mail;
        matchid = "";
        cara = "";
        position = new Vector3(-192f, 0f, 0f);
        items = new Dictionary<string, int>();
    }

    public Player(ushort id, string mid)
    {
        Id = id;
        matchid = mid;
        Username = "";
        cara = "";
        Mail = "";
        position = new Vector3(-192f, 0f, 0f);
        items = new Dictionary<string, int>();
    }

    public Player(ushort id)
    {
        Id = id;
        matchid = "";
        Username = "";
        Mail = "";
        cara = "";
        position = new Vector3(-192f, 0f, 0f);
        items = new Dictionary<string, int>();
    }

    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message)
    {
        Debug.Log("player " + fromClientId + " added");
        string nck = message.GetString();

        //Player.plist.Add(fromClientId, new Player(fromClientId, nck));

        //Spawn(fromClientId, message.GetString());
    }

}