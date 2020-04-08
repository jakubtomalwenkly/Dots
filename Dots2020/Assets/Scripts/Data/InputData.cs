using UnityEngine;
using Unity.Entities;
using System;

[GenerateAuthoringComponent]
[Serializable]
public struct InputData : IComponentData
{
    public KeyCode upKey;
    public KeyCode downKey;
    public KeyCode rightKey;
    public KeyCode lefTKey;
    public KeyCode fireKey;
    public bool IsFiring;
}

