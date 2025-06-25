using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ReloadInfo
{
    public int ammoToTriggerAbove;
    public int ammoToTriggerBelow;

    public float reloadTime;

    public bool reloadFully;

    public int amountToReload;

    public bool isCountedReload;
    public int countedReloadIndex;
    public string reloadTrigger;

}


[CreateAssetMenu(menuName = "Scriptable Objects/Reload Config")]
public class ReloadConfigScriptable : ScriptableObject
{
    public ReloadInfo[] reloads;


    public ReloadInfo GetReloadInfo(float ammo)
    {
        for (int i = 0; i < reloads.Length; i++)
        {
            if(ammo > reloads[i].ammoToTriggerBelow && ammo < reloads[i].ammoToTriggerAbove)
            {
                return reloads[i];
            }
        }
        return reloads[0];
    }
}
