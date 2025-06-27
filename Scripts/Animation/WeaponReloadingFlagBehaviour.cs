using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponReloadingFlagBehaviour : WeaponAnimationBehaviourBase
{
    public float blockFromNormTime, unblockAfterNormTime;
    public bool blocked;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        controller.reloading = true;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        if (!canExecute)
            return;
        blocked = LoopTime >= blockFromNormTime && LoopTime < unblockAfterNormTime;
        UpdateBlock();
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        if (!canExecute)
            return;
        blocked = false;
        controller.reloading = false;
        UpdateBlock();
    }
    void UpdateBlock()
    {
        if(controller != null)
        {
            controller.fireBlockedByAnimation = blocked;
        }
    }
}
