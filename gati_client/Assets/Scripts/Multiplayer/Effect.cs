using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public string Name { get; set; }
    public float StartTime { get; set; }
    public float Duration { get; set; }
    public Player pl { get; set; }

    public Effect(string name, float dur)
    {
        this.Name = name;
        this.Duration = dur;
        this.StartTime = Mathf.RoundToInt(Time.realtimeSinceStartup * 1000);

        Debug.Log("new effect " + this.Name);
        switch (this.Name)
        {
            case "stun":
                GameLogic.Singleton.localplayer.canmove = false;
                break;
            case "speed":
                GameLogic.Singleton.localplayer.mov.Maxspeed+=2;
                GameLogic.Singleton.localplayer.mov.Acceleration+=1;
                break;
            case "agility":
                GameLogic.Singleton.localplayer.mov.Agility+=2;
                break;
            case "disability":
                GameLogic.Singleton.localplayer.canuseobjects = false;
                break;
            case "flash":
                UIManager.flash(this.Duration);
                GameLogic.Singleton.localplayer.effects.Remove(this);
                break;
            case "invisibility":
                GameLogic.Singleton.localplayer.GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, 0.784f);
                break;
            case "web_slowness":
                GameLogic.Singleton.localplayer.mov.Maxspeed -= 4;
                GameLogic.Singleton.localplayer.mov.Acceleration -= 3;
                break;
            default:
                break;
        }
    }

    private void Start() // #addeffect
    {
        
    }

    public void Clear()
    {
        GameLogic.Singleton.localplayer.effects.Remove(this);
        switch (this.Name)
        {
            case "stun":
                GameLogic.Singleton.localplayer.canmove = true;
                break;
            case "speed":
                GameLogic.Singleton.localplayer.mov.Maxspeed-=2;
                GameLogic.Singleton.localplayer.mov.Acceleration-=1;
                break;
            case "agility":
                GameLogic.Singleton.localplayer.mov.Agility-=2;
                break;
            case "disability":
                GameLogic.Singleton.localplayer.canuseobjects = true;
                break;
            case "flash":
                UIManager.Singleton.flashobj.SetActive(false);
                break;
            case "invisibility":
                GameLogic.Singleton.localplayer.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                break;
            case "web_slowness":
                GameLogic.Singleton.localplayer.mov.Maxspeed += 4;
                GameLogic.Singleton.localplayer.mov.Acceleration += 3;
                break;
            default:
                break;
        }
    }

    public bool Update()
    {
        if (this.Duration!=-1 && (Mathf.RoundToInt(Time.realtimeSinceStartup * 1000) - this.StartTime) >= this.Duration)
        {
            Debug.Log("no longer under "+this.Name);
            GameLogic.Singleton.localplayer.effects.Remove(this);
            switch (this.Name)
            {
                case "stun":
                    GameLogic.Singleton.localplayer.canmove = true;
                    break;
                case "speed":
                    GameLogic.Singleton.localplayer.mov.Maxspeed-=2;
                    GameLogic.Singleton.localplayer.mov.Acceleration-=1;
                    break;
                case "agility":
                    GameLogic.Singleton.localplayer.mov.Agility-=2;
                    break;
                case "disability":
                    GameLogic.Singleton.localplayer.canuseobjects = true;
                    break;
                case "flash":
                    UIManager.Singleton.flashobj.SetActive(false);
                    break;
                case "invisibility":
                    GameLogic.Singleton.localplayer.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                    break;
                case "web_slowness":
                    GameLogic.Singleton.localplayer.mov.Maxspeed += 4;
                    GameLogic.Singleton.localplayer.mov.Acceleration += 3;
                    break;
                default:
                    break;
            }
            return (true);
        } else
        {
            return(false);
        }
    }
}
