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
            DateTime = DateTime.Today.AddHours(8);
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            DateTime.AddSeconds(deltaTime);
            var time = DateTime.TimeOfDay.TotalMilliseconds;

            var commandBuffer = Barrier.CreateCommandBuffer().AsParallelWriter();

            var buildingAssignmentSystem = EntityManager.World.GetOrCreateSystem<BuildingAssignmentSystem>();
            var buildingToPosition = buildingAssignmentSystem.BuildingToPosition;
            var buildingHeight = buildingAssignmentSystem.BuildingHeight;

            Entities
                .WithName("ScheduleMainJob")
                .WithReadOnly(buildingToPosition)
                .WithAll<HumanData>()
                .WithNone<NavNeedsDestination, QueuedForDestination>()
                .ForEach((Entity human, int entityInQueryIndex, int nativeThreadIndex, ref HumanBuildingData buildingData, in HumanScheduleData humanScheduleData) =>
                {
                    if (buildingData.Location == Location.Moving)
                        return;

                    if (buildingData.Location == Location.Residence && time >= humanScheduleData.WorkStart) // Go to work.
                    {
                        var workPos = buildingToPosition[buildingData.Work];
                        commandBuffer.AddComponent(entityInQueryIndex, human, new QueuedForDestination
                        {
                            Destination = new float3(workPos.x, buildingHeight, workPos.y)
                        });
                    }
                    else if (buildingData.Location == Location.Work && time >= humanScheduleData.WorkEnd) // Go to residence.
                    {
                        var housePos = buildingToPosition[buildingData.Residence];
                        commandBuffer.AddComponent(entityInQueryIndex, human, new QueuedForDestination
                        {
                            Destination = new float3(housePos.x, buildingHeight, housePos.y)
                        });
                    }

                    //commandBuffer.RemoveComponent<HumanData>(entityInQueryIndex, human); //TEMP
                })
                .ScheduleParallel();

            Barrier.AddJobHandleForProducer(Dependency);
        }
    }
}