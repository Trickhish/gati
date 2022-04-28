// NetworkManager
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RiptideNetworking;
using UnityEngine;

public enum ServerToClient : ushort
{
	playerSpawned = 1,
	match = 2,
	matchstatus = 3,
	updatepos = 4,
	rcvplayerposupdate = 5,
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

	private static string sat = "ShOi3HUwJ1BdRfj5eFrQcsM40PGaT82Vk9Imluy7xo6vnXLCbKWq";

	public static string version;

	[SerializeField]
	private ushort port;

	[SerializeField]
	private ushort maxclientcount;

	public static NetworkManager Singleton
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
				UnityEngine.Object.Destroy(value);
			}
		}
	}

	public Server Server { get; private set; }

	private void Awake()
	{
		Singleton = this;
	}

	public static void log(string msg, string elv)
	{
		string text = DateTime.Now.ToString("HH:mm:ss");
		Debug.Log("[" + text + "]" + elv + ": " + msg);
	}

	private void Start()
	{
		Application.targetFrameRate = 60;
		if (File.Exists(Application.dataPath + "/version"))
		{
			version = new StreamReader(Application.dataPath + "/version").ReadToEnd();
		}
		else
		{
			version = "Debug";
		}
		Server = new Server(5000, 1000);
		Server.Start(port, maxclientcount, 0);
		if (Server.IsRunning)
		{
			log("Server Started with version " + version, "SS");
		}
		Server.ClientDisconnected += PlayerLeft;
		Server.ClientConnected += PlayerJoined;
	}

	private void FixedUpdate()
	{
		Server.Tick();
	}

	private void OnApplicationQuit()
	{
		log("Server Stopped.", "SS");
		Server.Stop();
	}

	public static string getreq(string uri)
	{
		try
		{
			HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(uri);
			obj.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			using HttpWebResponse httpWebResponse = (HttpWebResponse)obj.GetResponse();
			if (httpWebResponse.StatusCode == HttpStatusCode.NotFound)
			{
				return "unreachable";
			}
			if (!new List<int>
			{
				200, 201, 202, 203, 204, 205, 206, 207, 208, 210,
				226
			}.Contains((int)httpWebResponse.StatusCode))
			{
				return "error";
			}
			using Stream stream = httpWebResponse.GetResponseStream();
			using StreamReader streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd();
		}
		catch
		{
			return "unreachable";
		}
	}

	public static void renew(ushort pid)
	{
		string mail = Player.plist[pid].Mail;
		getreq("https://trickhisch.alwaysdata.net/gati/?a=renew&m=" + mail + "&sat=" + sat);
	}

	public static void setstatus(ushort pid, bool ingame)
	{
		string mail = Player.plist[pid].Mail;
		string text = ((!ingame) ? getreq("https://trickhisch.alwaysdata.net/gati/?a=snig&m=" + mail + "&sat=" + sat) : getreq("https://trickhisch.alwaysdata.net/gati/?a=sig&m=" + mail + "&sat=" + sat));
		if (text == "false")
		{
			Task.Run(async delegate
			{
				await Task.Delay(500);
				setstatus(pid, ingame);
			});
		}
	}

	private void PlayerJoined(object sender, ServerClientConnectedEventArgs e)
	{
		ushort id = e.Client.Id;
		if (!Player.plist.ContainsKey(id))
		{
			Player.plist.Add(id, new Player(id));
		}
		log("Player " + id + " joined.", "J");
	}

	private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
	{
		if (Player.plist.ContainsKey(e.Id))
		{
			if (Player.plist[e.Id].Username != "")
			{
				log("Player " + e.Id + " (" + Player.plist[e.Id].Username + ") left.", "L");
			}
			else
			{
				log("Player " + e.Id + " left.", "L");
			}
			setstatus(e.Id, ingame: false);
			string matchid = Player.plist[e.Id].matchid;
			if (matchid != "")
			{
				if (Match.mlist[matchid].players.ContainsKey(e.Id))
				{
					Match.mlist[matchid].players.Remove(e.Id);
				}
				if (Match.mlist[matchid].players.Count == 0)
				{
					Match.mlist.Remove(matchid);
					log("no player in match " + matchid + ", match removed", "M");
				}
			}
			Player.plist.Remove(e.Id);
		}
		else
		{
			log("Player " + e.Id + " left.", "L");
		}
	}

	public static string[] hashstring(string input)
	{
		int num = 24;
		int cb = 24;
		int iterations = 100000;
		RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		byte[] array = new byte[num];
		rNGCryptoServiceProvider.GetBytes(array);
		Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(input, array, iterations);
		return new string[2]
		{
			Encoding.UTF8.GetString(array),
			Encoding.UTF8.GetString(rfc2898DeriveBytes.GetBytes(cb))
		};
	}

	[MessageHandler(8, 0)]
	private static void register(ushort cid, Message m)
	{
		m.GetString();
		string @string = m.GetString();
		@string = m.GetString() + @string;
	}

	[MessageHandler(7, 0)]
	private static void login(ushort cid, Message m)
	{
		string @string = m.GetString();
		string string2 = m.GetString();
		log("Player " + cid + ", signed in: " + @string + ", " + string2, "S");
		if (Player.plist.ContainsKey(cid))
		{
			Player.plist[cid].Mail = string2;
			Player.plist[cid].Username = @string;
		}
		else
		{
			Player.plist.Add(cid, new Player(cid, string2, @string));
		}
		setstatus(cid, ingame: true);
	}
}
