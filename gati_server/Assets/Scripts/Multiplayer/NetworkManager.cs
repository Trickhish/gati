using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Security.Cryptography;

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

    public Server Server { get; private set; }

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxclientcount;

    private void Awake()
    {
        Singleton = this;
    }


    // Start is called before the first frame update
    void Start()
    {

        Application.targetFrameRate = 60;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server = new Server();
        Server.Start(port, maxclientcount);
        Server.ClientDisconnected += PlayerLeft;
    }

    private void FixedUpdate()
    {
        Server.Tick();
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        string mid = Player.plist[e.Id].matchid;

        if (mid != "")
        {
            if (Match.mlist[mid].players.ContainsKey(e.Id))
            {
                Match.mlist[mid].players.Remove(e.Id);
            }

            if (Match.mlist[mid].players.Count == 0)
            {
                Match.mlist.Remove(mid);
                Debug.Log("no player in match "+mid+", match removed");
            }
        }

        Player.plist.Remove(e.Id);
        
    }

    public static string[] hashstring(string input)
    {
        int SALT_SIZE = 24;
        int HASH_SIZE = 24;
        int ITERATIONS = 100000;

        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        byte[] salt = new byte[SALT_SIZE];
        provider.GetBytes(salt);

        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(input, salt, ITERATIONS);

        return (new string[] { System.Text.Encoding.UTF8.GetString(salt), System.Text.Encoding.UTF8.GetString(pbkdf2.GetBytes(HASH_SIZE)) });
    }

    [MessageHandler((ushort)ClientToServerId.register)]
    private static void register(ushort cid, Message m)
    {
        string mail = m.GetString();
        string pass = m.GetString();
        string salt = m.GetString();
        pass = salt + pass;

        
    }

    [MessageHandler((ushort)ClientToServerId.login)]
    private static void login(ushort cid, Message m)
    {
        string mail = m.GetString();
        string pass = m.GetString();


    }
}
