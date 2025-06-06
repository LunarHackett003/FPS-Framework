using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : LunarNetScript
{

    public bool canCarry;

    public NetworkVariable<bool> beingCarried = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool displayInteractText;
    public string interactText = "Interact with me!";

    public bool holdToInteract;
    public float interactTime;

    public bool hasInteraction;

    public UnityEvent onInteract;

    public Collider[] colliders;

    public virtual bool CanInteract(ulong attemptedInteractor)
    {
        return !interactionInProgress && !beingCarried.Value;
    }

    internal float currentInteractTime;

    public bool interactionInProgress;
    bool cancelled = true;
    public ulong currentInteractingPlayer;



    internal NetPlayerEntity currentInteractor;

    [SerializeField] internal Rigidbody rb;
    public Quaternion grabRotationOffset = Quaternion.identity;

    public UnityEvent onThrow;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        CarriedUpdated(false, beingCarried.Value);
        beingCarried.OnValueChanged += CarriedUpdated;
    }

    void CarriedUpdated(bool previous, bool current)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = !current;
        }
        if (IsOwner)
        {
            if(rb != null)
            {
                rb.useGravity = !current;
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    public void InteractStart_RPC(ulong clientID)
    {
        currentInteractingPlayer = clientID;
        if (holdToInteract)
        {
            interactionInProgress = true;
            InteractionStarted();
        }
        else
        {
            InteractionCompleted();
        }

        currentInteractor = NetPlayerEntity.playersByID[currentInteractingPlayer];
    }

    [Rpc(SendTo.Everyone)]
    public void InteractEnd_RPC(bool finished)
    {
        interactionInProgress = false;
        currentInteractTime = 0;
        currentInteractor.InteractionCompleted(holdToInteract, finished);

        if (IsServer && finished)
        {
            onInteract?.Invoke();
        }
    }

    public override void LTimestep()
    {
        base.LTimestep();
        if (!interactionInProgress && !cancelled)
        {
            InteractionCancelled();
            return;
        }

        if (interactionInProgress)
        {
            if(currentInteractTime < interactTime)
            {
                currentInteractTime += Time.fixedDeltaTime;
            }
            else
            {
                InteractionCompleted();
            }
        }
        
    }

    public virtual void InteractionStarted()
    {
        cancelled = false;
    }

    public virtual void InteractionCompleted()
    {
        cancelled = true;
        if(IsServer)
            InteractEnd_RPC(true);
    }

    public virtual void InteractionCancelled()
    {
        cancelled = true;
    }

    [Rpc(SendTo.Server)]
    public void TryGrab_RPC(ulong clientID)
    {
        if (!interactionInProgress && OwnerClientId != clientID)
        {
            NetworkObject.ChangeOwnership(clientID);
        }
        beingCarried.Value = true;

        NetPlayerEntity.playersByID.TryGetValue(clientID, out var player);
        player.ConfirmGrabRequest_RPC(this);
    }

    [Rpc(SendTo.Server)]
    public void GrabReleased_RPC(Vector3 throwDirection, Vector3 throwOrigin, bool thrown)
    {
        if ((IsHost && OwnerClientId != 0) || !IsOwnedByServer)
        {
            NetworkObject.RemoveOwnership();
        }

        beingCarried.Value = false;

        if (!rb.isKinematic)
        {
            rb.position = throwOrigin;
            if(thrown)
            {
                rb.velocity = throwDirection;
            }
        }

        if(thrown)
        {
            onThrow?.Invoke();
        }
    }

    public virtual void GrabbedCarriable()
    {
        
    }
}
