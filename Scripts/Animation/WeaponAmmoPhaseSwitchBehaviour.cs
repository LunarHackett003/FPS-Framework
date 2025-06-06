using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAmmoPhaseSwitchBehaviour : WeaponAnimationBehaviourBase
{
    bool triggered;
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (canExecute && !triggered && stateInfo.normalizedTime > 0.9f)
        {
            triggered = true;
            weapon.IncrementAmmoPhase();
        }
    }
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        triggered = false;
    }
}
