using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Epsim.Human
{
    public struct HumanInsideBuildingData : IComponentData
    {
        public int Building;
        public int Contacts;
    }
}
