using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Epsim.Human
{
    public struct HumanData : IComponentData
    {
        public int Age;
        public bool Male;

        public Status Status;
        public float InfectionProbability;
        public float RecoveryProbability;
        public int TransmissionsRemaining;
    }
}
