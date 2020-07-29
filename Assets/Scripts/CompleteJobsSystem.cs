using Epsim.Human;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Epsim
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public class CompleteJobsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var scheduleSystem = EntityManager.World.GetOrCreateSystem<ScheduleSystem>();
            scheduleSystem.ScheduleJobHandle.Complete();
        }
    }
}