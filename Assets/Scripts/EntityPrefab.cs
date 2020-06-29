using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Epsim
{
    [GenerateAuthoringComponent]
    public class EntityPrefab : IComponentData
    {
        public Entity Value;
    }
}