using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponReloadBehaviour : WeaponAnimationBehaviourBase
{
    public float reloadAtTime = 1;
    public bool emptyReload;

    [SerializeField]
    protected bool reloaded;
    [SerializeField]
    protected float normalisedTimeForReload = 0.5f;

    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        if (!canExecute)
        {
            weapon.onReloadEvent?.Invoke(emptyReload, false);
            return;
        }
        normalisedTimeForReload = Mathf.InverseLerp(0, stateInfo.length, weapon.reloadTime);
        reloaded = false;
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (!canExecute)
            return;

        //Hopefully this will reset the loop functionality :/
        if(LoopTime< normalisedTimeForReload && reloaded)
        {
            Debug.Log("Reset reload state on state machine");
            normalisedTimeForReload = Mathf.InverseLerp(0, stateInfo.length, weapon.reloadTime);
            reloaded = false;
        }

        if (!reloaded && LoopTime >= normalisedTimeForReload)
        {
            Debug.Log("attempted to reload weapon", weapon);
            reloaded = true;
            weapon.ReloadWeapon(true);
            weapon.onWeaponReloaded?.Invoke();


            if (weapon.isCountedReload)
            {
                weapon.UpdateCountedReload();
            }
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        if (!canExecute && LoopTime < 0.9f)
        {
            weapon.onReloadEvent?.Invoke(emptyReload, true);
            return;
        }
    }
}
