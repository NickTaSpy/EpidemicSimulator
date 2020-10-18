using Reese.Nav;
using Reese.Random;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Epsim.Human
{
    public class HumanSystem : SystemBase
    {
        private EntityQuery Query;

        private EntityCommandBufferSystem ECB;

        protected override void OnCreate()
        {
            ECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commandBuffer = ECB.CreateCommandBuffer().AsParallelWriter();

            var humans = Query.ToEntityArray(Allocator.TempJob);
            var humanInBuildingDataAll = Query.ToComponentDataArray<HumanInsideBuildingData>(Allocator.TempJob);
            //var humanDataAll = Query.ToComponentDataArray<HumanData>(Allocator.TempJob);

            Entities
                .WithName("CalculateContactsJob")
                .WithStoreEntityQueryInField(ref Query)
                .WithReadOnly(humans)
                .WithReadOnly(humanInBuildingDataAll)
                //.WithReadOnly(humanDataAll)
                .WithAll<NavAgent, HumanInsideBuildingData>()
                .WithNone<NavNeedsDestination>()
                .ForEach((Entity human, int entityInQueryIndex, in HumanData humanData) =>
                {
                    if (humanData.Status == Status.Infected)
                    {
                        var humanInsideBuildingData = humanInBuildingDataAll[entityInQueryIndex];

                        for (int i = 0; i < humans.Length; i++)
                        {
                            if (i == entityInQueryIndex)
                            {
                                continue;
                            }

                            if (humanInBuildingDataAll[i].Building == humanInsideBuildingData.Building)
                            {
                                SetComponent(humans[i], new HumanInsideBuildingData
                                {
                                    Building = humanInBuildingDataAll[i].Building,
                                    Contacts = humanInBuildingDataAll[i].Contacts + 1
                                });
                            }
                        }
                    }
                })
                .WithDisposeOnCompletion(humans)
                .WithDisposeOnCompletion(humanInBuildingDataAll)
                //.WithDisposeOnCompletion(humanDataAll)
                .Schedule();

            ECB.AddJobHandleForProducer(Dependency);
        }
    }
}