using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] GregomonBase _base;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;

    public Gregomon Gregomon { get; set; }

    public void Setup()
    {
        Gregomon = new Gregomon(_base, level);
        if (isPlayerUnit)
        {
            GetComponent<Image>().sprite = Gregomon.Base.BackSprite;
        }
        else
        {
            GetComponent<Image>().sprite = Gregomon.Base.FrontSprite;
        }
    }
}
