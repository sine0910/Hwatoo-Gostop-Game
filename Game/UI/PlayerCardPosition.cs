using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardPosition : MonoBehaviour
{   
    // 플레이어가 먹은 바닥패의 위치.
    Dictionary<PAE_TYPE, Vector3> floor_positions;

    // 플레이어가 손에 들고 있는 패의 위치.
    List<Vector3> hand_positions;

    void Start()
    {
        List<Vector3> targets = new List<Vector3>();
        make_slot_positions(transform.Find("floor"), targets);
        this.floor_positions = new Dictionary<PAE_TYPE, Vector3>();
        this.floor_positions.Add(PAE_TYPE.KWANG, targets[0]);
        this.floor_positions.Add(PAE_TYPE.YEOL, targets[1]);
        this.floor_positions.Add(PAE_TYPE.TEE, targets[2]);
        this.floor_positions.Add(PAE_TYPE.PEE, targets[3]);

        this.hand_positions = new List<Vector3>();
        make_slot_positions(transform.Find("hand"), this.hand_positions);
    }

    void make_slot_positions(Transform root, List<Vector3> targets)
    {
        Transform[] slots = root.GetComponentsInChildren<Transform>();
        for (int i = 0; i < slots.Length; ++i)
        {
            if (slots[i] == root)
            {
                continue;
            }

            targets.Add(slots[i].position);
        }
    }

    public Vector3 get_floor_position(int card_count, PAE_TYPE pae_type)
    {
        return this.floor_positions[pae_type] + new Vector3(card_count * 12, 0, 0);
    }

    public Vector3 get_hand_position(int slot_index)
    {
        //Debug.Log("get_hand_position " + slot_index);
        if (this.hand_positions.Count - 1 > slot_index)
        {
            return this.hand_positions[slot_index];
        }
        else
        {
            return this.hand_positions[this.hand_positions.Count - 1];
        }
    }

    public int get_hand_slot()
    {
        return this.hand_positions.Count - 1;
    }
}
