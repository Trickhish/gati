using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect
{
    public string Name {get;set;}
    public float StartTime {get;set;}
    public float Duration {get;set;}
    public Player pl {get;set;}
    public float Force {get;set;}

    public Effect(Player p, string name, float dur, float force=1)
    {
        this.Name = name;
        this.Duration = dur;
        this.Force = force;
        this.StartTime = Time.realtimeSinceStartup;
        this.pl = p;
    }

    private void Start()
    {
        switch(this.Name)
        {
            case "stun":
                break;
            default:
                break;
        }
    }

    public bool Update()
    {
        if ((Time.realtimeSinceStartup - this.StartTime) >= this.Duration)
        {
            NetworkManager.log("Effect " + this.Name + " ended for " + this.pl.Username, "PS");
            this.pl.effects.Remove(this);
            return(true);
        } else
        {
            //NetworkManager.log("Still " + (this.Duration - (Time.realtimeSinceStartup - this.StartTime)).ToString() + "s of " + this.Name + " for " + this.pl.Username, "PS");
            return(false);
        }
    }
}
