using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISendManager : MonoBehaviour
{
    static AIGameRoom gameRoom;
    AIGameUI gameUI;

    public void Awake()
    {
        Debug.Log("AISendManager Start");
        gameRoom = new AIGameRoom(this);
    }

    public void on_start(AIGameUI ui)
    {
        Debug.Log("AISendManager on start");
        gameUI = ui;

        gameRoom.on_ready_start();
    }

    public static void send_from_ai(List<string> msg)
    {
        Debug.Log("send_from_ai " + msg);
        gameRoom.on_receive(1, msg);
    }

    public static void send_from_player(List<string> msg)
    {
        Debug.Log("send_from_player " + msg);
        gameRoom.on_receive(0, msg);
    }

    public void send_to_ui(List<string> msg)
    {
        Debug.Log("send_to_ui " + msg);
        RecordManager.instance.save_record(msg);
        gameUI.on_recive(msg);
    }
}
