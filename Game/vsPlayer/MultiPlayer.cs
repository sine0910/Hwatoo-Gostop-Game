using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MULTI_PLAYER_TYPE : byte
{
    HOST,
    GUEST
}

public class MultiPlayer : MonoBehaviour
{
    public delegate void SendFn(List<string> msg);

    SendFn send_function;

    public byte player_index { get; private set; }
    public PlayerAgent agent { get; private set; }

    public MultiPlayer(byte player_index, MULTI_PLAYER_TYPE player_type, SendFn send_function, MultiGameRoom room)
    {
        this.player_index = player_index;
        this.agent = new PlayerAgent(player_index);

        switch (player_type)
        {
            case MULTI_PLAYER_TYPE.HOST:
                this.send_function = send_function;
                break;

            case MULTI_PLAYER_TYPE.GUEST:
                this.send_function = send_function;
                break;
        }
    }

    public void send(List<string> msg)
    {
        List<string> clone = msg.ToList();
        this.send_function(msg);
    }
}
