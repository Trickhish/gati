using System;
using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

public class Match : MonoBehaviour
{
	private static Match _singleton;

	public static Dictionary<string, Match> mlist = new Dictionary<string, Match>();

	public static List<(string, Vector2, Vector2)> maps = new List<(string, Vector2, Vector2)>
	{
		("Etril Sewer", new Vector2(-192f, 0f), new Vector2(2f, -3.5f)),
		("Niya City", new Vector2(-192f, 0f), new Vector2(2f, -3.5f)),
		("Ayrith Forest", new Vector2(-192f, 0f), new Vector2(2f, -3.5f)),
		("Bravo Camp", new Vector2(-192f, 0f), new Vector2(2f, -3.5f)),
		("Pirate Beach", new Vector2(-192f, 0f), new Vector2(2f, -3.5f)),
		("Maya Temple", new Vector2(-192f, 0f), new Vector2(2f, -3.5f)),
		("Snowy Mountain", new Vector2(-192f, 0f), new Vector2(2f, -3.5f)),
		("Prehistory", new Vector2(-192f, 0f), new Vector2(2f, -3.5f)),
		("Camda", new Vector2(-192f, 0f), new Vector2(2f, -3.5f))
	};

	private static System.Random rand = new System.Random();

	public Dictionary<ushort, Player> players = new Dictionary<ushort, Player>();

	public static Match Singleton
	{
		get
		{
			return _singleton;
		}
		private set
		{
			if (_singleton == null)
			{
				_singleton = value;
			}
			else if (_singleton != value)
			{
				Debug.Log("Match");
				UnityEngine.Object.Destroy(value);
			}
		}
	}

	public bool isprivate { get; private set; }

	public int capacity { get; private set; }

	public string status { get; private set; }

	public int map { get; private set; }

	public string id { get; private set; }

	public static string getrandomid(int l)
	{
		string text = "";
		for (int i = 0; i < l; i++)
		{
			int num = rand.Next(0, 36);
			text = ((num >= 26) ? (text + (num - 26)) : (text + (char)(num + 65)));
		}
		return text;
	}

	private void Awake()
	{
		Singleton = this;
	}

	public static string creatematch(bool isprivate, int capacity)
	{
		string text = getrandomid(5);
		int num = 0;
		while (mlist.ContainsKey(text))
		{
			text = getrandomid(5 + num / 50);
			num++;
		}
		Match match = new Match();
		match.id = text;
		match.capacity = capacity;
		match.isprivate = isprivate;
		match.players = new Dictionary<ushort, Player>();
		match.status = "filling";
		match.map = rand.Next(0, 1);
		mlist.Add(text, match);
		return text;
	}

	public static string findmatch()
	{
		foreach (Match value in mlist.Values)
		{
			if (value.players.Count < value.capacity && !value.isprivate)
			{
				return value.id;
			}
		}
		return null;
	}

	public void launch()
	{
		if (capacity != players.Count)
		{
			return;
		}
		Message message = Message.Create(MessageSendMode.reliable, 6);
		foreach (ushort key in players.Keys)
		{
			NetworkManager.Singleton.Server.Send(message, key);
		}
		NetworkManager.log("match " + id + " starting", "M");
	}

	public static void sendmatch(ushort clientid, string mid)
	{
		Message message = Message.Create(MessageSendMode.reliable, 2);
		message.AddString(mid);
		message.AddInt(mlist[mid].players.Count);
		message.AddInt(mlist[mid].capacity);
		message.AddVector2(maps[mlist[mid].map].Item2);
		foreach (Player value in mlist[mid].players.Values)
		{
			message.AddUShort(value.Id);
			message.AddString(value.Username);
			message.AddString(value.cara);
		}
		NetworkManager.Singleton.Server.Send(message, clientid);
	}

	public void sendmatchstatus(ushort pid, string username, string cara, bool joined)
	{
		Message message = Message.Create(MessageSendMode.reliable, 3);
		message.AddUShort(pid);
		message.AddString(username);
		message.AddString(cara);
		message.AddBool(joined);
		foreach (Player value in players.Values)
		{
			NetworkManager.Singleton.Server.Send(message, value.Id);
		}
		if (joined && players.Count == capacity)
		{
			launch();
		}
	}

	[MessageHandler(2, 0)]
	private static void privatematch(ushort cid, Message message)
	{
		int @int = message.GetInt();
		int int2 = message.GetInt();
		string @string = message.GetString();
		string string2 = message.GetString();
		string text = getrandomid(5);
		int num = 0;
		while (mlist.ContainsKey(text))
		{
			text = getrandomid(5 + num / 50);
			num++;
		}
		Match match = new Match();
		match.id = text;
		match.capacity = @int;
		match.isprivate = true;
		match.players = new Dictionary<ushort, Player>();
		match.status = "filling";
		match.map = int2;
		mlist.Add(text, match);
		if (!Player.plist.ContainsKey(cid))
		{
			Player player = new Player(cid, text);
			player.Username = @string;
			player.cara = string2;
			Player.plist.Add(cid, player);
		}
		else
		{
			Player.plist[cid].matchid = text;
			Player.plist[cid].position = maps[mlist[text].map].Item2;
			Player.plist[cid].cara = string2;
		}
		mlist[text].players.Add(cid, Player.plist[cid]);
		sendmatch(cid, text);
		if (mlist[text].players.Count == mlist[text].capacity)
		{
			mlist[text].launch();
			NetworkManager.log("test match, launching right away", "M");
		}
		else
		{
			NetworkManager.log("match " + text + " created", "M");
		}
	}

