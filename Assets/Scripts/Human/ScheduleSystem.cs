using Reese.Nav;
using Reese.Random;
using Reese.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Epsim.Human
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public class ScheduleSystem : SystemBase
    {
        private const int ArrayLength = 11;
        private const int InnerloopBatchCount = 1;

        public JobHandle ScheduleJobHandle;

        private DateTime DateTime;

        private readonly EntityQueryDesc Query = new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(NavNeedsDestination) },
            All = new ComponentType[] { ComponentType.ReadOnly<HumanBuildingData>(), ComponentType.ReadWrite<HumanData>() }
        };

        private EntityCommandBufferSystem ECB;

        protected override void OnCreate()
        {
            ECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            Enabled = false;
        }

        protected override void OnStartRunning()
        {
            DateTime = DateTime.Today.AddHours(6);
        }

        protected override void OnUpdate()
        {
            // Time
            var deltaTime = Time.DeltaTime;
            DateTime.AddSeconds(deltaTime);

            // ScheduleJob
            var buildingAssignmentSystem = EntityManager.World.GetOrCreateSystem<BuildingAssignmentSystem>();
            var query = GetEntityQuery(Query);

            var job = new ScheduleJob()
            {
                BuildingToPosition = buildingAssignmentSystem.BuildingToPosition,
                BuildingHeight = buildingAssignmentSystem.BuildingHeight,
                CommandBuffer = ECB.CreateCommandBuffer().AsParallelWriter(),
                Entities = query.ToEntityArray(Allocator.TempJob),
                BuildingData = query.ToComponentDataArray<HumanBuildingData>(Allocator.TempJob)
            };

            ScheduleJobHandle = job.Schedule(ArrayLength, InnerloopBatchCount, Dependency);
        }

        private struct ScheduleJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeHashMap<int, float2> BuildingToPosition;

            [ReadOnly]
            public float BuildingHeight;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<Entity> Entities;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<HumanBuildingData> BuildingData;

            [NativeDisableParallelForRestriction]
            [WriteOnly]
            public EntityCommandBuffer.ParallelWriter CommandBuffer;

            public void Execute(int i)
            {
                var entity = Entities[i];
                var buildingData = BuildingData[i];
                
                var workPos = BuildingToPosition[buildingData.Work];
                CommandBuffer.AddComponent(i, entity, new NavNeedsDestination
                {
                    Destination = new float3(workPos.x, BuildingHeight, workPos.y)
                });

                CommandBuffer.RemoveComponent<HumanData>(i, entity); //TEMP
            }
        }
    }
}