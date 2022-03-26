using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardCollision : MonoBehaviour
{
    public delegate void TouchFunc(CardPicture card_picture);
    public TouchFunc callback_on_touch = null;

    void Update()
    {
        //UI�� ������ ī���� ��� ������ �� ���� �����Ѵ�.
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                GameObject obj = hit.transform.gameObject;
                //ī�� �±׸� �����ִ� ������Ʈ���� �����Ѵ�. 
                if (obj.CompareTag("card"))
                {
                    CardPicture card_picture = obj.GetComponent<CardPicture>();
                    if (card_picture == null || this.callback_on_touch == null)
                    {
                        return;
                    }
                    //���� ī�带 ��������Ʈ�� ������ ������� ��ȯ���ش�.
                    this.callback_on_touch(card_picture);
                }
            }
        }
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
