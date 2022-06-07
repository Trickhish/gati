using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectBlock
{
    public string EffectName { get; set; }
    public float EffectDuration { get; set; }
    public Vector2 BlockPosition { get; set; }
    public float BlockDuration {get;set;}
    public float StartTime { get; set; }
    public float BlockRadius {get;set;}
    public Player Owner { get; set; }
    public Match ParentMatch { get; set; }
    public string BlockName { get; set; }
    public float X
    {
        get { return BlockPosition.x; }
        set { BlockPosition = new Vector2(value, BlockPosition.y); }
    }
    public float Y
    {
        get { return BlockPosition.y; }
        set { BlockPosition = new Vector2(BlockPosition.x, value); }
    }

    public EffectBlock(Match match, string blockname, float x, float y, float blockdur, float blockrad, Player owner, string effectname, float effectdur)
    {
        this.BlockName = blockname;
        this.EffectName = effectname;
        this.EffectDuration = effectdur;
        this.BlockPosition = new Vector2(x, y);
        this.BlockDuration = blockdur;
        this.StartTime = Time.realtimeSinceStartup*1000;
        this.BlockRadius = blockrad;
        this.Owner = owner;
        this.ParentMatch = match;

        SendToPlayers();
    }

    public void SendToPlayers()
    {
        foreach(Player pl in this.ParentMatch.players.Values)
        {
            Message msg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.effectblock);
            msg.AddBool(true);
            msg.AddVector2(this.BlockPosition);
            msg.AddString(this.BlockName);
            NetworkManager.Singleton.Server.Send(msg, pl.Id);
        }
    }

    public bool AffectPlayer(Player pl)
    {
        if (Vector3.Distance(pl.position, this.BlockPosition)<this.BlockRadius && pl!=this.Owner)
        {
            NetworkManager.log(pl.Username+" is affected by "+this.BlockName, "PA");
            bool ctn = false;
            foreach(Effect ef in pl.effects)
            {
                if (ef.Name == this.EffectName && ef.ParentBlock==this)
                {
                    ctn = true;
                    ef.Duration = this.EffectDuration;
                    ef.StartTime = Time.realtimeSinceStartup * 1000;
                    pl.Affect(ef);
                    break;
                }
            }

            if (!ctn)
            {
                pl.Affect(new Effect(pl, this.EffectName, this.EffectDuration));
            }
            
            return (true);
        } else
        {
            foreach (Effect ef in pl.effects)
            {
                if (ef.Name == this.EffectName && ef.ParentBlock == this)
                {
                    ef.Clear();
                    NetworkManager.log(pl.Username + " is no longer affected by " + this.BlockName, "PA");
                    break;
                }
            }
            return(false);
        }
    }

    public bool AffectPlayer(ushort pid)
    {
        return(Player.plist.ContainsKey(pid) && this.AffectPlayer(Player.plist[pid]));
    }

    public void Remove()
    {
        this.ParentMatch.EffectBlocks.Remove(this);
    }

    public static void RemoveAll(Match m)
    {
        m.EffectBlocks.Clear();
    }

    public static void UpdateAll(Match m)
    {
        int i = 0;
        while (i < m.EffectBlocks.Count)
        {
            EffectBlock eb = m.EffectBlocks[i];
            if (!eb.Update())
            {
                i++;
            }
        }
    }

    public bool Update()
    {
        if (this.BlockDuration == -1)
        {
            return(false);
        }

        if ((Time.realtimeSinceStartup*1000)-this.StartTime > this.BlockDuration)
        {
            this.Remove();
            return(true);
        }

        return(false);
    }
}
