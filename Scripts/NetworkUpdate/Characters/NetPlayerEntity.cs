using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Rendering.Universal;

/// <summary>
/// The class that ties together all of the player related systems.
/// </summary>
public class NetPlayerEntity : NetEntity
{
    /// <summary>
    /// A dictionary of all players by their Client ID. Used for quick player lookups.
    /// </summary>
    internal static Dictionary<ulong, NetPlayerEntity> playersByID = new();

    [SerializeField] internal NetPlayerMotor motor;

    [SerializeField] internal NetPlayerWeaponController weaponController;

    [SerializeField] internal Camera viewmodelCamera;

    [SerializeField] internal Renderer[] allRenderers;

    [SerializeField] internal float quickReviveHealthPortion = 0.5f;

    [SerializeField] internal NetworkObject playerReviveItemPrefab;

    internal NetworkObject reviveItemInstance;

    internal BaseAnimatable Animator => weaponController.animator;

    public ParticleSystem deathParticle;

    internal InteractableObject currentInteractTarget;
    internal InteractableObject carryTargetRequested;

    internal Rigidbody interactTargetRigidbody;

    internal bool carryConfirmed;

    internal bool heldInteraction;

    [SerializeField] internal CapsuleCollider capsule;


    [SerializeField] internal Material friendlyMaterial, enemyMaterial;
    [SerializeField] internal Renderer[] materialOverrideRenderers;
    [SerializeField] internal InteractionConfig interactConfig;

    internal NetHitbox[] playerHitboxes;

    public bool isFriendly;
    bool lastDead;

    bool hookedUpdateMaterials;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playersByID.Add(OwnerClientId, this);

        playerHitboxes = GetComponentsInChildren<NetHitbox>();

        if (IsOwner)
        {
            if(GameplayCanvas.Instance != null)
                GameplayCanvas.player = this;
            Camera.main.GetUniversalAdditionalCameraData().cameraStack.Add(viewmodelCamera);
        }

