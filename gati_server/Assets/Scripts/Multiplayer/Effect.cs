using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public string Name {get;set;}
    public float StartTime {get;set;}
    public float Duration {get;set;}
    public Player pl {get;set;}
    public float Force {get;set;}

    public Effect(string name, float dur, float force=1)
    {
        this.Name = name;
        this.Duration = dur;
        this.Force = force;
        this.StartTime = Time.realtimeSinceStartup;
        this.pl = GetComponent<Player>();
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

    void Update()
    {
        if ((Time.realtimeSinceStartup-this.StartTime) >= this.Duration)
        {
            this.pl.effects.Remove(this);
            Destroy(this);
        }
    }
}
