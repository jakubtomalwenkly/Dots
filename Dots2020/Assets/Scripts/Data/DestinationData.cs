using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct DestinationData : IComponentData
{
    public Entity destination;
}
