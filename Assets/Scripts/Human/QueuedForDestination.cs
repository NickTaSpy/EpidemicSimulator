using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Epsim.Human
{
    public struct QueuedForDestination : IComponentData
    {
        public float3 Destination;
    }
}