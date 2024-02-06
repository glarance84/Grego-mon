using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Gregomon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] new string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] GregomonBase.GregomonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;

    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return Description; }
    }

    public GregomonBase.GregomonType Type
    {
        get { return type; }
    }

    public int Power
    {
        get { return power; }
    }

    public int Accuracy
    {
        get { return accuracy; }
    }

    public int PP
    {
        get { return pp; }
    }

    public bool IsSpecial
    {
        get
        {
            if (type == GregomonBase.GregomonType.Fire || type == GregomonBase.GregomonType.Water || type == GregomonBase.GregomonType.Grass || type == GregomonBase.GregomonType.Ice || type == GregomonBase.GregomonType.Electric || type == GregomonBase.GregomonType.Dragon)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
