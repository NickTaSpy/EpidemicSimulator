using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Epsim.Human
{
    public struct HumanScheduleData : IComponentData
    {
        public float WorkStart;
        public float WorkEnd;
    }
}