using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ReviveTrophy : InteractableObject
{

    public MeshRenderer renderer;
    public Material friendlyMaterial, enemyMaterial;

    public ulong targetClientID;
    internal bool friendly;

    public override bool CanInteract(ulong attemptedInteractor)
    {
        return base.CanInteract(attemptedInteractor) && friendly;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        targetClientID = OwnerClientId;

        NetPlayerEntity.playersByID[targetClientID].reviveItemInstance = NetworkObject;
        NetworkPlayer.netPlayers[OwnerClientId].onTeamUpdated += UpdateTeam;
        NetworkPlayer.netPlayers[NetworkManager.LocalClientId].onTeamUpdated += UpdateTeam;
        UpdateTeam();
    }

    public override void LUpdate()
    {
        base.LUpdate();
    }

    public void UpdateTeam()
    {
        friendly = NetworkPlayer.IsPlayerOnMyTeam(NetworkManager.LocalClientId, OwnerClientId);

        renderer.material = friendly ? friendlyMaterial : enemyMaterial;
    }

    public override void InteractionCompleted()
    {
        base.InteractionCompleted();

        NetPlayerEntity.playersByID[targetClientID].Revive_RPC(currentInteractor.OwnerClientId, false);
    }

    public bool HitByQuickRevive(ulong clientID)
    {
        if (friendly)
        {
            NetPlayerEntity.playersByID[targetClientID].Revive_RPC(clientID, true);
            return true;
        }
        return false;

    }
}
