using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect
{
    public string Name {get;set;}
    public float StartTime {get;set;}
    public float Duration {get;set;}
    public Player pl {get;set;}
    public int Force {get;set;}
    public EffectBlock ParentBlock { get; set; }

    public Effect(Player p, string name, float dur, int force=1, EffectBlock pblock=null)
    {
        this.Name = name;
        this.Duration = dur;
        this.Force = force;
        this.StartTime = Mathf.RoundToInt(Time.realtimeSinceStartup * 1000);
        this.pl = p;
        this.ParentBlock = pblock;
    }

    private void Start()
    {
        switch(this.Name)
        {
            case "stun":
                pl.canmove = false;
                break;
            case "invisibility":
                pl.visible = false;
                break;
            case "disability":
                pl.canuseobjects = false;
                break;
            case "resistance":
                pl.resistanceadd = this.Force;
                break;
            case "invincible":
                pl.invincible = true;
                break;
            case "web_slowness":
                break;
            default:
                break;
        }
    }

    public void Clear()
    {
        NetworkManager.log("Effect " + this.Name + " cleared for " + this.pl.Username, "PS");

        switch (this.Name)
        {
            case "stun":
                pl.canmove = true;
                break;
            case "invisibility":
                pl.visible = true;
                break;
            case "disability":
                pl.canuseobjects = true;
                break;
            case "resistance":
                pl.resistanceadd = 0;
                break;
            case "invincibility":
                pl.invincible = false;
                break;
            case "web_slowness":
                break;
            default:
                break;
        }

        this.pl.effects.Remove(this);
    }

    public bool Update()
    {
        if (this.Duration!=-1 && (Mathf.RoundToInt(Time.realtimeSinceStartup * 1000) - this.StartTime) >= this.Duration)
        {
            NetworkManager.log("Effect " + this.Name + " ended for " + this.pl.Username, "PS");

            switch (this.Name)
            {
                case "stun":
                    pl.canmove = true;
                    break;
                case "invisibility":
                    pl.visible = true;
                    break;
                case "disability":
                    pl.canuseobjects = true;
                    break;
                case "resistance":
                    pl.resistanceadd = 0;
                    break;
                case "invincibility":
                    pl.invincible = false;
                    break;
                default:
                    break;
            }

            this.pl.effects.Remove(this);
            return(true);
        } else
        {
            //NetworkManager.log("Still " + (this.Duration - (Time.realtimeSinceStartup - this.StartTime)).ToString() + "s of " + this.Name + " for " + this.pl.Username, "PS");
            return(false);
        }
    }
}
