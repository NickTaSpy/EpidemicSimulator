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
        private EntityCommandBufferSystem Barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        private DateTime DateTime;

        protected override void OnCreate()
        {
            Enabled = false;
            DateTime = DateTime.Today.AddHours(6);
        }

        protected override void OnUpdate()
        {
            var commandBuffer = Barrier.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithName("HumanMainJob")
                .WithNone<NavNeedsDestination>()
                .WithAll<HumanData, NavAgent>()
                .ForEach((Entity human, int entityInQueryIndex, int nativeThreadIndex, in Translation translation) =>
                {
                    
                })
                .ScheduleParallel();

            Barrier.AddJobHandleForProducer(Dependency);
        }
    }
}