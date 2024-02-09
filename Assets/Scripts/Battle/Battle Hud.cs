using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Gregomon _gregomon;
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Gregomon gregomon)
    {
        _gregomon = gregomon;
        nameText.text = gregomon.Base.Name;
        levelText.text = "lvl " + gregomon.Level;
        hpBar.SetHP((float)gregomon.HP / gregomon.MaxHp);

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor },
            {ConditionID.brn, brnColor },
            {ConditionID.slp, slpColor },
            {ConditionID.par, parColor },
            {ConditionID.frz, frzColor },
        };  

        SetStatusText();
        _gregomon.OnStatusChanged += SetStatusText;
    }

    void SetStatusText()
    {
        if (_gregomon.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _gregomon.Status.ID.ToString().ToUpper();
            statusText.color = statusColors[_gregomon.Status.ID];
        }
    }

    public IEnumerator UpdateHP()
    {
        if (_gregomon.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float)_gregomon.HP / _gregomon.MaxHp);
            _gregomon.HpChanged = false;
        }
        
    }
}
