using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiSendManager : CSingletonMonobehaviour<MultiSendManager>
{
    static MultiPlayManager playManager;

    MultiGameRoom gameRoom;
    MultiGameUI gameUI;

    static bool host = false;

    //MultiPlayManager���� �⺻���� ������ �ΰ谡 ���� ���� ȣ��Ǿ����
    //Awake�Լ����� MultiPlayManager�� ȣ���ϴ� �Լ��� ����
    public void send_manager_start(bool host_value)
    {
        Debug.Log("MultiSendManager Start");
        playManager = gameObject.GetComponent<MultiPlayManager>();
        if (host_value)//MultiPlayManager�� host���� ������ �ڽ��� host������ �ް�, �α׷� Ȯ���� �� �ֵ��� �Ѵ�. 
        {
            Debug.Log("MultiSendManager host");
            host = true;
            gameRoom = new MultiGameRoom(this);
        }
        else
        {
            Debug.Log("MultiSendManager guest");
        }
    }

    public void on_start(MultiGameUI ui)
    {
        Debug.Log("MultiSendManager on start");
        gameUI = ui;
    }

    public void local_server_start()
    {
        Debug.Log("MultiSendManager local_server_start");
        gameRoom.on_ready_start();
    }

    public void send_to_host(List<string> msg)
    {
        Debug.Log("send_to_host_ui " + (PROTOCOL)Convert.ToInt32(msg[0]));
        //string s_msg = "";
        //for (int i = 0; i < msg.Count; i++)
        //{
        //    s_msg += msg[i];
        //    if (i < msg.Count - 1)
        //    {
        //        s_msg += "/";
        //    }
        //}
        //playManager.ProtocolToHost(s_msg);

        //ȣ��Ʈ�� UI�����ʹ� ���ÿ��� ���� �����Ѵ�.
        List<string> clone = msg.ToList();
        receive_ui(clone);
    }

    public void send_to_guest(List<string> msg)
    {
        Debug.Log("send_to_guest_ui " + (PROTOCOL)Convert.ToInt32(msg[0]));
        string s_msg = "";
        for (int i = 0; i < msg.Count; i++)
        {
            s_msg += msg[i];
            if (i < msg.Count - 1)
            {
                s_msg += "/";
            }
        }
        playManager.ProtocolToGuest(s_msg);
    }

    public void receive_ui(List<string> msg)
    {
        Debug.Log("receive_ui " + (PROTOCOL)Convert.ToInt32(msg[0]));
        RecordManager.instance.save_record(msg);
        gameUI.on_receive(msg);
    }

    public void send_to_game_room(byte player_index, List<string> msg)
    {
        Debug.Log("send_to_game_room " + (PROTOCOL)Convert.ToInt32(msg[0]));

        if (!host)
        {
            string s_msg = "";
            for (int i = 0; i < msg.Count; i++)
            {
                s_msg += msg[i] + "/";
            }
            s_msg += player_index;
            playManager.ProtocolToGameRoom(s_msg);
        }
        else
        {
            List<string> clone = msg.ToList();
            receive_game_room(player_index, clone);
        }
    }

    public void receive_game_room(byte player, List<string> msg)
    {
        Debug.Log("receive_game_room " + (PROTOCOL)Convert.ToInt32(msg[0]));
        List<string> clone = msg.ToList();
        gameRoom.on_receive(player, clone);
    }
}
