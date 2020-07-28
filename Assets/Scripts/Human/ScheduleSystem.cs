using Reese.Nav;
using Reese.Random;
using Reese.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Epsim.Human
{
    public class ScheduleSystem : SystemBase
    {
        private DateTime DateTime;

        private EntityCommandBufferSystem Barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnCreate()
        {
            Enabled = false;
        }

        protected override void OnStartRunning()
        {
            DateTime = DateTime.Today.AddHours(6);
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            DateTime.AddSeconds(deltaTime);

            var commandBuffer = Barrier.CreateCommandBuffer().AsParallelWriter();

            var buildingAssignmentSystem = EntityManager.World.GetOrCreateSystem<BuildingAssignmentSystem>();
            var buildingToPosition = buildingAssignmentSystem.BuildingToPosition;
            var buildingHeight = buildingAssignmentSystem.BuildingHeight;

            Entities
                .WithName("ScheduleMainJob")
                .WithReadOnly(buildingToPosition)
                .WithAll<HumanData>()
                .WithNone<NavNeedsDestination>()
                .ForEach((Entity human, int entityInQueryIndex, int nativeThreadIndex, in HumanBuildingData buildingData) =>
                {
                    var workPos = buildingToPosition[buildingData.Work];
                    commandBuffer.AddComponent(entityInQueryIndex, human, new NavNeedsDestination
                    {
                        Destination = new float3(workPos.x, buildingHeight, workPos.y)
                    });

                    commandBuffer.RemoveComponent<HumanData>(entityInQueryIndex, human); //TEMP
                })
                .ScheduleParallel();

            Barrier.AddJobHandleForProducer(Dependency);
        }
    }
}