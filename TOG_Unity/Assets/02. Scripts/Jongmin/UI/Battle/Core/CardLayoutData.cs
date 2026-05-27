using UnityEngine;

[System.Serializable]
public class CardLayoutData
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;

    public CardLayoutData(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
}