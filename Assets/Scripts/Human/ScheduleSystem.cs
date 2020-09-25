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

        private EntityCommandBufferSystem ECB;

        protected override void OnCreate()
        {
            Enabled = false;
            ECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnStartRunning()
        {
            DateTime = DateTime.Today.AddHours(8);
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            DateTime = DateTime.AddSeconds(deltaTime);
            var time = DateTime.TimeOfDay.TotalMilliseconds;

            var commandBuffer = ECB.CreateCommandBuffer().AsParallelWriter();

            var buildingAssignmentSystem = EntityManager.World.GetOrCreateSystem<BuildingAssignmentSystem>();
            var buildingToPosition = buildingAssignmentSystem.BuildingToPosition;
            var buildingHeight = buildingAssignmentSystem.BuildingHeight;

            Entities
                .WithName("ScheduleMainJob")
                .WithReadOnly(buildingToPosition)
                .WithAll<HumanData>()
                .WithNone<NavNeedsDestination, QueuedForDestination>()
                .ForEach((Entity human, int entityInQueryIndex, int nativeThreadIndex, ref HumanBuildingData buildingData, in HumanScheduleData humanScheduleData, in Translation translation) =>
                {
                    var workPos = buildingToPosition[buildingData.Work];
                    var housePos = buildingToPosition[buildingData.Residence];

                    if (buildingData.Location == Location.Moving)
                    {
                        // Detect if arrived at location.
                        if (workPos.x == translation.Value.x && workPos.y == translation.Value.z) // Arrived at work.
                        {
                            buildingData.Location = Location.Work;
                        }
                        else if (housePos.x == translation.Value.x && housePos.y == translation.Value.z) // Arrived at residence.
                        {
                            buildingData.Location = Location.Residence;
                        }
                        else // Still moving.
                        {
                            return;
                        }
                    }

                    if (buildingData.Location == Location.Residence && time >= humanScheduleData.WorkStart && time < humanScheduleData.WorkEnd) // Go to work.
                    {                        
                        commandBuffer.AddComponent(entityInQueryIndex, human, new QueuedForDestination
                        {
                            Destination = new float3(workPos.x, buildingHeight, workPos.y)
                        });
                        buildingData.Location = Location.Moving;
                    }
                    else if (buildingData.Location == Location.Work && time >= humanScheduleData.WorkEnd) // Go to residence.
                    {
                        commandBuffer.AddComponent(entityInQueryIndex, human, new QueuedForDestination
                        {
                            Destination = new float3(housePos.x, buildingHeight, housePos.y)
                        });
                        buildingData.Location = Location.Moving;
                    }

                    //commandBuffer.RemoveComponent<HumanData>(entityInQueryIndex, human); //TEMP
                })
                .ScheduleParallel();

            ECB.AddJobHandleForProducer(Dependency);
        }
    }
}