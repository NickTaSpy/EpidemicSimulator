using Reese.Nav;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
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

            RequireSingletonForUpdate<NavProblemCount>();

            var entity = EntityManager.CreateEntity(typeof(NavProblemCount));
#if UNITY_EDITOR
            EntityManager.SetName(entity, "NavProblemCount Singleton");
#endif
            SetSingleton(new NavProblemCount { Value = 0 });
        }

        protected override void OnUpdate()
        {
            var commandBuffer = ECB.CreateCommandBuffer().AsParallelWriter();

            var navProblemCount = new NativeArray<NavProblemCount>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            navProblemCount[0] = GetSingleton<NavProblemCount>();

            Entities
                .WithName("HandleNavProblems")
                .WithNativeDisableContainerSafetyRestriction(navProblemCount)
                .WithAll<NavHasProblem>()
                .ForEach((Entity human, int entityInQueryIndex) =>
                {
                    navProblemCount[0] += 1;
                    commandBuffer.AddComponent(entityInQueryIndex, human, new Disabled());
                })
                .Schedule();

            ECB.AddJobHandleForProducer(Dependency);

            Dependency.Complete();

            SetSingleton(navProblemCount[0]);
            navProblemCount.Dispose();
        }
    }
}