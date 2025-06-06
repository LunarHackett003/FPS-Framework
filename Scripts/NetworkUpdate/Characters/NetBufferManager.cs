using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetBufferManager : LunarNetScript
{
    [SerializeField] internal NetworkTimer timer;
    [SerializeField] internal int expectedServerTickRate = 30;
    [SerializeField] internal int bufferSize = 1024;
    internal float TickMinTime => timer.MinTimeBetweenTicks;
    [SerializeField] internal float reconciliationPositionThreshold = 2f;

    //------------
    // CLIENT STATE
    //------------

    /// <summary>
    /// The Client's state buffer
    /// </summary>
    internal CircularBuffer<StatePayload> clientStateBuffer;
    /// <summary>
    /// The Client's input buffer
    /// </summary>
    internal CircularBuffer<InputPayload> clientInputBuffer;
    /// <summary>
    /// The last state payload we received from the server
    /// </summary>
    internal StatePayload lastServerState;
    /// <summary>
    /// The last state payload we received AND reconciled to
    /// </summary>
    internal StatePayload lastProcessedState;

    //------------
    // SERVER STATE
    //------------

    internal CircularBuffer<StatePayload> serverStateBuffer;
    internal Queue<InputPayload> serverInputQueue;
    public bool ShouldTick => timer != null && timer.ShouldTick;
    public int Tick => timer == null ? -1 : timer.CurrentTick;

    public float TickTime =>  Time.fixedDeltaTime;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        expectedServerTickRate = (int)NetworkManager.NetworkTickSystem.TickRate;
        timer = new(expectedServerTickRate);

        clientInputBuffer = new((int)bufferSize);
        clientStateBuffer = new((int)bufferSize);
        serverStateBuffer = new((int)bufferSize);
        serverInputQueue = new();
    }
    [Rpc(SendTo.Server)]
    public void SendInputPayload_RPC(InputPayload payload)
    {
        serverInputQueue.Enqueue(payload);
    }
    [Rpc(SendTo.Owner)]
    public void SendStateToClient_RPC(StatePayload payload)
    {
        if (!IsOwner)
            return;
        lastServerState = payload;   
    }

    public override void LTimestep()
    {
        base.LTimestep();
        timer.Update(Time.fixedDeltaTime);
    }
}
