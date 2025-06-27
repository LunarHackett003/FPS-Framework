using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseNetWeapon : LunarNetScript
{

    public const string PRIMARYATTACK = "Primary", SECONDARYATTACK = "Secondary", AMMOPHASE = "AmmoPhase", RELOAD = "FullReload", EMPTYRELOAD = "EmptyReload", 
        COUNTEDRELOADINDEX = "CountedReloadIndex", FIRESWITCHUP = "FireSwitchUp", FIRESWITCHDOWN = "FireSwitchDown", COUNTEDRELOAD = "CountedReload", 
        MANUALACTION = "ManualAction", CHANGEWEAPON = "ChangeWeapon", CHARGEAMOUNT = "Charge", CHARGING = "Charging";
    public const float TRIGGERTIMETINY = 0.1f, TRIGGERTIMESHORT = 0.4f, TRIGGERTIMELONG = 0.8f;

    public string displayName = "Networked Weapon";
    public Sprite weaponIcon;
    public WeaponAnimationSetScriptable animationSet;
    internal bool primaryInput, secondaryInput, primaryPressed, secondaryPressed;

    [SerializeField] internal NetWeaponController controller;
    [SerializeField] internal BaseAnimatable animator;
    [SerializeField] internal bool canCrit;
    [SerializeField] internal float critMultiplier;

    [SerializeField] internal AimParams aimParams;

    [SerializeField] internal float crosshairSpreadBase;
    [SerializeField] internal float crosshairSpreadMax;
    [SerializeField] internal bool hideCrosshairWhenAiming;
    [SerializeField] internal bool useAttackSpread, spreadOnPrimary, spreadOnSecondary;
    [SerializeField] internal float attackSpreadIncrement;
    [SerializeField] internal float attackSpreadDecay;
    [SerializeField] internal float baseAttackSpread = 0.1f;
    [SerializeField] internal float attackSpreadAmount;

    [SerializeField] internal bool useAmmunition, ammoConsumeOnPrimary, ammoConsumeOnSecondary;
    [SerializeField] internal int maxAmmo;
    [SerializeField] internal float ammoPerShot;
    [SerializeField] internal NetworkVariable<float> CurrentAmmo = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] internal bool useAmmoPhases;
    [SerializeField] internal int ammoPhases;
    [SerializeField] internal int currentAmmoPhase;

    public ReloadConfigScriptable reloadConfig;
    public float reloadTime;
    public bool isCountedReload;

    [SerializeField] internal bool useEquipmentRecharge;
    [SerializeField] internal bool canSwitchIfNoCharges;
    [SerializeField] internal float equipmentRechargeTime;
    [SerializeField] internal bool replenishAllEquipmentCharges;
    [SerializeField] internal bool equipmentChargeFillsAmmo;
    internal float currentEquipmentRechargeTime; 

    [SerializeField, Tooltip("Equipment charges present themselves almost like magazines. If you have no equipment charges, you cannot reload a weapon that is configured with recharges.\n" +
        "If you have no charges, you cannot use equipment that might not use ammunition.")] internal NetworkVariable<int> equipmentCharges = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] internal int equipmentChargeCapacity, equipmentChargesRefilled;

    [SerializeField] internal bool queuedReloadAnimation;
    [SerializeField] protected bool fired = false;

    [SerializeField, Tooltip("Should the weapon charge for the primary attack? Takes priority over secondary charge")] internal bool primaryUsesCharge;
    [SerializeField, Tooltip("Should the weapon charge for the secondary attack? Does not work if primary uses charge")] internal bool secondaryUsesCharge;
    [SerializeField, Tooltip("How much charge the weapon accumulates every second")] internal float chargeRate;
    [SerializeField, Tooltip("How much charge the weapon loses every second when not charge")] protected float chargeDecayRate;
    [SerializeField, Tooltip("How much charge is required to fire the weapon?")] protected float minimumChargeToFire;
    [SerializeField, Tooltip("Resets charge to zero after firing")] protected bool resetChargeOnFire;
    [SerializeField, Tooltip("How much charge the weapon currently has")] internal float chargeAmount;
    [SerializeField, Tooltip("Will the weapon charge to full, even if the player releases the fire input?")] protected bool chargeUntilFire;
    [SerializeField, Tooltip("Will the forced charge end when we reach minimum charge, if we've released the fire input?")] protected bool chargeOnlyUntilMinimum;
    [SerializeField, Tooltip("Fires the weapon when we release the fire input")] protected bool fireOnRelease;
    protected virtual bool PrimaryBlocked => fired || (useAmmunition && CurrentAmmo.Value <= 0) || BlockedByRecharge;
    protected virtual bool ChargeInput => (primaryUsesCharge && primaryInput) || (secondaryUsesCharge && secondaryInput) || chargeHoldFrame;
    protected virtual bool BlockedByRecharge => useEquipmentRecharge && (equipmentCharges.Value == 0 && CurrentAmmo.Value == 0);
    internal bool animatedFirePending;
    internal bool animatedFireLast;

    public HashSet<Collider> ignoredColliders;

    [SerializeField] internal bool chargeCoroutineRunning;
    internal bool chargeHoldFrame;


    [SerializeField] internal RecoilParams recoilParams;

    public delegate void WeaponFired(float charge);
    public WeaponFired onWeaponFired;

    public delegate void ReloadEvent(bool emptyReload, bool canceled);
    public ReloadEvent onReloadEvent;

    public delegate void WeaponReloaded();
    public WeaponReloaded onWeaponReloaded;

    public virtual bool Charging => chargeCoroutineRunning || ChargeInput || chargeHoldFrame;

    public bool isCurrentWeapon;

    public ReloadInfo currentReloadInfo;

    /// <summary>
    /// Calculates the damage that should be dealt at the supplied distance.
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    public virtual float GetDamage(float distance = 0)
    {
        return 0;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        controller = NetPlayerEntity.playersByID[OwnerClientId].weaponController;
        controller.WeaponAdded(this);

        equipmentCharges.OnValueChanged += WeaponChargesUpdated;

        if (useAmmunition)
        {
            if(IsServer)
            {
                CurrentAmmo.Value = maxAmmo;
            }
        }
        if (IsServer && useEquipmentRecharge)
        {
            equipmentCharges.Value = equipmentChargeCapacity;
        }
    }

    public virtual void WeaponChargesUpdated(int previous, int current)
    {
        currentEquipmentRechargeTime = 0;
    }

    public virtual void InitialiseWeapon(NetWeaponController controller)
    {
        Debug.Log("Initialised weapon", gameObject);

        this.controller = controller;
        animator.controller = controller;

        animator.Initialise();

        ignoredColliders = new(controller.colliderSet);
    }

    public override void LTimestep()
    {
        if(animatedFireLast != animatedFirePending)
        {
            animatedFireLast = animatedFirePending;
            SetBool(MANUALACTION, animatedFirePending);
        }
        
        if(primaryUsesCharge || secondaryUsesCharge)
        {
            UpdateCharge();
        }

        if (primaryInput && !secondaryPressed && !PrimaryBlocked)
        {
            PrimaryBehaviour();
        }
        if(secondaryInput && !primaryPressed)
        {
            SecondaryBehaviour();
        }

        if (useAttackSpread)
        {
            attackSpreadAmount = Mathf.Clamp01(attackSpreadAmount - (Time.fixedDeltaTime * attackSpreadDecay));
        }

        TickAmmoCharges();

    }
    protected virtual void TickAmmoCharges()
    {
        if (useEquipmentRecharge && equipmentCharges.Value < equipmentChargeCapacity)
        {
            currentEquipmentRechargeTime += Time.fixedDeltaTime;
            if (currentEquipmentRechargeTime >= equipmentRechargeTime)
            {
                currentEquipmentRechargeTime = 0;
                if (IsServer)
                {
                    UpdateAmmoCharges();
                }
            }
        }
    }

    protected virtual void PrimaryBehaviour()
    {
        if (fired)
            return;
    }
    protected virtual void SecondaryBehaviour()
    {

    }
    protected virtual void PostAttackBehaviour()
    {
        onWeaponFired?.Invoke(chargeAmount);
        PostAttackSpread();
        PostAttackCharge();
        CheckEquipmentCharge();
        UpdateAmmo();
        if(useAmmunition && IsOwner)
        {
            CheckReloadOrPhase();
        }
    }
    protected void PostAttackSpread()
    {
        if (IsOwner || IsServer)
        {
            if (useAttackSpread)
            {
                attackSpreadAmount = Mathf.Clamp01(attackSpreadAmount + attackSpreadIncrement);
            }
        }
    }
    protected void CheckEquipmentCharge()
    {
        if (IsServer && useEquipmentRecharge)
        {
            equipmentCharges.Value--;
        }
    }
    protected void UpdateAmmo()
    {
        if(useAmmunition && IsServer)
        {
            CurrentAmmo.Value -= ammoPerShot;
        }
    }
    protected void CheckReloadOrPhase()
    {
        //if (CurrentAmmo.Value <= 0)
        //{
        //    if (useAmmoPhases && currentAmmoPhase < ammoPhases)
        //    {
        //        TriggerAnimation(AMMOPHASE, TRIGGERTIMELONG, true);
        //    }
        //    else if (hasReloadAnimation && (!useEquipmentRecharge || equipmentCharges.Value > 0))
        //    {
        //        TriggerAnimation(EMPTYRELOAD, TRIGGERTIMESHORT, true);
        //    }
        //}

        if(CurrentAmmo.Value <= 0)
        {
            if(useAmmoPhases && currentAmmoPhase < ammoPhases)
            {
                TriggerAnimation(AMMOPHASE, TRIGGERTIMELONG, true);
            }
            else if (reloadConfig != null && (!useEquipmentRecharge || equipmentCharges.Value > 0))
            {
                PlayReloadAnimation();
            }
        }
    }
    protected void PostAttackCharge()
    {
        if ((primaryUsesCharge || secondaryUsesCharge) && resetChargeOnFire)
        {
            chargeAmount = 0;
        }
    }

    protected virtual void UpdateAmmoCharges()
    {
        if (replenishAllEquipmentCharges)
        {
            equipmentCharges.Value = equipmentChargeCapacity;
        }
        else
        {
            equipmentCharges.Value += equipmentChargesRefilled;
        }

        if (equipmentChargeFillsAmmo)
        {
            CurrentAmmo.Value = equipmentCharges.Value;
        }
    }
    public void IncrementAmmoPhase()
    {
        currentAmmoPhase++;
        ReloadWeapon(false);
    }
    /// <summary>
    /// Restores the weapon's ammunition to MaxAmmo, and optionally reset the ammo phase.
    /// </summary>
    /// <param name="resetAmmoPhase">Reset the ammo phase back to zero?</param>
    internal virtual void ReloadWeapon(bool resetAmmoPhase = false)
    {
        if (useAmmoPhases && resetAmmoPhase)
        {
            currentAmmoPhase = 0;
        }



        primaryPressed = false;
        animatedFirePending = false;

        if (isCountedReload)
        {
            UpdateCountedReload();
        }

        if (IsServer)
        {
            //Determine how much of our equipment charge we're consuming to recharge this item
            float reloadAmount = useEquipmentRecharge ? Mathf.Min(equipmentCharges.Value, maxAmmo) : (currentReloadInfo.reloadFully ? maxAmmo : CurrentAmmo.Value + currentReloadInfo.amountToReload);
            //And then reload our weapon
            CurrentAmmo.Value = reloadAmount;
        }
    }

    protected virtual void UpdateCharge()
    {
        //If we are not charging this weapon via a coroutine...
        if (!chargeCoroutineRunning)
        {
            //Start charging 
            if (chargeUntilFire && chargeAmount < (chargeOnlyUntilMinimum ? minimumChargeToFire : 1) && !chargeCoroutineRunning && ChargeInput)
            {
                StartCoroutine(ChargeWeaponCoroutine());
            }
            if (!chargeHoldFrame)
                chargeAmount += Time.fixedDeltaTime * (Charging ? chargeRate : -chargeDecayRate);
        }

        chargeAmount = Mathf.Clamp01(chargeAmount);
        SetFloat(CHARGEAMOUNT, chargeAmount);
        SetBool(CHARGING, Charging);
    }

    public virtual void PlayReloadAnimation()
    {
        //Find the weapon's reload info for its current ammo value
        UpdateCountedReload();

        SetBool(EMPTYRELOAD, CurrentAmmo.Value == 0);

        TriggerAnimation(currentReloadInfo.reloadTrigger, TRIGGERTIMESHORT, true);
    }
    public virtual void UpdateCountedReload()
    {
        currentReloadInfo = reloadConfig.GetReloadInfo(CurrentAmmo.Value);
        reloadTime = currentReloadInfo.reloadTime;

        //We shouldn't be here if we're not using a counted reload, so uh... yeah
        if (currentReloadInfo.isCountedReload)
        {
            SetInt(COUNTEDRELOADINDEX, currentReloadInfo.countedReloadIndex);
        }
        

    }


    internal virtual void TriggerAnimation(string parameter, float time, bool reset = false)
    {
        if (!isCurrentWeapon)
            return;

        if (controller != null && controller.animator != null)
            controller.animator.TriggerAnimation(parameter, time, reset);
        if (animator != null)
            animator.TriggerAnimation(parameter, time, reset);
    }

    internal virtual void SetBool(string parameter, bool value)
    {
        if (!isCurrentWeapon)
            return;

        if (controller != null && controller.animator != null)
            controller.animator.SetAnimationBool(parameter, value);
        if (animator != null)
            animator.SetAnimationBool(parameter, value);
    }
    internal virtual void SetInt(string parameter, int value)
    {
        if (!isCurrentWeapon)
            return;

        if (controller != null && controller.animator != null)
            controller.animator.SetAnimationInt(parameter, value);
        if (animator != null)
            animator.SetAnimationInt(parameter, value);
    }
    internal virtual void SetFloat(string parameter, float value)
    {
        if (!isCurrentWeapon)
            return;

        if (controller != null && controller.animator != null)
            controller.animator.SetAnimationFloat(parameter, value);
        if (animator != null)
            animator.SetAnimationFloat(parameter, value);
    }

    public virtual IEnumerator ChargeWeaponCoroutine()
    {
        chargeCoroutineRunning = true;
        float threshold = chargeOnlyUntilMinimum ? minimumChargeToFire : 1;
        while (chargeAmount < threshold)
        {
            chargeAmount += Time.fixedDeltaTime * chargeRate;
            if (chargeAmount >= minimumChargeToFire && chargeOnlyUntilMinimum && ChargeInput)
                threshold = 1;
            yield return new WaitForFixedUpdate();
        }
        chargeHoldFrame = true;
        chargeCoroutineRunning = false;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        chargeHoldFrame = false;
        yield break;
    }
}
