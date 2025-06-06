using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NetWeaponController : LunarNetScript
{
    [SerializeField] internal bool primaryInput, secondaryInput;
    internal bool lastPrimary, lastSecondary, lastReload;
    internal int previousWeaponCount;

    [SerializeField] internal List<BaseNetWeapon> weapons;
    [SerializeField] internal int lastWeaponCount;
    [SerializeField]
    internal NetworkVariable<int> weaponIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField]
    internal int nextWeaponIndex;
    public BaseNetWeapon CurrentWeapon => weaponIndex.Value < weapons.Count ? weapons[weaponIndex.Value] : null;

    [SerializeField] BaseNetWeapon currentWeapon;

    [SerializeField] internal Collider[] colliders;
    internal HashSet<Collider> colliderSet; 
    [SerializeField] internal Transform fireOrigin;

    internal virtual bool FireBlocked => fireBlockedByAnimation;
    [SerializeField] internal bool fireBlockedByAnimation;
    internal float aimLerp = 0;
    internal float aimAmount;
    internal bool switchingWeapons;

    [SerializeField] internal NetWeaponAnimator animator;

    [SerializeField] internal bool hideWeapons;
    internal bool weaponsHiddenLast;

    public Vector3 recoilTargetAngular;
    public Vector3 currentRecoilAngular;
    public Vector3 recoilTargetLinear;
    public Vector3 currentRecoilLinear;

    public RecoilParams defaultRecoilParams;
    internal float recoilShotClearTime;
    internal int recoilShotsFired;

    public virtual float Spread(float value) => value * (1 - aimAmount);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        colliderSet = new(colliders);

        weaponIndex.OnValueChanged += WeaponIndexChanged;
    }
    void WeaponIndexChanged(int previous, int current)
    {
        if (!IsOwner)
        {
            animator.UpdateAnimations();
        }
    }

    public void WeaponAdded(BaseNetWeapon weapon)
    {
        if (!weapons.Contains(weapon))
            weapons.Add(weapon);
        weapon.InitialiseWeapon(this);

        ShowWeapon(weaponIndex.Value, false);

        RePollWeapons();
    }
    public void RePollWeapons()
    {
        Debug.Log("Repolling weapons");
        if (lastWeaponCount == 0)
        {
            Debug.Log("Repolled weapons and re-initialised");
            lastWeaponCount = weapons.Count;
            Initialise();
        }
    }
    public virtual void Initialise()
    {
        animator.Initialise();
        if (weapons.Count > 0)
        {
            ChangeCurrentWeapon(weapons[0], out _, out _);

            ShowWeapon(0, false);
        }
        lastWeaponCount = weapons.Count;
    }

    public override void LUpdate()
    {
        if(weaponsHiddenLast != hideWeapons)
        {
            ShowWeapon(weaponIndex.Value, hideWeapons);
            weaponsHiddenLast = hideWeapons;
        }



        if(CurrentWeapon != null)
        {
            if(lastPrimary != primaryInput)
            {
                CurrentWeapon.primaryInput = primaryInput;
                lastPrimary = primaryInput;
            }
            if(lastSecondary != secondaryInput)
            {
                CurrentWeapon.secondaryInput = secondaryInput;
                lastSecondary = secondaryInput;
            }
        }
    }


    public override void LTimestep()
    {
        base.LTimestep();

        if (lastWeaponCount != weapons.Count)
        {
            RePollWeapons();
            lastWeaponCount = weapons.Count;
        }

        if(defaultRecoilParams != null)
        {
            
            UpdateRecoil(CurrentWeapon.recoilParams != null ? CurrentWeapon.recoilParams : defaultRecoilParams);
        }
    }

    public void ShowWeapon(int indexToShow, bool hideAll = false)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].transform.localScale = hideAll ? Vector3.zero : (i == indexToShow ? Vector3.one : Vector3.zero);
        }
    }

    public virtual void ChangeCurrentWeapon(BaseNetWeapon newWeapon, out BaseNetWeapon oldWeapon, out bool success)
    {
        oldWeapon = CurrentWeapon;

        oldWeapon.primaryInput = false;
        oldWeapon.secondaryInput = false;

        currentWeapon = newWeapon;
        success = newWeapon != null && newWeapon != CurrentWeapon;

        if (animator != null)
            animator.UpdateAnimations();

        oldWeapon.isCurrentWeapon = false;
        newWeapon.isCurrentWeapon = true;

        switchingWeapons = false;
    }
    public virtual void SwitchToWeaponIndex(int index)
    {
        switchingWeapons = false;
        ChangeCurrentWeapon(weapons[index], out BaseNetWeapon oldWeapon, out bool success);
        if (success)
        {
            if (IsOwner)
            {
                UpdateWeaponIndex_RPC(weaponIndex.Value, index);
            }


        }
    }

    [Rpc(SendTo.Everyone)]
    public void UpdateWeaponIndex_RPC(int oldIndex, int newIndex)
    {
        if (IsOwner)
        {
            weaponIndex.Value = newIndex;
        }
        nextWeaponIndex = newIndex;

        animator.UpdateAnimations();
        ShowWeapon(newIndex, false);
    }
    public virtual void WeaponIndexUpdated()
    {
        
    }
    public virtual void ReceivePostAttack()
    {

    }

    public virtual void UpdateRecoil(RecoilParams rp)
    {
        if(recoilShotClearTime < rp.recoilShotClearTime)
        {
            recoilShotClearTime += Time.fixedDeltaTime;
        }
        else
        {
            recoilShotsFired = 0;
        }


        float recoilDecay = rp.recoilDecay * Time.fixedDeltaTime;
        recoilTargetLinear = Vector3.MoveTowards(recoilTargetLinear, Vector3.zero, recoilDecay);
        recoilTargetAngular = Vector3.MoveTowards(recoilTargetAngular, Vector3.zero, recoilDecay);

        currentRecoilLinear = Vector3.Lerp(currentRecoilLinear, recoilTargetLinear, defaultRecoilParams.recoilSnappiness * Time.fixedDeltaTime);
        currentRecoilAngular = Vector3.Slerp(currentRecoilAngular, recoilTargetAngular, defaultRecoilParams.recoilSnappiness * Time.fixedDeltaTime);
    }

    public virtual void ReceiveRecoil(float charge, out float recoilCurveEval)
    {
        
        Debug.Log("received recoil impulse");

        float shotsFiredLerp = Mathf.InverseLerp(0, CurrentWeapon.recoilParams.maxRecoilShots, recoilShotsFired);
        //Debug.Log($"{recoilShotsFired}/{CurrentWeapon.recoilParams.maxRecoilShots} => {shotsFiredLerp}");
        recoilCurveEval = CurrentWeapon.recoilParams.recoilMultiplierCurve.Evaluate(shotsFiredLerp);

        Vector3 linearRecoil = Vector3.Scale(HelperMethods.RandomPerAxis(CurrentWeapon.recoilParams.minLinearRecoil, CurrentWeapon.recoilParams.maxLinearRecoil),
            Vector3.Lerp(Vector3.one, CurrentWeapon.recoilParams.aimedLinearRecoilScale, aimAmount)) * recoilCurveEval;
        Vector3 angularRecoil = Vector3.Scale(HelperMethods.RandomPerAxis(CurrentWeapon.recoilParams.minAngularRecoil, CurrentWeapon.recoilParams.maxAngularRecoil),
            Vector3.Lerp(Vector3.one, CurrentWeapon.recoilParams.aimedAngularRecoilScale, aimAmount)) * recoilCurveEval;

        recoilShotClearTime = 0;
        recoilShotsFired++;

        if (CurrentWeapon.recoilParams.chargeAffectsRecoil)
        {
            float chargeMult = CurrentWeapon.recoilParams.recoilChargeInfluence.Evaluate(charge);
            linearRecoil *= chargeMult;
            angularRecoil *= chargeMult;
        }

        recoilTargetLinear += linearRecoil;
        recoilTargetAngular += angularRecoil;
    }
}
