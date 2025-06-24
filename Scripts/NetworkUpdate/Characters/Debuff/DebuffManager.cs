using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DebuffManager : LunarNetScript
{
    public static DebuffManager Instance;

    public List<Debuff> debuffTemplates;
    public Dictionary<DebuffType, Debuff> debuffDictionary = new();


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            enabled = false;
            return;
        }



        //Debuffs will be overwritten by any duplicate debuff types.
        //If the developer includes multiple types, this will also warn them.

        for (int i = 0; i < debuffTemplates.Count; i++)
        {
            Debuff item = debuffTemplates[i];
            if (!debuffDictionary.ContainsKey(item.type))
            {
                debuffDictionary.TryAdd(item.type, item);
            }
            else
            {
                Debug.LogWarning($"Overwrote debuff of type {System.Enum.GetName(typeof(DebuffType), item.type)} at index {i} in debuff templates.\nVerify that debuff templates are set up correctly..");
                debuffDictionary[item.type] = item;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    private void OnValidate()
    {
        if (debuffTemplates.Count == 0)
            return;
        for (int i = 0; i < debuffTemplates.Count; i++)
        {
            debuffTemplates[i].name = Enum.GetName(typeof(DebuffType), debuffTemplates[i].type);
        }
    }
}
