using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitchBehaviour : WeaponAnimationBehaviourBase
{
    public AnimationClip unequipTargetState;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        /* We want to change the animation we unequip with.
         * We'll then continue as normal.
        */
        if (!canExecute)
            return;

        controller.switchingWeapons = true;
        //controller.animator.ChangeEquipAnimation();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        if (canExecute && stateInfo.normalizedTime >= 0.99f)
        {
            controller.SwitchToWeaponIndex(controller.nextWeaponIndex);
            Debug.Log("Switched weapon");
        }
        controller.switchingWeapons = false;
    }
}
