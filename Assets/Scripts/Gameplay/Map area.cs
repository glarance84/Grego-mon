using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maparea : MonoBehaviour
{
    [SerializeField] List<Gregomon> wildGregomons;

    public Gregomon GetRandomWildGregomon()
    {
        var wildGregomon = wildGregomons[Random.Range(0, wildGregomons.Count)];
        wildGregomon.Init();
        return wildGregomon;
    }
}

