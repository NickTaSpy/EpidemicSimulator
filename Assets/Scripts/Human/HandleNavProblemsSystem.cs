using Reese.Nav;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Epsim.Human
{
    public class HandleNavProblemsSystem : SystemBase
    {
        private EntityCommandBufferSystem ECB;

        protected override void OnCreate()
        {
            ECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commandBuffer = ECB.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithName("HandleNavProblems")
                .WithAll<NavHasProblem>()
                .ForEach((Entity human, int entityInQueryIndex, int nativeThreadIndex) =>
                {
                    //commandBuffer.DestroyEntity(entityInQueryIndex, human);
                    commandBuffer.RemoveComponent<NavAgent>(entityInQueryIndex, human);
                })
                .ScheduleParallel();

            ECB.AddJobHandleForProducer(Dependency);
        }
    }
}