using Reese.Nav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;

namespace Epsim.Human
{
    [DisableAutoCreation]
    public class StatisticsCollectionSystem : SystemBase
    {
        public int SusceptibleCount => SusceptibleCounts.Sum();
        public int InfectedCount => InfectedCounts.Sum();
        public int RecoveredCount => RecoveredCounts.Sum();

        private NativeArray<int> SusceptibleCounts = new NativeArray<int>(JobsUtility.MaxJobThreadCount, Allocator.Persistent);
        private NativeArray<int> InfectedCounts = new NativeArray<int>(JobsUtility.MaxJobThreadCount, Allocator.Persistent);
        private NativeArray<int> RecoveredCounts = new NativeArray<int>(JobsUtility.MaxJobThreadCount, Allocator.Persistent);

        public void Complete() => CompleteDependency();

        protected override void OnUpdate()
        {
            var susceptibleCounts = SusceptibleCounts;
            var infectedCounts = InfectedCounts;
            var recoveredCounts = RecoveredCounts;

            Reset(susceptibleCounts);
            Reset(infectedCounts);
            Reset(recoveredCounts);

            Entities
                .WithAll<NavAgent>()
                .WithNativeDisableContainerSafetyRestriction(susceptibleCounts)
                .WithNativeDisableContainerSafetyRestriction(infectedCounts)
                .WithNativeDisableContainerSafetyRestriction(recoveredCounts)
                .ForEach((Entity human, int entityInQueryIndex, int nativeThreadIndex, in HumanData humanData) =>
                {
                    if (humanData.Status == Status.Susceptible)
                    {
                        susceptibleCounts[nativeThreadIndex] += 1;
                    }
                    else if (humanData.Status == Status.Infected)
                    {
                        infectedCounts[nativeThreadIndex] += 1;
                    }
                    else if (humanData.Status == Status.Recovered)
                    {
                        recoveredCounts[nativeThreadIndex] += 1;
                    }
                })
                .ScheduleParallel();
        }

        private void Reset(NativeArray<int> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = 0;
            }
        }
    }
}
