using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponAnimationBehaviourBase : StateMachineBehaviour
{
    protected NetWeaponController controller;
    protected BaseNetWeapon weapon;
    protected bool canExecute;
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
}
