using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetPlayerWeaponController : NetWeaponController
{
    [SerializeField] internal bool reloadInput, meleeInput;
    internal Vector2 weaponSwitchInput;
    [SerializeField] internal NetPlayerEntity player;


    [SerializeField] float crouchAccuracyMultiplier;
    internal float currentFOV;
    [SerializeField] internal Transform weaponPositionOffset, weaponRotationInvert, weaponTargetTransform;
    internal float fovLerp = 0;

    internal override bool FireBlocked => base.FireBlocked || player.isDead.Value || player.heldInteraction || player.carryConfirmed;

    public float tempAimPitchCurr, tempAimPitchTarg;

    public override float Spread(float value)
    {
        return value * Mathf.Clamp01((1 - aimAmount) - (player.motor.currentCrouchLerp * crouchAccuracyMultiplier));
    }
    public override void LPostUpdate()
    {
        base.LPostUpdate();
        if (CurrentWeapon != null)
        {
            CurrentWeapon.transform.SetPositionAndRotation(weaponRotationInvert.position, weaponRotationInvert.rotation);
        }
    }

    public override void LUpdate()
    {

        if (IsOwner)
        {
            primaryInput = InputManager.PrimaryInput && !FireBlocked;
            secondaryInput = InputManager.SecondaryInput && !FireBlocked;


            weaponSwitchInput = InputManager.WeaponSwitchInput;
            reloadInput = InputManager.ReloadInput;


            if (primaryInput != lastPrimary || secondaryInput != lastSecondary || reloadInput != lastReload)
            {
                SendInputToServer_RPC(primaryInput, secondaryInput, reloadInput);
            }

            if (CurrentWeapon != null && !FireBlocked)
            {
                if (!switchingWeapons && weaponSwitchInput != Vector2.zero)
                {
                    TrySwitchWeapon();
                }
                if (reloadInput)
                {
                    if ((!CurrentWeapon.useEquipmentRecharge || CurrentWeapon.equipmentCharges.Value > 0) && 
                        (CurrentWeapon.useAmmunition && CurrentWeapon.hasReloadAnimation && (CurrentWeapon.CurrentAmmo.Value < CurrentWeapon.maxAmmo) 
                        || (CurrentWeapon.useAmmoPhases && CurrentWeapon.currentAmmoPhase != 0)))
                    {
                        CurrentWeapon.TriggerAnimation(CurrentWeapon.CurrentAmmo.Value > 0 ? BaseNetWeapon.PARTIALRELOAD : BaseNetWeapon.EMPTYRELOAD, 0.2f, true);
                    }
                    if (IsOwner)
                    {
                        InputManager.ReloadInput = false;
                    }
                    reloadInput = false;
                }
            }
        }



        player.motor.aiming = secondaryInput && !player.motor.sliding;

        if (IsOwner)
        {
            UpdateFOV();
        }

        UpdateWeaponOrientation();


        base.LUpdate();
    }

    public void TrySwitchWeapon()
    {

        int weaponSwitchIndex = Mathf.RoundToInt(Mathf.Atan2(weaponSwitchInput.x, weaponSwitchInput.y) * Mathf.Rad2Deg);
        if (weaponSwitchIndex < 0)
        {
            weaponSwitchIndex += 360;
        }
        weaponSwitchIndex /= 90;
        if (!CanSwitchToWeapon(weaponSwitchIndex))
        {
            return;
        }
        Debug.Log(weaponSwitchIndex);
        CurrentWeapon.TriggerAnimation(BaseNetWeapon.CHANGEWEAPON, BaseNetWeapon.TRIGGERTIMETINY, true);
        weaponSwitchInput = Vector2.zero;
        if (IsOwner)
        {
            InputManager.WeaponSwitchInput = Vector2.zero;
            nextWeaponIndex = weaponSwitchIndex;
        }
    }
    

    public bool CanSwitchToWeapon(int index)
    {
        bool canSwitch = weaponIndex.Value != index && index < weapons.Count && index > -1 ;
        if (!canSwitch)
            return false;
        BaseNetWeapon weapon = weapons[index];
        canSwitch &= (!weapon.useEquipmentRecharge || weapon.canSwitchIfNoCharges) || weapon.equipmentCharges.Value > 0;

        return canSwitch;
    }

    public void UpdateWeaponOrientation()
    {
        if (CurrentWeapon == null || CurrentWeapon.aimParams == null || weaponTargetTransform == null || weaponPositionOffset == null || weaponRotationInvert == null)
        {
            //Invalid setup, cannot do anything
            if (aimLerp > 0)
            {
                aimAmount = Mathf.MoveTowards(aimAmount, 0, player.motor.viewParams.fovMoveSpeed * Time.deltaTime);
                aimLerp = Mathf.Lerp(aimLerp, aimAmount, Time.deltaTime * player.motor.viewParams.fovMoveSpeed);
            }
            return;
        }

        aimAmount = Mathf.MoveTowards(aimAmount, player.motor.aiming ? 1 : 0, CurrentWeapon.aimParams.aimSpeed * Time.deltaTime);
        //aimLerp = Mathf.Lerp(aimLerp, aimAmount, Time.deltaTime * CurrentWeapon.aimParams.aimSpeed);
        aimLerp = CurrentWeapon.aimParams.aimLerpCurve.Evaluate(aimAmount);
        float crouchLerp = CurrentWeapon.aimParams.crouchLerpCurve.Evaluate(player.motor.currentCrouchLerp);

        //We need to scale the local position of the weapon target, and apply that to the weapon offset
        weaponPositionOffset.localPosition = CurrentWeapon.aimParams.crouchPositionOffset * (crouchLerp * (1 - (aimLerp * CurrentWeapon.aimParams.aimRotationReduction)))
            + (CurrentWeapon.aimParams.baseAimPositionOffset +
            weaponTargetTransform.localPosition.Multiply(CurrentWeapon.aimParams.aimedWeaponPositionScale) +
            (CurrentWeapon.aimParams.aimPositionOffsetAngled * (1 - CurrentWeapon.aimParams.aimRotationReduction)) ) * aimLerp + currentRecoilLinear;


        weaponRotationInvert.localRotation = Quaternion.Lerp(
            Quaternion.Lerp(Quaternion.identity, CurrentWeapon.aimParams.crouchRotationOffset, crouchLerp),
            Quaternion.Inverse(weaponTargetTransform.localRotation) * CurrentWeapon.aimParams.aimRotationOffset,
            aimLerp * CurrentWeapon.aimParams.aimRotationReduction) * Quaternion.Euler(currentRecoilAngular);
    }

    public void UpdateFOV()
    {

        //The mother of all ternary statements...
        float fov =
            //Are we sliding
            player.motor.sliding ? player.motor.viewParams.slideFOV :
            //Are we sprinting or sliding?
            ((player.motor.sprinting && player.motor.moveInput != Vector2.zero) || player.motor.sliding) ? player.motor.viewParams.sprintFOV :
            //Are we moving normally or crouching?
            0;
        //currentFOV = Mathf.Lerp(rbpm.viewParams.baseFOV, rbpm.viewParams.baseFOV + fov, aimLerp);
        fovLerp = Mathf.Lerp(fovLerp, fov + (CurrentWeapon != null ? CurrentWeapon.aimParams.aimFOV * aimLerp : 0), Time.deltaTime * player.motor.viewParams.fovMoveSpeed);
        currentFOV = player.motor.viewParams.baseFOV + fovLerp;
        //rbpm.viewCineCam.m_Lens.FieldOfView = Mathf.Lerp(rbpm.viewParams.viewmodelBaseFOV, rbpm.viewParams.viewmodelBaseFOV + currentWeapon.aimParams.viewmodelFOV, aimLerp);
        player.motor.viewCamera.fieldOfView = player.motor.viewParams.viewmodelBaseFOV + 
            (CurrentWeapon != null ? (CurrentWeapon.aimParams.viewmodelFOV * aimLerp) : 0);
        player.motor.mainCamera.fieldOfView = currentFOV;
    }

    [Rpc(SendTo.Everyone)]
    void SendInputToServer_RPC(bool primary, bool secondary, bool reload)
    {
        primaryInput = primary;
        secondaryInput = secondary;
        reloadInput = reload;
    }

    public override void ReceiveRecoil(float charge, out float recoilCurveEval)
    {
        base.ReceiveRecoil(charge, out recoilCurveEval);

        tempAimPitchTarg += CurrentWeapon.recoilParams.tempAimPitchPerShot * recoilCurveEval;
        player.motor.lookPitch += CurrentWeapon.recoilParams.permanentAimPitchPerShot * recoilCurveEval;
    }
    public virtual void UpdateAimPitch()
    {
        RecoilParams rp = CurrentWeapon.recoilParams != null ? CurrentWeapon.recoilParams : defaultRecoilParams;
        tempAimPitchTarg = Mathf.MoveTowardsAngle(tempAimPitchTarg, 0, rp.tempAimPitchDecay * Time.fixedDeltaTime);
        tempAimPitchCurr = Mathf.Lerp(tempAimPitchCurr, tempAimPitchTarg, rp.tempAimPitchSnappiness * Time.fixedDeltaTime);
    }
}
