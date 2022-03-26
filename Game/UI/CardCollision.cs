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
        //UI에 가려진 카드의 경우 선택할 수 없게 방지한다.
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                GameObject obj = hit.transform.gameObject;
                //카드 태그를 갖고있는 오브젝트에만 반응한다. 
                if (obj.CompareTag("card"))
                {
                    CardPicture card_picture = obj.GetComponent<CardPicture>();
                    if (card_picture == null || this.callback_on_touch == null)
                    {
                        return;
                    }
                    //정한 카드를 델리게이트로 선언한 펑션으로 반환해준다.
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
