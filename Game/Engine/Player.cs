using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player 
{
    public byte player_index { get; private set; }
    public PlayerAgent agent { get; private set; }

    public Player(byte player_index)
    {
        this.player_index = player_index;
        this.agent = new PlayerAgent(player_index);
    }
}