	[MessageHandler(6, 0)]
	private static void joinprivatematch(ushort clientid, Message message)
	{
		string @string = message.GetString();
		string text = message.GetString().ToUpper();
		string string2 = message.GetString();
		if (mlist.ContainsKey(text))
		{
			NetworkManager.log(@string + " (" + string2 + ") joined match " + text, "M");
			if (!Player.plist.ContainsKey(clientid))
			{
				Player player = new Player(clientid, text);
				player.Username = @string;
				Player.plist.Add(clientid, player);
			}
			else
			{
				Player.plist[clientid].matchid = text;
				Player.plist[clientid].position = maps[mlist[text].map].Item2;
			}
			mlist[text].players.Add(clientid, Player.plist[clientid]);
			sendmatch(clientid, text);
			mlist[text].sendmatchstatus(clientid, @string, string2, joined: true);
		}
		else
		{
			NetworkManager.log(@string + " tried to join inexisting match " + text, "M");
		}
	}

	[MessageHandler(3, 0)]
	private static void getorcreatematch(ushort clientid, Message message)
	{
		string @string = message.GetString();
		string string2 = message.GetString();
		string text = findmatch();
		if (text == null)
		{
			text = creatematch(isprivate: false, 5);
			if (Player.plist.ContainsKey(clientid) && Player.plist[clientid].Username != "")
			{
				NetworkManager.log("New match (" + text + ") by " + Player.plist[clientid].Username + " (" + string2 + ")", "M");
			}
			else
			{
				NetworkManager.log("New match (" + text + ") by Player " + clientid + " (" + string2 + ")", "M");
			}
		}
		else if (Player.plist.ContainsKey(clientid) && Player.plist[clientid].Username != "")
		{
			NetworkManager.log("Match " + text + " found for " + Player.plist[clientid].Username + " (" + string2 + ")", "M");
		}
		else
		{
			NetworkManager.log("Match " + text + " found for Player " + clientid + " (" + string2 + ")", "M");
		}
		if (!Player.plist.ContainsKey(clientid))
		{
			Player player = new Player(clientid, text);
			player.Username = @string;
			Player.plist.Add(clientid, player);
		}
		else
		{
			Player.plist[clientid].matchid = text;
			Player.plist[clientid].position = maps[mlist[text].map].Item2;
		}
		mlist[text].players.Add(clientid, Player.plist[clientid]);
		sendmatch(clientid, text);
		mlist[text].sendmatchstatus(clientid, @string, string2, joined: true);
	}

	[MessageHandler(4, 0)]
	private static void leaverequest(ushort pid, Message message)
	{
		string matchid = Player.plist[pid].matchid;
		if (mlist[matchid].players.ContainsKey(pid))
		{
			mlist[matchid].players.Remove(pid);
		}
		Message message2 = Message.Create(MessageSendMode.reliable, ServerToClient.matchstatus);
		message2.AddUShort(pid);
		message2.AddString(Player.plist[pid].Username);
		message2.AddBool(value: false);
		foreach (ushort key in mlist[matchid].players.Keys)
		{
			NetworkManager.Singleton.Server.Send(message2, key);
		}
		if (Player.plist.ContainsKey(pid) && Player.plist[pid].Username != "")
		{
			NetworkManager.log(Player.plist[pid].Username + " left match " + matchid, "M");
		}
		else
		{
			NetworkManager.log("Player " + pid + " left match " + matchid, "M");
		}
		if (mlist[matchid].players.Count == 0)
		{
			mlist.Remove(matchid);
			NetworkManager.log("no player in match " + matchid + ", match removed", "M");
		}
	}

	[MessageHandler(5, 0)]
	private static void playerupdate_forward(ushort pid, Message message)
	{
		string matchid = Player.plist[pid].matchid;
		string @string = message.GetString();
		Vector3 vector = message.GetVector3();
		Vector3 b = maps[mlist[matchid].map].Item3;
		if (Vector3.Distance(vector, b) < 5f)
		{
			Message message2 = Message.Create(MessageSendMode.reliable, 7);
			message2.AddString(mlist[matchid].players[pid].Username);
			message2.AddUShort(pid);
			foreach (Player value in mlist[matchid].players.Values)
			{
				NetworkManager.Singleton.Server.Send(message2, value.Id);
			}
			mlist.Remove(matchid);
			NetworkManager.log(Player.plist[pid].Username + " (" + pid + ") won", "M");
			return;
		}
		Message message3 = Message.Create(MessageSendMode.unreliable, 5);
		message3.AddString(@string);
		message3.AddUShort(pid);
		message3.AddVector3(vector);
		mlist[matchid].players[pid].position = vector;
		foreach (Player value2 in mlist[matchid].players.Values)
		{
			if (value2.Id != pid)
			{
				NetworkManager.Singleton.Server.Send(message3, value2.Id);
			}
		}
	}
}
