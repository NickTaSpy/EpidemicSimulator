using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Epsim.Profile.Data
{
    [Serializable]
    public class VirusProfile
    {
        public int InitiallyInfected;

        public float InfectionProbability;

        public float RecoveryProbability;

        public int ReproductionMin;
        public int ReproductionMax;

        public VirusProfile()
        {
            
        }
    }
}