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
        private const float DestinationCheckRange = 1.5f;

        public float TimeScale = 1f;
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
            DateTime = DateTime.AddSeconds(deltaTime * TimeScale);
            var time = DateTime.TimeOfDay.TotalMilliseconds;

            var commandBuffer = ECB.CreateCommandBuffer().AsParallelWriter();

            var buildingAssignmentSystem = World.GetOrCreateSystem<BuildingAssignmentSystem>();
            var buildingToPosition = buildingAssignmentSystem.BuildingToPosition;
            var buildingHeight = buildingAssignmentSystem.BuildingHeight;

            var destinationQueue = World.GetOrCreateSystem<QueueNavSystem>().GetParallelQueue();

            Entities
                .WithName("ScheduleMainJob")
                .WithReadOnly(buildingToPosition)
                .WithAll<HumanData>()
                .WithNone<NavNeedsDestination>()
                .WithNativeDisableContainerSafetyRestriction(destinationQueue)
                .ForEach((Entity human, int entityInQueryIndex, int nativeThreadIndex, ref HumanBuildingData buildingData, in HumanScheduleData humanScheduleData, in Translation translation) =>
                {
                    var workPos = buildingToPosition[buildingData.Work];
                    var housePos = buildingToPosition[buildingData.Residence];

                    if (buildingData.Location == Location.MovingHome)
                    {   
                        if (housePos.x.Approx(translation.Value.x, DestinationCheckRange) && housePos.y.Approx(translation.Value.z, DestinationCheckRange)) // Arrived at residence.
                        {
                            buildingData.Location = Location.Residence;
                            commandBuffer.AddComponent(entityInQueryIndex, human, new HumanInsideBuildingData
                            {
                                Building = buildingData.Residence,
                                Contacts = 0
                            });
                        }
                    }
                    else if (buildingData.Location == Location.MovingWork)
                    {
                        if (workPos.x.Approx(translation.Value.x, DestinationCheckRange) && workPos.y.Approx(translation.Value.z, DestinationCheckRange)) // Arrived at work.
                        {
                            buildingData.Location = Location.Work;
                            commandBuffer.AddComponent(entityInQueryIndex, human, new HumanInsideBuildingData
                            {
                                Building = buildingData.Work,
                                Contacts = 0
                            });
                        }
                    }
                    else if (buildingData.Location == Location.Residence && time >= humanScheduleData.WorkStart && time < humanScheduleData.WorkEnd) // Go to work.
                    {
                        destinationQueue.Enqueue(new DestinationRequest(human, new float3(workPos.x, buildingHeight, workPos.y)));
                        buildingData.Location = Location.MovingWork;
                        commandBuffer.RemoveComponent<HumanInsideBuildingData>(entityInQueryIndex, human);
                    }
                    else if (buildingData.Location == Location.Work && time >= humanScheduleData.WorkEnd) // Go to residence.
                    {
                        destinationQueue.Enqueue(new DestinationRequest(human, new float3(housePos.x, buildingHeight, housePos.y)));
                        buildingData.Location = Location.MovingHome;
                        commandBuffer.RemoveComponent<HumanInsideBuildingData>(entityInQueryIndex, human);
                    }

                    //commandBuffer.RemoveComponent<HumanData>(entityInQueryIndex, human); // TEMP: Use for debugging
                })
                .ScheduleParallel();

            ECB.AddJobHandleForProducer(Dependency);
        }
    }
}