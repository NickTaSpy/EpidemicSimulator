using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Epsim.Human
{
    public struct BuildingEntrancesBuffer : IBufferElementData
    {
        public int ID;
        public float3 Entrance;
    }
}
