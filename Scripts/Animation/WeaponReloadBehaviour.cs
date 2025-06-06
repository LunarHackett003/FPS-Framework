using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponReloadBehaviour : WeaponAnimationBehaviourBase
{
    public float reloadAtTime = 1;
    public bool emptyReload;

    protected bool reloaded;
    protected float normalisedTimeForReload = 0.5f;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        if (!canExecute)
        {
            weapon.onReloadEvent?.Invoke(emptyReload, false);
            return;
        }

        Debug.Log($"reload time : {reloadAtTime}, state duration: {stateInfo.length}", animator.gameObject);
        reloadAtTime = emptyReload ? controller.CurrentWeapon.emptyReloadTime : controller.CurrentWeapon.partialReloadTime;
        normalisedTimeForReload = Mathf.InverseLerp(0, stateInfo.length, reloadAtTime);

        reloaded = false;
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (!reloaded && stateInfo.normalizedTime >= normalisedTimeForReload)
        {
            Debug.Log("attempted to reload weapon", weapon);
            reloaded = true;
            weapon.ReloadWeapon(true);
            weapon.onWeaponReloaded?.Invoke();
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        if (!canExecute && stateInfo.normalizedTime < 0.9f)
        {
            weapon.onReloadEvent?.Invoke(emptyReload, true);
            return;
        }
    }
}
