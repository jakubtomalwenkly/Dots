using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;

[GenerateAuthoringComponent]
[Serializable]
public struct HealthData : IComponentData
{
    public float fullHealth;
    public float currentHealth;
}
