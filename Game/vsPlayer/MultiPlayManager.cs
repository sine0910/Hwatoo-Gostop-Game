using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using Firebase.Extensions;

public class MultiPlayManager : CSingletonMonobehaviour<MultiPlayManager>
{
    ListenerRegistration listener;

    GameDataManger dataManger;
    MultiSendManager sendManager;

    public ChatManager chatManager;

    public Text per_point_text;

    string game_room_id;
    string my_account_id;
    string other_account_id;

    IEnumerator check_coroutine;

    public bool host = false;

    public int win_count;
    public int lose_count;
    public int nagari_count;

    public long before_money;
    public long get_money;

    public GameObject other_out_popup;

    public GameObject other_pocus_out_popup;
    public Text other_pocus_out_text;

    public GameObject on_error_popup;
    public GameObject out_reservation_popup;
    public GameObject cancle_out_reservation_popup;

    public GameObject canvas;

    List<PROTOCOL> game_room_protocol_list;

    long stack = 0;

    public InputField chatting_input;

    public GameObject chat_panel;
    public GameObject emoticon_panel;

    public Scrollbar scroll_bar;

    GameObject my_chat_prefab;
    GameObject other_chat_prefab;

    GameObject preview_prefab;
    public GameObject preview_panel;

    public Text preview_set_button_text;

    List<string> chat_id_list;

    public GameObject chat_alarm;
    public bool on_chat;

    public bool game_playing;

    bool exception_out = false;

    void Awake()
    {
        system_message = Resources.Load("Prefab/SystemMessage") as GameObject;

        dataManger = GameDataManger.instance;
        sendManager = MultiSendManager.instance;

        game_room_id = dataManger.game_room_id;
        host = dataManger.host;
        my_account_id = dataManger.get_player_acount_id(0);
        other_account_id = dataManger.get_player_acount_id(1);

        win_count = 0;
        lose_count = 0;
        nagari_count = 0;

        game_room_protocol_list = new List<PROTOCOL>();
        add_game_room_protocol_list();

        chat_id_list = new List<string>();

        my_chat_prefab = Resources.Load<GameObject>("Prefab/Chatting/MyChat");
        other_chat_prefab = Resources.Load<GameObject>("Prefab/Chatting/OtherChat");
        preview_prefab = Resources.Load<GameObject>("Prefab/Chatting/PreviewChat");

        Initialize();

        MultiSendManager.instance.send_manager_start(host);
        MultiGameUI.instance.ui_start();
    }

    public void Initialize()
    {
        if (host)
        {
            FirestoreHandleValueChangedForHost();
        }
        else
        {
            FirestoreHandleValueChangedForGuest();
        }

        StartCoroutine(FirestoreHandleValueChangedForChatting());

        if (!host)
        {
            ProtocolToHost("1");
        }

        CheckProtocolDelay();
    }

    public void CheckProtocolDelay()
    {
        Debug.Log("CheckProtocol");
        try
        {
            if (check_coroutine != null)
            {
                StopCoroutine(check_coroutine);
                check_coroutine = null;
            }
            check_coroutine = CheckingMessage();
            StartCoroutine(check_coroutine);
        }
        catch (Exception e)
        {
            Debug.Log("check_coroutine error " + e);
        }
    }

    public void ProtocolToGameRoom(string msg)//GameRoom에게 보내는 메세지
    {
        Debug.Log(game_room_id + " To GameRoom: " + msg + " time: " + DateTime.Now.ToString());
        DocumentReference docRef = FirebaseManager.instance.firestore.Collection("MultiRoom").Document(game_room_id).Collection("host").Document("protocol");
        Dictionary<string, object> user = new Dictionary<string, object>
        {
                { "value",  msg }
        };
        docRef.SetAsync(user);
        //    .ContinueWithOnMainThread(task =>
        //{
        //    if (task.IsCompleted)
        //    {
        //        Debug.Log("ProtocolToGameRoom task success! " + msg);
        //    }
        //    else
        //    {
        //        Debug.Log("ProtocolToGameRoom task failed! " + msg + "\nexc: " + task.Exception);
        //    }
        //});
    }

