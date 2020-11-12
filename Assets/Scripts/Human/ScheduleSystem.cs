using Reese.Nav;
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
        public float TimeScale { get; private set; } = 3600f;

        public DateTime DateTime;

        private const float DestinationCheckRange = 1.5f;
        private const float DefaultAgentSpeed = 20f;

        private EntityCommandBufferSystem ECB;

        private bool TimeScaleChanged = true;

        public void SetTimeScale(float value)
        {
            TimeScale = value;
            TimeScaleChanged = true;
        }

        protected override void OnCreate()
        {
            Enabled = false;
            ECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            DateTime = DateTime.Today;
        }

        protected override void OnStartRunning()
        {
            DateTime = DateTime.AddHours(8);
        }

        protected override void OnUpdate()
        {
            if (TimeScaleChanged)
            {
                TimeScaleChanged = false;
                float newSpeed = DefaultAgentSpeed * TimeScale;

                Entities
                    .ForEach((Entity human, ref NavAgent navAgent) =>
                    {
                        navAgent.TranslationSpeed = newSpeed;
                    })
                    .ScheduleParallel();
            }

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
                            commandBuffer.AddComponent<HumanInBuildingData>(entityInQueryIndex, human);
                        }
                    }
                    else if (buildingData.Location == Location.MovingWork)
                    {
                        if (workPos.x.Approx(translation.Value.x, DestinationCheckRange) && workPos.y.Approx(translation.Value.z, DestinationCheckRange)) // Arrived at work.
                        {
                            buildingData.Location = Location.Work;
                            commandBuffer.AddComponent<HumanInBuildingData>(entityInQueryIndex, human);
                        }
                    }
                    else if (buildingData.Location == Location.Residence && time >= humanScheduleData.WorkStart && time < humanScheduleData.WorkEnd) // Go to work.
                    {
                        destinationQueue.Enqueue(new DestinationRequest(human, new float3(workPos.x, buildingHeight, workPos.y)));
                        buildingData.Location = Location.MovingWork;
                        commandBuffer.RemoveComponent<HumanInBuildingData>(entityInQueryIndex, human);
                    }
                    else if (buildingData.Location == Location.Work && time >= humanScheduleData.WorkEnd) // Go to residence.
                    {
                        destinationQueue.Enqueue(new DestinationRequest(human, new float3(housePos.x, buildingHeight, housePos.y)));
                        buildingData.Location = Location.MovingHome;
                        commandBuffer.RemoveComponent<HumanInBuildingData>(entityInQueryIndex, human);
                    }

                    //commandBuffer.RemoveComponent<HumanData>(entityInQueryIndex, human); // TEMP: Use for debugging
                })
                .ScheduleParallel();

            ECB.AddJobHandleForProducer(Dependency);
        }
    }
}