using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Epsim.Human
{
    public enum Location
    {
        Residence,
        Work,
        MovingHome,
        MovingWork
    }

    public struct HumanBuildingData : IComponentData
    {
        public int Residence;
        public int Work;
        public Location Location;
    }
}
