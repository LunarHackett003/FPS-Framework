using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitchBehaviour : WeaponAnimationBehaviourBase
{
    public AnimationClip unequipTargetState;
    bool executed = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        /* We want to change the animation we unequip with.
         * We'll then continue as normal.
        */
        if (!canExecute)
            return;
        executed = false;
        controller.switchingWeapons = true;
        //controller.animator.ChangeEquipAnimation();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        if (executed || !canExecute)
            return;
        if (canExecute && stateInfo.normalizedTime >= 0.99f)
        {
            controller.SwitchToWeaponIndex(controller.nextWeaponIndex);
            Debug.Log("Switched weapon");
            executed = true;
        }
        controller.switchingWeapons = false;
    }
}
