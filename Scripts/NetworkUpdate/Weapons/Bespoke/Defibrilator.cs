using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Defibrilator : BaseNetWeapon
{
    [SerializeField] internal float defibRange;
    [SerializeField] internal float defibThickness;
    [SerializeField] internal float defibDamageOnEnemyHit;

    [SerializeField] internal LayerMask defibLayerMask;

    protected override bool ChargeInput => base.ChargeInput && (CurrentAmmo.Value > 0 || CurrentAmmo.Value > 0) && !fired;

    public override bool Charging => base.Charging && !fired;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        CurrentAmmo.OnValueChanged += AmmoChanged;
    }
    void AmmoChanged(float previous, float current)
    {
        if (current > 0)
            fired = false;
    }
    public override void LTimestep()
    {
        TryDefib();
        TickAmmoCharges();
        UpdateCharge();
    }

    public void TryDefib()
    {
        if(IsOwner && chargeAmount >= 1 && primaryInput && CurrentAmmo.Value > 0 && !fired)
        {
            fired = true;
            TryDefib_RPC(controller.fireOrigin.position, controller.fireOrigin.forward);
            TriggerAnimation(PRIMARYATTACK, TRIGGERTIMESHORT, true);
        }
    }

    protected override void PostAttackBehaviour()
    {
        onWeaponFired?.Invoke(chargeAmount);
        PostAttackCharge();

    }

    [Rpc(SendTo.Server)]
    public void TryDefib_RPC(Vector3 pos, Vector3 forward)
    {
        if(Physics.SphereCast(pos, defibThickness, forward, out RaycastHit hit, defibRange, defibLayerMask))
        {
            if(hit.rigidbody != null)
            {
                if (hit.rigidbody.TryGetComponent(out ReviveTrophy trophy) && NetworkPlayer.IsPlayerOnMyTeam(OwnerClientId, trophy.targetClientID))
                {
                    if (trophy.HitByQuickRevive(OwnerClientId))
                    {
                        Debug.Log("Defibrillated!");
                        PostAttackBehaviour();
                    }
                }
                else if(hit.rigidbody.TryGetComponent(out NetPlayerEntity player))
                {
                    if (!NetworkPlayer.IsPlayerOnMyTeam(OwnerClientId, player.OwnerClientId))
                    {
                        player.ModifyHealth(-defibDamageOnEnemyHit, this, DamageSourceType.weapon, false);
                        PostAttackBehaviour();
                    }
                }
            }
        }
    }
}
