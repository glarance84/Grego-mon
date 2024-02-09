using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Condition
{

    public ConditionID ID {  get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage {  get; set; }

    public Action<Gregomon> OnStart { get; set; }
    public Func<Gregomon, bool> OnBeforeMove { get; set; }

    public Action<Gregomon> OnAfterTurn { get; set; }
}
