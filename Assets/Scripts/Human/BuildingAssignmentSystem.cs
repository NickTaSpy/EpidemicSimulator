using Reese.Nav;
using Reese.Random;
using Reese.Spawning;
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
    public class BuildingAssignmentSystem : SystemBase
    {
        public float BuildingHeight;
        public NativeHashMap<int, float2> BuildingToPosition;
        public NativeList<int2> Pairs;

        private EntityCommandBufferSystem Barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        private ScheduleSystem ScheduleSystem;

        protected override void OnCreate()
        {
            ScheduleSystem = World.GetOrCreateSystem<ScheduleSystem>();
        }

        protected override void OnStartRunning()
        {
            ScheduleSystem.Enabled = false;
        }

        protected override void OnUpdate()
        {
            if (!Pairs.IsCreated || Pairs.Length < 2)
            {
                ScheduleSystem.Enabled = true;
                return;
            }

            var commandBuffer = Barrier.CreateCommandBuffer().AsParallelWriter();

            var buildingToPosition = BuildingToPosition;
            var pairs = Pairs;
            var buildingHeight = BuildingHeight;

            Entities
                .WithName("BuldingAssignmentJob")
                .WithReadOnly(buildingToPosition)
                .WithAll<HumanData>()
                .WithNone<HumanBuildingData>()
                .ForEach((Entity human, int entityInQueryIndex, int nativeThreadIndex) =>
                {
                    if (pairs.Length < 2)
                    {
                        return;
                    }

                    int last = pairs.Length - 1;
                    int residence = pairs[last].x;
                    int work = pairs[last].y;
                    commandBuffer.AddComponent(entityInQueryIndex, human, new HumanBuildingData
                    {
                        Residence = residence,
                        Work = work
                    });
                    pairs.RemoveAtSwapBack(last);

                    var residencePos = buildingToPosition[residence];
                    commandBuffer.AddComponent(entityInQueryIndex, human, new NavNeedsDestination
                    {
                        Destination = new float3(residencePos.x, buildingHeight, residencePos.y),
                        Teleport = true
                    });
                })
                .Schedule();
            
            Barrier.AddJobHandleForProducer(Dependency);
        }

        protected override void OnStopRunning()
        {
            ScheduleSystem.Enabled = true;
        }

        protected override void OnDestroy()
        {
            if (BuildingToPosition.IsCreated)
                BuildingToPosition.Dispose();

            if (Pairs.IsCreated)
                Pairs.Dispose();
        }
    }
}
