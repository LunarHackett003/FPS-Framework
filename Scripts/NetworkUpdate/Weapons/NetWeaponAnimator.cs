using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode.Components;
using UnityEngine;

public class NetWeaponAnimator : LunarNetScript
{
    [SerializeField] internal Animator animator;
    [SerializeField] internal NetworkAnimator networkAnimator;
    [SerializeField] internal NetWeaponController controller;
    [SerializeField] internal BaseNetWeapon weapon;
    public bool isWeapon;

    internal AnimatorOverrideController aoc;
    internal AnimationClipOverrides clipOverrides;


    internal void Initialise()
    {
        if (controller == null && !TryGetComponent(out controller))
        {
            controller = GetComponentInParent<NetWeaponController>();
        }
        if (isWeapon)
        {
            weapon = GetComponent<BaseNetWeapon>();
            UpdateAnimations();
            animator.tag = "Weapon";
            animator.SetLayerWeight(1, 1);
        }
        else
        {
            animator.SetLayerWeight(2, 1);
        }

        networkAnimator = animator.GetComponent<NetworkAnimator>();

    }

    public void UpdateAnimations()
    {
        if (aoc == null)
        {
            aoc = new(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = aoc;

            clipOverrides = new(aoc.overridesCount);
            aoc.GetOverrides(clipOverrides);
        }

        AnimationClipPair[] clips = isWeapon ? weapon.animationSet.clips : controller.CurrentWeapon.animationSet.clips;
        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClipPair acp = clips[i];
            //if (!isWeapon && acp.targetClip.name == "ChangeWeapon")
            //{
            //    continue;
            //}
            //else
            clipOverrides[acp.targetClip.name] = isWeapon ? acp.weaponClip : acp.characterClip;
        }
        aoc.ApplyOverrides(clipOverrides);

        if (!isWeapon)
        {
            animator.Update(Time.fixedDeltaTime);
        }
    }
    public void ChangeEquipAnimation()
    {
        if (aoc != null)
        {
            clipOverrides["ChangeWeapon"] =
                controller.CurrentWeapon.animationSet.clips.First(x => x.targetClip.name == "ChangeWeapon").characterClip;
        }
    }

    public virtual void SetAnimationBool(string parameter, bool value)
    {
        if (!IsOwner)
            return;

        if (animator != null)
        {
            animator.SetBool(parameter, value);
        }
    }
    public virtual void SetAnimationFloat(string parameter, float value, float dampTime = 0, bool useFixedDelta = true)
    {
        if (!IsOwner)
            return;

        if (animator != null)
        {
            if(dampTime > 0)
            {
                animator.SetFloat(parameter, value, dampTime, useFixedDelta ? Time.fixedDeltaTime : Time.deltaTime);
            }
            else
            {
                animator.SetFloat(parameter, value);
            }
        }
    }

    public virtual void TriggerAnimation(string trigger, float time, bool reset = false)
    {
        if (!IsOwner)
            return;

        if (animator != null)
        {
            StartCoroutine(AnimationTrigger(trigger, time, reset));
        }
    }
    protected virtual IEnumerator AnimationTrigger(string trigger, float time, bool reset = false)
    {
        //Debug.Log($"Triggered {trigger} on {gameObject.name}'s animator for {time} seconds");
        networkAnimator.SetTrigger(trigger);
        if (reset)
        {
            yield return new WaitForSeconds(time);
            networkAnimator.ResetTrigger(trigger);
        }
        yield break;
    }
}
