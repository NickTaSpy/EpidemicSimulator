using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Epsim.Human
{
    public struct HumanBuildingData : IComponentData
    {
        public int Residence;
        public int Work;
    }
}
