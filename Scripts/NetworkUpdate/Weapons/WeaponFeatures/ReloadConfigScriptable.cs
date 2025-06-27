using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ReloadInfo
{
    [Tooltip("Ammo must be greater than or equal to this value to trigger this reload.")]
    public int ammoToTriggerAbove;
    [Tooltip("Ammo must be less than or equal to this value to trigger this reload.")]
    public int ammoToTriggerBelow;

    [Tooltip("How long after which, in seconds, the weapon should be reloaded")]
    public float reloadTime;

    [Tooltip("Should the weapon's ammo be reloaded fully?")]
    public bool reloadFully;

    [Tooltip("How many rounds should be reloaded if \"Reload Fully\" is false")]
    public int amountToReload;
    [Tooltip("Is this animation a \"Counted Reload\"")]
    public bool isCountedReload;
    [Range(-1, 2), Tooltip("Which animation for counted reloads should be triggered")]
    public int countedReloadIndex;
    [Tooltip("If not empty, which animation should be triggered by this animation")]
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
            if(ammo <= reloads[i].ammoToTriggerBelow && ammo >= reloads[i].ammoToTriggerAbove)
            {
                //Debug.Log($"reload {i} met reload condition");
                return reloads[i];
            }
        }
        //Debug.Log("Nothing met reload condition");
        return reloads[0];
    }
}
