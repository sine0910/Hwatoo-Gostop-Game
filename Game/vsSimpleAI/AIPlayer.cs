using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum PLAYER_TYPE : byte
{
    HUMAN,
    AI
}

public class AIPlayer
{
    public delegate void SendFn(List<string> msg);

    SendFn send_function;

    public byte player_index { get; private set; }
    public PlayerAgent agent { get; private set; }

    AIBrain ai_brain;

    public AIPlayer(byte player_index, PLAYER_TYPE player_type, SendFn send_function, AIGameRoom room)
    {
        this.player_index = player_index;
        this.agent = new PlayerAgent(player_index);

        switch (player_type)
        {
            case PLAYER_TYPE.HUMAN:
                this.send_function = send_function;
                break;

            case PLAYER_TYPE.AI:
                this.ai_brain = new AIBrain(room);
                this.send_function = this.ai_brain.send;
                break;
        }
    }

    public void send(List<string> msg)
    {
        List<string> clone = msg.ToList();
        this.send_function(msg);
    }
}
