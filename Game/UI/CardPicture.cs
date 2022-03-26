using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPicture : MonoBehaviour
{
    public Card card { get; private set; }
    public SpriteRenderer sprite_renderer { get; private set; }

    public byte slot { get; private set; }

    BoxCollider box_collider;

    void Awake()
    {
        this.sprite_renderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        this.box_collider = gameObject.GetComponent<BoxCollider>();
    }

    public void set_slot_index(byte slot)
    {
        this.slot = slot;
    }

    public void update_card(Card card, Sprite image)
    {
        this.card = card;
        this.sprite_renderer.sprite = image;
    }

    public void update_backcard(Sprite back_image)
    {
        this.card = null;
        update_image(back_image);
    }

    public void update_image(Sprite image)
    {
        this.sprite_renderer.sprite = image;
    }

    public void enable_collider(bool flag)
    {
        this.box_collider.enabled = flag;
    }

    public bool is_same(byte number, PAE_TYPE pae_type, byte position)
    {
        return this.card.is_same_card(number, pae_type, position);
    }

    public bool is_back_card()
    {
        return this.card == null;
    }
}
