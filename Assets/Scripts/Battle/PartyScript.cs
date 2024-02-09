using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyScript : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI messegeText;

    PartyMemberUI[] memberSlots;
    List<Gregomon> gregomons;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyyData(List<Gregomon> gregomons)
    {
        this.gregomons = gregomons;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < gregomons.Count)
                memberSlots[i].SetData(gregomons[i]);
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        messegeText.text = "Choose a Pokemon";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for  (int i = 0;i < gregomons.Count;i++)
        {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    public void SetMessageText(string message)
    {
        messegeText.text = message;
    }
}
