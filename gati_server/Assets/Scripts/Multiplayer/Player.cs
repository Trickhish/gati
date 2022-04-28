using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

public class Player : MonoBehaviour
{
	public static Dictionary<ushort, Player> plist = new Dictionary<ushort, Player>();

	public string cara;

	public ushort Id { get; set; }

	public string Username { get; set; }

	public string Mail { get; set; }

	public string matchid { get; set; }

	public Vector3 position { get; set; }

	public Dictionary<string, int> items { get; set; }

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
		position = new Vector3(-192f, 0f, 0f);
		items = new Dictionary<string, int>();
	}

	public Player(ushort id, string mid)
	{
		Id = id;
		matchid = mid;
		Username = "";
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
		position = new Vector3(-192f, 0f, 0f);
		items = new Dictionary<string, int>();
	}

	public static void Spawn(ushort id, string username)
	{
		foreach (Player value in plist.Values)
		{
			value.SendSpawned(id);
		}
		Player component = Object.Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(-192f, 0f, 0f), Quaternion.identity).GetComponent<Player>();
		component.name = string.Format("Player {0} ({1})", id, string.IsNullOrEmpty(username) ? "Guest" : username);
		component.Id = id;
		component.Username = (string.IsNullOrEmpty(username) ? $"Guest {id}" : username);
		component.SendSpawned();
		if (!plist.ContainsKey(id))
		{
			plist.Add(id, component);
			return;
		}
		component.Mail = plist[id].Mail;
		component.matchid = plist[id].matchid;
		component.Username = plist[id].Username;
		component.name = plist[id].Username;
		plist[id] = component;
	}

	private void SendSpawned()
	{
		NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, 1)));
	}

	private void SendSpawned(ushort toClientId)
	{
		NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, 1)), toClientId);
	}

	private Message AddSpawnData(Message message)
	{
		message.AddUShort(Id);
		message.AddString(Username);
		message.AddVector3(base.transform.position);
		return message;
	}

	[MessageHandler(1, 0)]
	private static void Name(ushort fromClientId, Message message)
	{
		Debug.Log("player " + fromClientId + " added");
		message.GetString();
	}
}
