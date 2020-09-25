using Reese.Nav;
using Reese.Random;
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
    public class HumanSystem : SystemBase
    {
        private EntityCommandBufferSystem ECB;

        protected override void OnCreate()
        {
            Enabled = false;
            ECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commandBuffer = ECB.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithName("HumanMainJob")
                .WithNone<NavNeedsDestination, HumanInsideBuildingData>()
                .WithAll<HumanData, NavAgent>()
                .ForEach((Entity human, int entityInQueryIndex, int nativeThreadIndex, in HumanBuildingData humanBuildingData) =>
                {
                    if (humanBuildingData.Location != Location.Moving)
                    {
                        commandBuffer.AddComponent<HumanInsideBuildingData>(entityInQueryIndex, human);
                    }
                })
                .ScheduleParallel();

            ECB.AddJobHandleForProducer(Dependency);
        }
    }
}