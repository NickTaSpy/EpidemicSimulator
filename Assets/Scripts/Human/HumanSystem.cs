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
    [DisableAutoCreation]
    public class HumanSystem : SystemBase
    {
        private EntityCommandBufferSystem Barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        private DateTime DateTime;

        protected override void OnCreate()
        {
            DateTime = DateTime.Today.AddHours(6);
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            DateTime.AddSeconds(deltaTime);

            var commandBuffer = Barrier.CreateCommandBuffer().ToConcurrent();
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;

            Entities
                .WithName("HumanMainJob")
                .WithNone<NavNeedsDestination>()
                .WithAll<HumanData, NavAgent>()
                .ForEach((Entity human, int entityInQueryIndex, int nativeThreadIndex, in Translation translation) =>
                {
                    var random = randomArray[nativeThreadIndex];

                    if (translation.Value.x > 27f && translation.Value.x < 32f)
                    {
                        commandBuffer.AddComponent(entityInQueryIndex, human, new NavNeedsDestination
                        {
                            Destination = new float3(0f, 0.1f, 0f)
                        });
                    }
                    else
                    {
                        commandBuffer.AddComponent(entityInQueryIndex, human, new NavNeedsDestination
                        {
                            Destination = new float3(30f, 0.1f, 30f),
                            Teleport = false
                        });
                    }

                    randomArray[nativeThreadIndex] = random;
                })
                .ScheduleParallel();

            Barrier.AddJobHandleForProducer(Dependency);
        }
    }
}