using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonPart : MonoBehaviour
{
    [SerializeField] List<Gregomon> gregomons;

    public List<Gregomon> Gregomons
    {
        get { return gregomons; }
    }

    private void Start()
    {
        foreach (var gregomon in gregomons)
        {
            gregomon.Init();
        }
    }

    public Gregomon GetHealthyPokemon()
    {
        return gregomons.Where(x => x.HP > 0).FirstOrDefault();
    }

}
