using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Epsim.Human
{
    public struct NavProblemCount : IComponentData
    {
        public int Value;

        public static implicit operator int(NavProblemCount x) => x.Value;
        public static NavProblemCount operator +(NavProblemCount x, int i) => new NavProblemCount { Value = x.Value + i };
    }
}