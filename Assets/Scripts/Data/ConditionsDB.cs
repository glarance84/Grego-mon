using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;
            
            condition.ID = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Gregomon gregomon) =>
                {
                    gregomon.UpdateHP(gregomon.MaxHp / 8);
                    gregomon.StatusChanges.Enqueue($"{gregomon.Base.Name} is hurt due to poison");
                }
            }
        },
        { 
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Gregomon gregomon) =>
                {
                    gregomon.UpdateHP(gregomon.MaxHp / 16);
                    gregomon.StatusChanges.Enqueue($"{gregomon.Base.Name} is hurt due to burn");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Gregomon gregomon) =>
                {
                    if (Random.Range(1,5) == 1)
                    {
                        gregomon.StatusChanges.Enqueue($"{gregomon.Base.Name} is paralyzed and can't move");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Gregomon gregomon) =>
                {
                    if (Random.Range(1,5) == 1)
                    {
                        gregomon.CureStatus();
                        gregomon.StatusChanges.Enqueue($"{gregomon.Base.Name} is not frozen anymore");
                        return true;
                    }
                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (Gregomon gregomon) =>
                {
                    gregomon.StatusTime = Random.Range(1,4);
                    Debug.Log($"Will be asleep for {gregomon.StatusTime} moves");
                },
                OnBeforeMove = (Gregomon gregomon) =>
                {
                    if (gregomon.StatusTime <= 0)
                    {
                        gregomon.CureStatus();
                        gregomon.StatusChanges.Enqueue($"{gregomon.Base.Name} woke up!");
                        return true;
                    }

                    gregomon.StatusTime--;
                    gregomon.StatusChanges.Enqueue($"{gregomon.Base.Name} is sleeping");
                    return false;
                }
            }
        },
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "has been confused",
                OnStart = (Gregomon gregomon) =>
                {
                    gregomon.VolatileStatusTime = Random.Range(1,5);
                    Debug.Log($"Will be confused for {gregomon.StatusTime} moves");
                },
                OnBeforeMove = (Gregomon gregomon) =>
                {
                    if (gregomon.VolatileStatusTime <= 0)
                    {
                        gregomon.CureVolatileStatus();
                        gregomon.StatusChanges.Enqueue($"{gregomon.Base.Name} kicked out of confusion!");
                        return true;
                    }

                    gregomon.VolatileStatusTime--;

                    if(Random.Range(1,3) == 1)
                        return true;

                    gregomon.StatusChanges.Enqueue($"{gregomon.Base.Name} is confused");
                    gregomon.UpdateHP(gregomon.MaxHp / 8);
                    gregomon.StatusChanges.Enqueue("It hurt itself due to confusion");
                    return false;

                }
            }
        },
    };
}

public enum ConditionID
{
    none,psn, brn, slp, par, frz,
    confusion
}
