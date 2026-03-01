using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct IsaacMapGenPayload : INetworkSerializable
{
    public int seed;

    public Vector2Int gridSize;
    public Vector3 cubeSize;

    public int maxRooms;
    public int specialRoomCount;
    public int horizontalSize;
    public int verticalSize;
    public int spacing;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref seed);
        serializer.SerializeValue(ref gridSize);
        serializer.SerializeValue(ref cubeSize);
        serializer.SerializeValue(ref maxRooms);
        serializer.SerializeValue(ref specialRoomCount);
        serializer.SerializeValue(ref horizontalSize);
        serializer.SerializeValue(ref verticalSize);
        serializer.SerializeValue(ref spacing);
    }
}