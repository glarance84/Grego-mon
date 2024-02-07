using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyScript : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI messegeText;

    PartyMemberUI[] memberSlots;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyyData(List<Gregomon> gregomons)
    {
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < gregomons.Count)
                memberSlots[i].SetData(gregomons[i]);
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        messegeText.text = "Choose a Pokemon";
    }
}
