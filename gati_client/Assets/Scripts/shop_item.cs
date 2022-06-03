using System.Collections;
using System.Collections.Generic;
using System.Web;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class shop_item : MonoBehaviour
{
    [SerializeField] public uint price;
    [SerializeField] public string desc;
    public static Dictionary<string, shop_item> items = new Dictionary<string, shop_item>();
    private TMP_Text pricetext;
    private TMP_Text stocktext;
    public int stock { get; set; }
    [SerializeField] public bool realitem = false;

    public shop_item(string n, int stock)
    {
        this.stock = stock;
    }

    void Start()
    {
        pricetext = this.gameObject.transform.GetChild(0).GetComponent<TMP_Text>();
        stocktext = this.gameObject.transform.GetChild(1).GetComponent<TMP_Text>();
        pricetext.text = this.price.ToString() + "$";
        stocktext.text = "?";

        if (items.ContainsKey(this.name))
        {

            this.stock = items[this.name].stock;
            stocktext.text = stock.ToString();
        }
        items[this.name] = this;


        
        

        GetComponent<Button>().onClick.AddListener(click);

        if (Player.money>=this.price && this.realitem)
        {
            GetComponent<Button>().interactable = true;
        } else
        {
            GetComponent<Button>().interactable = false;
        }
    }

    void click()
    {
        string r = NetworkManager.getreq("https://trickhisch.alwaysdata.net/gati/?a=buy&as="+this.name+"&t="+NetworkManager.Singleton.token);

        if (r=="unreachable")
        {

        } else if (r=="true")
        {
            this.stock++;
            stocktext.text = stock.ToString();
            NetworkManager.Singleton.rldt();
            NetworkManager.Singleton.rlmoney();
            NetworkManager.Singleton.rlitems();
        } else
        {

        }
    }

    public void setstock(int n)
    {
        this.stock = n;
        if (realitem)
        {
            this.stocktext.text = stock.ToString();
        }
    }

    public void setdesc()
    {
        UIManager.Singleton.shop_itemdesc.text = (this.realitem ? (this.name + " : " + this.desc) : "");
    }
}