        weaponController.Initialise();
    }

    void UpdateMaterials()
    {

        Debug.Log("Updating materials on player", gameObject);
        isFriendly = NetworkPlayer.IsPlayerOnMyTeam(NetworkManager.LocalClientId, OwnerClientId);

        if (!IsOwner)
        {

            for (int i = 0; i < materialOverrideRenderers.Length; i++)
            {
                materialOverrideRenderers[i].material = isFriendly ? friendlyMaterial : enemyMaterial;
            }
        }
    }

    void SubscribeMaterialUpdate()
    {
        try
        {
            NetworkPlayer.netPlayers[OwnerClientId].onTeamUpdated += UpdateMaterials;
            NetworkPlayer.netPlayers[NetworkManager.LocalClientId].onTeamUpdated += UpdateMaterials;

            UpdateMaterials();
        }
        catch (System.Exception)
        {
            hookedUpdateMaterials = false;
            throw;
        }
        finally
        {
            hookedUpdateMaterials = true;
        }
    }

    public override void LUpdate()
    {
        base.LUpdate();

        if (!hookedUpdateMaterials)
        {
            SubscribeMaterialUpdate();
        }
    }
    public override void LTimestep()
    {
        base.LTimestep();

        if(lastDead != isDead.Value)
        {
            if (isDead.Value)
            {
                DamageableDied(null, false);
            }
            lastDead = isDead.Value;
        }


        if (!IsOwner)
            return;
        TryInteract();
        UpdateCarry();

    }

    void TryInteract()
    {
        if (isDead.Value)
        {
            if (carryTargetRequested != null && carryConfirmed)
            {
                carryTargetRequested.GrabReleased_RPC(Vector3.zero, carryTargetRequested.transform.position, false);
                ReleaseGrabbedObject_RPC(carryTargetRequested, false);
            }
            if (heldInteraction && currentInteractTarget != null)
            {
                currentInteractTarget.InteractEnd_RPC(false);
            }
        }
        else
        {
            Debug.DrawRay(weaponController.fireOrigin.position, weaponController.fireOrigin.forward * interactConfig.maxInteractDistance);
            if (!heldInteraction && !carryConfirmed)
            {
                if (Physics.SphereCast(weaponController.fireOrigin.position, interactConfig.interactThickness, weaponController.fireOrigin.forward, out RaycastHit hit, interactConfig.maxInteractDistance, interactConfig.interactLayerMask))
                {
                    if (hit.rigidbody != null)
                    {
                        interactTargetRigidbody = hit.rigidbody;
                    }
                    else
                    {
                        interactTargetRigidbody = null;
                    }
                }
                else
                {
                    interactTargetRigidbody = null;
                }
            }

            if (interactTargetRigidbody != null && (!heldInteraction || !carryConfirmed) && interactTargetRigidbody.TryGetComponent(out currentInteractTarget))
            {
                bool canUse = currentInteractTarget.CanInteract(OwnerClientId);
                if (InputManager.InteractInput && currentInteractTarget.hasInteraction && canUse)
                {
                    if (currentInteractTarget.holdToInteract)
                    {
                        heldInteraction = true;
                    }
                    else
                    {
                        InputManager.InteractInput = false;
                    }
                    currentInteractTarget.InteractStart_RPC(OwnerClientId);
                }

                if (InputManager.GrabInput && currentInteractTarget.canCarry && canUse)
                {
                    InputManager.GrabInput = false;
                    InputManager.PrimaryInput = false;
                    InputManager.SecondaryInput = false;
                    carryTargetRequested = currentInteractTarget;
                    carryTargetRequested.TryGrab_RPC(OwnerClientId);
                    weaponController.animator.TriggerAnimation("Grab", 0.2f, true);
                }
            }

            if (currentInteractTarget != null && heldInteraction && !InputManager.InteractInput)
            {
                currentInteractTarget.InteractEnd_RPC(false);
            }

            if (!InputManager.InteractInput && interactTargetRigidbody == null)
            {
                currentInteractTarget = null;
            }
        }
    }

    public void UpdateCarry()
    {
        if (carryTargetRequested != null && carryConfirmed)
        {
            if (OwnerClientId == carryTargetRequested.OwnerClientId)
            {
                carryTargetRequested.rb.Move(Vector3.Lerp(carryTargetRequested.rb.position, weaponController.fireOrigin.TransformPoint(interactConfig.grabbedObjectOffsetFromWeaponPoint), interactConfig
                    .interactedObjectMoveSpeed * Time.fixedDeltaTime),
                    interactConfig.interactRotateUseSlerp ? Quaternion.Slerp(carryTargetRequested.rb.rotation, weaponController.fireOrigin.rotation,
                    Time.fixedDeltaTime * interactConfig.interactedObjectRotateSpeed)
                    : Quaternion.Lerp(carryTargetRequested.rb.rotation, weaponController.fireOrigin.rotation * carryTargetRequested.grabRotationOffset, Time.fixedDeltaTime * interactConfig.interactedObjectRotateSpeed));

                if (InputManager.PrimaryInput && carryConfirmed && carryTargetRequested != null)
                {
                    carryTargetRequested.GrabReleased_RPC(weaponController.fireOrigin.forward * interactConfig.throwForce, carryTargetRequested.transform.position, true);
                    ReleaseGrabbedObject_RPC(carryTargetRequested, true);
                    weaponController.animator.TriggerAnimation("Throw", 0.2f, true);
                    InputManager.PrimaryInput = false;
                    carryConfirmed = false;
                    carryTargetRequested = null;
                }
                if (InputManager.SecondaryInput && carryConfirmed && carryTargetRequested != null)
                {
                    carryTargetRequested.GrabReleased_RPC(Vector3.zero, carryTargetRequested.transform.position, false);
                    ReleaseGrabbedObject_RPC(carryTargetRequested, false);
                    InputManager.SecondaryInput = false;
                    weaponController.animator.TriggerAnimation("Grab_Cancel", 0.2f, true);
                    carryConfirmed = false;
                    carryTargetRequested = null;
                }
            }
        }
    }
    public override void OnNetworkDespawn()
    {
        playersByID.Remove(OwnerClientId);

        if(reviveItemInstance != null && IsServer)
        {
            reviveItemInstance.Despawn();
        }

        base.OnNetworkDespawn();
    }

    public override void DamageableDied(NetworkBehaviourReference sourceObj, bool isCrit)
    {
        base.DamageableDied(sourceObj, isCrit);

        deathParticle.Play();

        capsule.enabled = false;

        ToggleRenderers(false);

        ToggleHitboxes(false);


        Debug.Log($"Player died at time: {System.DateTime.Now}");

        for (int i = 0; i < weaponController.weapons.Count; i++)
        {
            weaponController.ShowWeapon(0, true);
        }

        if (IsServer)
        {
            if(reviveItemInstance == null)
            {
                reviveItemInstance = NetworkManager.SpawnManager.InstantiateAndSpawn(playerReviveItemPrefab, OwnerClientId, position: transform.position);
                reviveItemInstance.GetComponent<ReviveTrophy>().targetClientID = OwnerClientId;
                Debug.Log($"Spawned trophy - revive item instance null: {reviveItemInstance == null}");
            }
        }

        if(!isDead.Value && IsServer)
        {
            isDead.Value = true;
        }


    }
    [Rpc(SendTo.Everyone)]
    public void Revive_RPC(ulong helperClientID, bool quickRevive)
    {
        Revive(helperClientID, quickRevive);
    }

    public virtual void Revive(ulong helperClientID, bool quickRevive)
    {
        capsule.enabled = true;

        ToggleRenderers(true);

        ToggleHitboxes(true);


        Debug.Log($"Player revived at time: {System.DateTime.Now}");

        weaponController.ShowWeapon(weaponController.weaponIndex.Value, false);


        if (IsOwner)
        {
            if(reviveItemInstance != null)
                GetComponent<NetworkTransform>().Teleport(reviveItemInstance.transform.position + Vector3.up, transform.rotation, Vector3.one);
        }

        if (IsServer)
        {
            if(reviveItemInstance != null)
                reviveItemInstance.Despawn();

            isDead.Value = false;

            currentHealth.Value = maxHealth * (quickRevive ? quickReviveHealthPortion : 1);
        }



    }

    public void ToggleRenderers(bool enabled)
    {
        for (int i = 0; i < allRenderers.Length; i++)
        {
            allRenderers[i].enabled = enabled;
        }
    }
    public void ToggleHitboxes(bool enabled)
    {
        for (int i = 0; i < playerHitboxes.Length; i++)
        {
            playerHitboxes[i].enabled = enabled;
        }
    }

    public void InteractionCompleted(bool holdInteraction, bool finished)
    {
        heldInteraction = false;
        if (IsOwner)
        {
            InputManager.InteractInput = false;
        }
        currentInteractTarget = null;
    }

    [Rpc(SendTo.Everyone)]
    public void ConfirmGrabRequest_RPC(NetworkBehaviourReference objectTriedToGrab)
    {
        Debug.Log("Confirmed Grab Request!");
        if(objectTriedToGrab.TryGet(out InteractableObject io))
        {
            if (IsOwner)
            {
                if(io == carryTargetRequested)
                {
                    carryConfirmed = true;
                }
            }
            io.GrabbedCarriable();
            io.rb.velocity = Vector3.zero;
            io.rb.angularVelocity = Vector3.zero;
            weaponController.ShowWeapon(0, true);
            
        }
    }

    [Rpc(SendTo.Everyone)]
    public void ReleaseGrabbedObject_RPC(NetworkBehaviourReference objectReleased, bool thrown = true)
    {
        carryConfirmed = false;

        if (IsOwner)
        {
            carryTargetRequested = null;
        }
    }

    public IEnumerator CancelGrabAfterTime()
    {
        float t = 0;
        while (t < 2 && !carryConfirmed)
        {
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (!carryConfirmed)
        {
            weaponController.animator.TriggerAnimation("Grab_Cancel", 0.1f, true);
        }
    }
}
