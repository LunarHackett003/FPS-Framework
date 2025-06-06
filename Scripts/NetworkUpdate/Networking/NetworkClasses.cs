using Unity.Netcode;
using UnityEngine;

public struct InputPayload : INetworkSerializable
{
    public int tick;
    public Vector2 moveInput;
    public bool sprintInput, crouchInput, jumpInput;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref moveInput);
        serializer.SerializeValue(ref sprintInput);
        serializer.SerializeValue(ref crouchInput);
        serializer.SerializeValue(ref jumpInput);
    }
}

public struct StatePayload : INetworkSerializable
{
    public int tick;
    public Vector3 velocity, position;
    public bool crouching, sliding, sprinting;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref velocity);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref sliding);
        serializer.SerializeValue(ref crouching);
        serializer.SerializeValue(ref sprinting);
    }
}