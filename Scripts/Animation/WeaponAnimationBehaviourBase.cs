using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponAnimationBehaviourBase : StateMachineBehaviour
{
    protected NetWeaponController controller;
    protected BaseNetWeapon weapon;
    protected bool canExecute;

    public float LoopTime
    {
        get { return _looptime; }
        set { _looptime = value % 1; }
    }
    float _looptime;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (controller == null)
        {
            controller = animator.GetComponentInParent<NetWeaponController>();
            canExecute = !animator.CompareTag("Weapon");
        }
        weapon = controller.CurrentWeapon;
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        LoopTime = stateInfo.normalizedTime;
    }
}
