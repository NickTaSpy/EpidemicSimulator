using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace Epsim.Human
{
    public struct UnassignedBuildingsBuffer : IBufferElementData
    {
        public int UnassignedBuilding;
    }
}
