using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Epsim.Profile.Data
{
    [Serializable]
    public class ScheduleProfile
    {
        public int WorkStart;

        public int WorkDurationHours;

        public ScheduleProfile()
        {

        }

        public ScheduleProfile(int workStart, int workDurationHours)
        {
            WorkStart = workStart;
            WorkDurationHours = workDurationHours;
        }
    }
}