    public void ProtocolToHost(string msg)//호스트UI에게 보내는 메세지
    {
        Debug.Log(game_room_id + " To Host: " + msg + " time: " + DateTime.Now.ToString());
        DocumentReference docRef = FirebaseManager.instance.firestore.Collection("MultiRoom").Document(game_room_id).Collection("host").Document("protocol");
        Dictionary<string, object> user = new Dictionary<string, object>
        {
                { "value",  msg }
        };
        docRef.SetAsync(user);
    }

    public void ProtocolToGuest(string msg)//게스트UI에게 보내는 메세지
    {
        Debug.Log(game_room_id + " To Guest: " + msg + " time: " + DateTime.Now.ToString());
        DocumentReference docRef = FirebaseManager.instance.firestore.Collection("MultiRoom").Document(game_room_id).Collection("guest").Document("protocol");
        Dictionary<string, object> user = new Dictionary<string, object>
        {
                { "value",  msg }
        };
        docRef.SetAsync(user);
    }

    void FirestoreHandleValueChangedForHost()
    {
        Debug.Log("FirestoreHandleValueChangedForHost");

        //listener = FirebaseManager.instance.firestore.Collection("MultiRoom").Document(game_room_id).Collection("host").Document("protocol").Listen(MetadataChanges.Include, snapshot =>
        listener = FirebaseManager.instance.firestore.Collection("MultiRoom").Document(game_room_id).Collection("host").Document("protocol").Listen(snapshot =>
        {
            Debug.Log("FirestoreHandleValueChangedForHost get event");
            try
            {
                if (snapshot.Exists)
                {
                    Debug.Log("FirestoreHandleValueChangedForHost snapshot Exists");

                    Dictionary<string, object> city = snapshot.ToDictionary();
                    if (city.Count == 0)
                    {
                        Debug.Log("FirestoreHandleValueChangedForHost city count 0");
                    }

                    foreach (KeyValuePair<string, object> pair in city)
                    {
                        if (pair.Value != null && pair.Value.ToString() != "")
                        {
                            CheckProtocolDelay();

                            string value = pair.Value.ToString();
                            List<string> protocol_list = value.Split(new string[] { "/" }, StringSplitOptions.None).ToList();
                            Debug.Log("Receive Packet " + (PROTOCOL)Convert.ToInt32(protocol_list[0]) + "\nStack: " + stack);
                            stack++;

                            //게임룸으로 전달되는 프로토콜 구분
                            if (game_room_protocol_list.Contains((PROTOCOL)Convert.ToInt32(protocol_list[0])))
                            {
                                ProtocolToGameRoom("");

                                byte player_index = Convert.ToByte(PopAt(protocol_list));
                                sendManager.receive_game_room(player_index, protocol_list);
                            }
                            else//게임룸에 전달되는 프로토콜이 아닌경우(UI에 전달되는 프로토콜)
                            {
                                if (snapshot.Metadata.IsFromCache)
                                {
                                    Debug.Log("FirestoreHandleValueChangedForHost snapshot.Metadata.IsFromCache!");
                                }
                                else
                                {
                                    sendManager.receive_ui(protocol_list);
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("FirestoreHandleValueChangedForHost pair Value  is null!");
                        }
                    }
                }
                else
                {
                    Debug.Log("FirestoreHandleValueChangedForHost snapshot is null!");
                }
                //else
                //{
                //    Debug.Log("FirestoreHandleValueChangedForHost IsFromCache and pendingwrites");
                //    if (snapshot.Exists)
                //    {
                //        Dictionary<string, object> city = snapshot.ToDictionary();
                //        foreach (KeyValuePair<string, object> pair in city)
                //        {
                //            if (pair.Value != null)
                //            {
                //                string value = pair.Value.ToString();
                //                if (value == "1")
                //                {
                //                    //호스트가 리스닝 연결 전에 게스트가 보낸 게임 시작 프로토콜을 누락하여 게임이 시작하지 않는 현상을 방지하기 위함
                //                    List<string> protocol_list = new List<string>();
                //                    protocol_list.Add(value);
                //                    sendManager.receive_ui(protocol_list);
                //                }
                //                Debug.Log("FirestoreHandleValueChangedForHost from Cache! " + value + " time: " + DateTime.Now.ToString());
                //            }
                //        }
                //    }
                //    else
                //    {
                //        Debug.Log("FirestoreHandleValueChangedForHost  IsFromCache and pendingwrites snapshot is null!");
                //    }
                //}
                //
            }
            catch (Exception e)
            {
                Debug.Log("FirestoreHandleValueChangedForHost " + e);
            }
        });
    }

    void FirestoreHandleValueChangedForGuest()
    {
        Debug.Log("FirestoreHandleValueChangedForGuest");

        listener = FirebaseManager.instance.firestore.Collection("MultiRoom").Document(game_room_id).Collection("guest").Document("protocol").Listen(snapshot =>
        {
            try
            {
                //if (!snapshot.Metadata.IsFromCache)
                //{
                    if (snapshot.Exists)
                    {
                        Dictionary<string, object> city = snapshot.ToDictionary();
                        foreach (KeyValuePair<string, object> pair in city)
                        {
                            if (pair.Value != null && pair.Value.ToString() != "")
                            {
                                CheckProtocolDelay();

                                string value = pair.Value.ToString();
                                List<string> protocol_list = value.Split(new string[] { "/" }, StringSplitOptions.None).ToList();
#if FALSE
                            string guest_stack = PopAt(protocol_list);
                                Debug.Log("Receive Packet " + (PROTOCOL)Convert.ToInt32(protocol_list[0]) + "\nguest_stack: " + guest_stack + " Stack: " + stack);
#endif
                            stack++;

                                if (snapshot.Metadata.IsFromCache)
                                {
                                    Debug.Log("FirestoreHandleValueChangedForGuest snapshot.Metadata.IsFromCache!");
                                }
                                else
                                {
                                    sendManager.receive_ui(protocol_list);
                                }
                            }
                        }
                    }
                //}
                //else
                //{
                //    if (snapshot.Exists)
                //    {
                //        Dictionary<string, object> city = snapshot.ToDictionary();
                //        foreach (KeyValuePair<string, object> pair in city)
                //        {
                //            if (pair.Value != null)
                //            {
                //                string value = pair.Value.ToString();
                //                if (value == "0")
                //                {
                //                    List<string> protocol_list = new List<string>();
                //                    protocol_list.Add(value);
                //                    sendManager.receive_ui(protocol_list);
                //                }
                //                Debug.Log("FirestoreHandleValueChangedForGuest from Cache! " + value + " time: " + DateTime.Now.ToString());
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception e)
            {
                Debug.Log("FirestoreHandleValueChangedForGuest " + e);
            }
        });
    }

    bool error = false;
    public IEnumerator CheckingMessage()
    {
        error = true;
        yield return new WaitForSecondsRealtime(15f);
        //2021-05-29 09:58 If there is no message from the server suddenly for 15 seconds, it is judged as an error
        if (error && !other_out)
        {
            Debug.Log("CheckingMessage Over 15sc!");
            on_error();
        }
    }

    bool other_out = false;
    IEnumerator checking_pucus_out;
    public void other_pocus_out()
    {
        other_out = false;
        Time.timeScale = 0;
        other_pocus_out_popup.SetActive(true);
        checking_pucus_out = CheckingPocusOutTime();
        StartCoroutine(checking_pucus_out);
    }

    public IEnumerator CheckingPocusOutTime()
    {
        for (int count = 10; count > 0; count--)
        {
            other_pocus_out_text.text = count + "초 후 응답이 없을 경우 탈주로 취급됩니다";
            yield return new WaitForSecondsRealtime(1f);
        }

        other_out = true;
        other_pocus_out_popup.SetActive(false);
        on_other_out_popup();
    }

    public void other_pocus_on()
    {
        if (!other_out)
        {
            Time.timeScale = 1;
            StopCoroutine(checking_pucus_out);
            other_pocus_out_popup.SetActive(false);
        }
    }

    public void on_error()
    {
        exception_out = true;
        on_error_popup.SetActive(true);
        Time.timeScale = 0;
    }

    public void on_system_message(string msg_val)
    {
        var msg = Instantiate(system_message);
        msg.transform.parent = canvas.transform;
        msg.transform.localPosition = new Vector3(0, 0, 0);
        msg.GetComponent<SystemMessage>().on_message(msg_val);
    }

    public void on_other_out_popup()
    {
        other_out = true;

        //Check for breakouts during the game
        if (game_playing)
        {
            if (!exception_out)
            {
                //Penalties are given if there is no breakout due to an error.
            }
        }
        else
        {
            other_out_popup.SetActive(true);
        }
    }

    #region OUT RESERVATION
    public void on_out_reservation_popup()
    {
        if (out_reservation_status)
        {
            cancle_out_reservation_popup.SetActive(true);
        }
        else
        {
            out_reservation_popup.SetActive(true);
        }
    }

    public bool out_reservation_status = false;
    public void out_reservation()
    {
        if (out_reservation_status)
        {
            out_reservation_status = false;

            if (host)
            {
                ProtocolToGuest("901");
            }
            else
            {
                ProtocolToHost("901");
            }
            close_cancle_out_reservation_popup();
        }
        else
        {
            out_reservation_status = true;

            if (host)
            {
                ProtocolToGuest("900");
            }
            else
            {
                ProtocolToHost("900");
            }
            close_out_reservation_popup();
        }
    }

    public void close_out_reservation_popup()
    {
        out_reservation_popup.SetActive(false);
    }

    public void close_cancle_out_reservation_popup()
    {
        cancle_out_reservation_popup.SetActive(false);
    }
    #endregion

    public string PopAt(List<string> list)
    {
        string r = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        return r;
    }

    public void Home()
    {
        Time.timeScale = 1;

        if (host)
        {
            ProtocolToGuest("999");
        }
        else
        {
            ProtocolToHost("999");
        }

        try
        {
            if (listener != null)
            {
                Debug.Log("listener stop");
                listener.Stop();
            }
        }
        catch (Exception e)
        {
            Debug.Log("MultiPlayManager listener Stop error: " + e);
        }

        //move the scene
    }

    void add_game_room_protocol_list()
    {
        game_room_protocol_list.Add(PROTOCOL.READY_TO_START);
        game_room_protocol_list.Add(PROTOCOL.DISTRIBUTED_ALL_CARDS);
        game_room_protocol_list.Add(PROTOCOL.BONUS_START);
        game_room_protocol_list.Add(PROTOCOL.SELECT_CARD_REQ);
        game_room_protocol_list.Add(PROTOCOL.CHOOSE_CARD);
        game_room_protocol_list.Add(PROTOCOL.FLIP_BOMB_CARD_REQ);
        game_room_protocol_list.Add(PROTOCOL.FLIP_BOMB_BONUS_CARD_REQ);
        game_room_protocol_list.Add(PROTOCOL.FLIP_DECK_CARD_REQ);
        game_room_protocol_list.Add(PROTOCOL.FLIP_DECK_BONUS_CARD_REQ);
        game_room_protocol_list.Add(PROTOCOL.TURN_END);
        game_room_protocol_list.Add(PROTOCOL.BONUS_TURN);
        game_room_protocol_list.Add(PROTOCOL.ANSWER_KOOKJIN_TO_PEE);
        game_room_protocol_list.Add(PROTOCOL.ANSWER_GO_OR_STOP);
        game_room_protocol_list.Add(PROTOCOL.SET_START_USER);
        game_room_protocol_list.Add(PROTOCOL.READY_TO_USER_SELET_START);
        game_room_protocol_list.Add(PROTOCOL.SET_START_USER_CARD_REQ);
        game_room_protocol_list.Add(PROTOCOL.SET_START_USER_TURN_END);
    }

    #region CHATTING
    public Queue<Chat> chat_list = new Queue<Chat>();

    bool emoticon = true;
    public void on_emoticon_panel()
    {
        if (!emoticon)
        {
            chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().transform.localPosition = new Vector3(chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().transform.localPosition.x, 25);
            chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().sizeDelta = new Vector3(chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().sizeDelta.x, 250);

            emoticon_panel.SetActive(true);
            emoticon = true;
        }
        else
        {
            chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().transform.localPosition = new Vector3(chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().transform.localPosition.x, -25);
            chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().sizeDelta = new Vector3(chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().sizeDelta.x, 350);

            emoticon_panel.SetActive(false);
            emoticon = false;
        }

        scroll_to_bottom();
    }

    public void send_chattng_message()
    {
        string msg = chatting_input.text;

        if (msg.Trim() != "")
        {
            chatting_input.text = "";

            send_to_chatting(msg);
        }
    }

    public void send_to_chatting(string msg)
    {
        WriteBatch batch = FirebaseManager.instance.firestore.StartBatch();

        DocumentReference my_ref = FirebaseManager.instance.firestore.Collection("MultiChatRoom").Document(my_account_id + other_account_id).Collection("chatting").Document(TimeStamp.GetUnixTimeStamp());
        Dictionary<string, object> my_msg = new Dictionary<string, object>
        {
            { "msg",  msg },
            { "player", 0 },
            { "time", TimeStamp.GetUnixTimeStamp() }
        };
        batch.Set(my_ref, my_msg);

        DocumentReference other_ref = FirebaseManager.instance.firestore.Collection("MultiChatRoom").Document(other_account_id + my_account_id).Collection("chatting").Document(TimeStamp.GetUnixTimeStamp());
        Dictionary<string, object> other_msg = new Dictionary<string, object>
        {
            { "msg",  msg },
            { "player", 1 },
            { "time", TimeStamp.GetUnixTimeStamp() }
        };
        batch.Set(other_ref, other_msg);

        batch.CommitAsync();
    }

    IEnumerator FirestoreHandleValueChangedForChatting()
    {
        Debug.Log("FirestoreHandleValueChangedForChatting");

        bool ready = false;

        FirebaseManager.instance.firestore.Collection("MultiChatRoom").Document(my_account_id + other_account_id).Collection("chatting").OrderBy("time").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                ready = true;
            }
            else
            {
                QuerySnapshot snapshot = task.Result;
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (!chat_id_list.Contains(document.Id))
                    {
                        chat_id_list.Add(document.Id);

                        Dictionary<string, object> pairs = document.ToDictionary();

                        byte player_index = Convert.ToByte(pairs["player"].ToString());
                        string msg = pairs["msg"].ToString();

                        chat_list.Enqueue(new Chat(player_index, msg));

                        set_chatting_page();
                    }
                }
                ready = true;
            }
        });

        yield return new WaitUntil(() => ready);

        FirebaseManager.instance.firestore.Collection("MultiChatRoom").Document(my_account_id + other_account_id).Collection("chatting").Listen(snapshot =>
        {
            if (!snapshot.Metadata.IsFromCache)
            {
                foreach (DocumentChange change in snapshot.GetChanges())
                {
                    if (change.ChangeType == DocumentChange.Type.Added)
                    {
                        if (!chat_id_list.Contains(change.Document.Id))
                        {
                            chat_id_list.Add(change.Document.Id);

                            Dictionary<string, object> pairs = change.Document.ToDictionary();

                            if (pairs.ContainsKey("player") && pairs.ContainsKey("msg"))
                            {
                                byte player_index = Convert.ToByte(pairs["player"].ToString());
                                string msg = pairs["msg"].ToString();

                                chat_list.Enqueue(new Chat(player_index, msg));

                                set_chatting_page();

                                if (!on_chat)
                                {
                                    chat_alarm.SetActive(true);
                                }
                            }
                        }
                    }
                }
            }
        });
    }

    public void set_chatting_page()
    {
        try
        {
            Chat chat_data = chat_list.Dequeue();

            if (chat_data.player_index == 0)
            {
                AreaScript chat = Instantiate(my_chat_prefab).GetComponent<AreaScript>();
                chat.transform.parent = chat_panel.transform.Find("ScrollPage/View/Panel");
                chat.transform.localScale = new Vector3(1, 1, 1);
                chatManager.chat_List.Add(chat);
                chat.set_msg(chat_data.msg);
            }
            else
            {
                AreaScript chat = Instantiate(other_chat_prefab).GetComponent<AreaScript>();
                chat.transform.parent = chat_panel.transform.Find("ScrollPage/View/Panel");
                chat.transform.localScale = new Vector3(1, 1, 1);
                chatManager.chat_List.Add(chat);
                chat.set_msg(chat_data.msg);

                if (!on_chat && DataManager.instance.preview == 1)
                {
                    PreviewChat preview = Instantiate(preview_prefab).GetComponent<PreviewChat>();
                    preview.transform.parent = preview_panel.transform;
                    preview.transform.localScale = new Vector3(1, 1, 1);
                    preview.transform.localPosition = new Vector3(0, 0, 0);
                    preview.set(chat_data.msg);
                }
            }
            chatManager.ChatSort();

            scroll_to_bottom();
        }
        catch (Exception e)
        {
            Debug.Log("set_chatting_page error: " + e);
        }
    }

    public void scroll_to_bottom()
    {
        Canvas.ForceUpdateCanvases();
        chat_panel.transform.Find("ScrollPage/View/Panel").transform.GetComponent<VerticalLayoutGroup>().enabled = false;
        chat_panel.transform.Find("ScrollPage/View/Panel").transform.GetComponent<VerticalLayoutGroup>().enabled = true;

        scroll_bar.value = 0;
    }

    public void on_chatting_page()
    {
        Debug.Log("on_chatting_page");

        if (emoticon)
        {
            chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().transform.localPosition = new Vector3(chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().transform.localPosition.x, 25);
            chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().sizeDelta = new Vector3(chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().sizeDelta.x, 250);
        }
        else
        {
            chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().transform.localPosition = new Vector3(chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().transform.localPosition.x, -25);
            chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().sizeDelta = new Vector3(chat_panel.transform.Find("ScrollPage").GetComponent<RectTransform>().sizeDelta.x, 350);
        }

        scroll_to_bottom();
    }

    public void close_chatting_page()
    {
        on_chat = false;
        chatting_input.text = "";
        chat_panel.SetActive(false);
    }

    public void select_preview()
    {
        if (DataManager.instance.preview == 1)
        {
            DataManager.instance.preview = 2;
        }
        else if (DataManager.instance.preview == 2)
        {
            DataManager.instance.preview = 1;
        }
        DataManager.instance.save_preview_data();
    }

    public class Chat
    {
        public byte player_index;
        public string msg;

        public Chat(byte player_index, string msg)
        {
            this.player_index = player_index;
            this.msg = msg;
        }
    }
    #endregion

    DateTime out_time;
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            out_time = DateTime.Now;

            if (host)
            {
                ProtocolToGuest("990");
            }
            else
            {
                ProtocolToHost("990");
            }
        }
        else
        {
            if (out_time.AddSeconds(15) < DateTime.Now)
            {
                exception_out = true;
            }

            if (host)
            {
                ProtocolToGuest("991");
            }
            else
            {
                ProtocolToHost("991");
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (host)
        {
            ProtocolToGuest("999");
        }
        else
        {
            ProtocolToHost("999");
        }

        if (win_count != 0 || lose_count != 0 || nagari_count != 0 && get_money != 0)
        {
            FirebaseManager.instance.send_play_history(win_count, lose_count, nagari_count, get_money, before_money, "사용자 대전");
        }

        stop_record();
    }

    private void OnDestroy()
    {
        //try
        //{
        //    if (listener != null)
        //    {
        //        listener.Stop();
        //    }
        //}
        //catch (Exception e)
        //{
        //    Debug.Log("MultiPlayManager listener Stop error: " + e);
        //}
    }
}
