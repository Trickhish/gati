using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.NetworkInformation;
using Ping = System.Net.NetworkInformation.Ping;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class server_reachability : MonoBehaviour
{
    float lt=0;
    float tp = 0.5f;

    void Start()
    {
        
    }

    public static bool visible()
    {
        return (UIManager.Singleton.connectUI.activeSelf);
    }



    public static bool PingHost(string hostUri, int portNumber)
    {
        try
        {
            using (var client = new TcpClient(hostUri, 80))
                return true;
        }
        catch (SocketException ex)
        {
            return false;
        }
    }

    public bool pingip(string ip)
    {
        try
        {
            return (new Ping().Send(ip).Status == IPStatus.Success);
        }
        catch
        {
            return (false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (UIManager.Singleton.connectUI.activeSelf && GameLogic.Singleton.id=="" && Time.realtimeSinceStartup - lt > tp)
        {
            //bool rs = PingHost(NetworkManager.Singleton.ip, NetworkManager.Singleton.port);
            //bool rs = pingip(NetworkManager.Singleton.ip);

            //Debug.Log(rs);

            if (NetworkManager.Singleton.Client.IsConnected && NetworkManager.Singleton.status == "connected")
            {
                GetComponent<Image>().color = new Color32(50, 120, 45, 255);
                if (visible())
                {
                    GetComponent<Animation>().Stop();
                    GetComponent<Image>().enabled = true;
                }
                tp = 2;

                //UIManager.Singleton.connectbt.interactable = true;
                UIManager.Singleton.enter_login.interactable = true;
                UIManager.Singleton.enter_register.interactable = true;

                UIManager.Singleton.public_match.interactable = true;
                UIManager.Singleton.private_match.interactable = true;
            }
            else
            {
                tp = 0.5f;

                if (visible())
                {
                    GetComponent<Image>().color = new Color32(99, 0, 22, 255);

                    GetComponent<Animation>().Play();
                }

                if (UIManager.Singleton.menuUI.activeSelf || UIManager.Singleton.waitUI.activeSelf || GameLogic.Singleton.id!="")
                {
                    UIManager.Singleton.menuUI.SetActive(false);
                    UIManager.Singleton.waitUI.SetActive(false);

                    foreach (GameObject s in GameLogic.Singleton.maps)
                    {
                        s.SetActive(false);
                    }

                    UIManager.Singleton.login_form.SetActive(false);
                    UIManager.Singleton.register_form.SetActive(false);

                    UIManager.Singleton.connectUI.SetActive(true);
                }

                //UIManager.Singleton.connectbt.interactable = false;

                UIManager.Singleton.enter_login.interactable = false;
                UIManager.Singleton.enter_register.interactable = false;

                UIManager.Singleton.public_match.interactable = false;
                UIManager.Singleton.private_match.interactable = false;
            }

            if (!NetworkManager.Singleton.Client.IsConnected && !NetworkManager.Singleton.Client.IsConnecting)
            {
                NetworkManager.Singleton.Connect();
            }

            lt = Time.realtimeSinceStartup;
        }
    }

    private void FixedUpdate()
    {
        if (!visible())
        {
            GetComponent<Image>().enabled = false;
        } else if (NetworkManager.Singleton.Client.IsConnected)
        {
            GetComponent<Image>().enabled = true;
        }
    }
}
