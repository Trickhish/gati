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
    public Dictionary<string, int> items {get;set;}

    private void OnDestroy()
    {
        plist.Remove(Id);
    }

    public Player(ushort id, string mail, string username)
    {
        this.Id = id;
        this.Username = username;
        this.Mail = mail;
        this.matchid = "";
        this.position = new Vector3(-192, 0, 0);
        this.items = new Dictionary<string, int>();
    }

    public Player(ushort id, string mid)
    {
        this.Id = id;
        this.matchid = mid;
        this.Username = "";
        this.Mail = "";
        this.position = new Vector3(-192, 0, 0);
        this.items = new Dictionary<string, int>();
    }

    public Player(ushort id)
    {
        this.Id = id;
        this.matchid = "";
        this.Username = "";
        this.Mail = "";
        this.position = new Vector3(-192, 0, 0);
        this.items = new Dictionary<string, int>();
    }

    public static void Spawn(ushort id, string username)
    {
        foreach(Player otherPlayer in plist.Values)
        {
            otherPlayer.SendSpawned(id);
        }

        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(-192, 0f, 0f), Quaternion.identity).GetComponent<Player>();
        
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;

        player.SendSpawned();
        if (!plist.ContainsKey(id))
        {
            plist.Add(id, player);
        } else
        {
            player.Mail = plist[id].Mail;
            player.matchid = plist[id].matchid;
            player.Username = plist[id].Username;
            player.name = plist[id].Username;

            plist[id] = player;
        }
    }

    #region Messages
    private void SendSpawned()
    {
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.playerSpawned)));

    }

    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.playerSpawned)), toClientId);
    }

    private Message AddSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.position);

        return (message);
    }

    #endregion

    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message)
    {
        Debug.Log("player "+fromClientId+" added");
        string nck = message.GetString();

        //Player.plist.Add(fromClientId, new Player(fromClientId, nck));

        //Spawn(fromClientId, message.GetString());
    }

}