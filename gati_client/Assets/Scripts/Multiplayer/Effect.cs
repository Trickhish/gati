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
        this.StartTime = Time.realtimeSinceStartup;

        Debug.Log("new effect " + this.Name);
        switch (this.Name)
        {
            case "stun":
                GameLogic.Singleton.localplayer.canmove = false;
                break;
            default:
                break;
        }
    }

    private void Start() // #addeffect
    {
        
    }

    public bool Update()
    {
        if ((Time.realtimeSinceStartup - this.StartTime) >= this.Duration)
        {
            GameLogic.Singleton.localplayer.effects.Remove(this);
            switch (this.Name)
            {
                case "stun":
                    GameLogic.Singleton.localplayer.canmove = true;
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
