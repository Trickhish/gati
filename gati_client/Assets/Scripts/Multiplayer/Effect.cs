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
        this.pl = GetComponent<Player>();
    }

    private void Start() // #addeffect
    {
        switch (this.Name)
        {
            case "stun":
                break;
            default:
                break;
        }
    }

    void Update()
    {
        if ((Time.realtimeSinceStartup - this.StartTime) >= this.Duration)
        {
            this.pl.effects.Remove(this);
            Destroy(this);
        }
    }
}
