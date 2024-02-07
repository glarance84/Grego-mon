using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;

    Gregomon _gregomon;

    public void SetData(Gregomon gregomon)
    {
        _gregomon = gregomon;
        nameText.text = gregomon.Base.Name;
        levelText.text = "lvl " + gregomon.Level;
        hpBar.SetHP((float)gregomon.HP / gregomon.MaxHp);
    }
}
