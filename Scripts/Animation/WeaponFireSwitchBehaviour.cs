using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponFireSwitchBehaviour : WeaponAnimationBehaviourBase
{
    RangedNetWeapon rweapon;
    bool switched;
    public float timeToSwitch;
    float normTime;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (!canExecute)
            return;
        rweapon = (RangedNetWeapon)weapon;
        if(rweapon != null)
        {
            timeToSwitch = rweapon.fireModeSwitchTime;
            normTime = Mathf.InverseLerp(0, stateInfo.length, timeToSwitch);
            switched = false;
        }
        else
        {
            Debug.Log("Should not have entered this state with current rweapon, re-evaluate. Stupid dev >:|");
        }
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if(canExecute && !switched && stateInfo.normalizedTime >= normTime)
        {
            rweapon.IncrementFireMode();
            switched = true;
        }
    }
}
