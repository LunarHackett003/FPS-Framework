using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class WeaponEventListener : MonoBehaviour
{
    public BaseNetWeapon weapon;

    public bool applyRecoil;
    public bool playFireEffects;
    public VisualEffect fireEffect;
    public ParticleSystem fireParticle;

    public UnityEvent reloadedEvents;
    public UnityEvent firedEvents;

    public bool playReloadSound;
    
    private void OnEnable()
    {
        if(playFireEffects || applyRecoil)
            weapon.onWeaponFired += WeaponFired;
        weapon.onWeaponReloaded += OnReloaded;
    }
    private void OnDisable()
    {
        if (playFireEffects || applyRecoil)
            weapon.onWeaponFired -= WeaponFired;
        weapon.onWeaponReloaded -= OnReloaded;
    }

    public void WeaponFired(float charge)
    {
        weapon.controller.ReceivePostAttack();
        if (applyRecoil)
        {
            weapon.controller.ReceiveRecoil(charge, out _);
        }
        if (playFireEffects)
        {
            if (fireEffect != null)
                fireEffect.Play();
            if(fireParticle != null)
                fireParticle.Play();
        }
        Debug.Log("Invoked Fire Events!");
        firedEvents?.Invoke();
    }
    public void ReloadStarted(bool emptyReload, bool canceled)
    {

    }
    public void OnReloaded()
    {
        reloadedEvents?.Invoke();
    }
}
