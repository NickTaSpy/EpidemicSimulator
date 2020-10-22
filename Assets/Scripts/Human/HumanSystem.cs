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
    public struct HumanInBuildingSystemData : ISystemStateComponentData
    {
        public int Building;
        public int Contacts;
    }

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
            
            Entities
                .WithName("HumanEnterBuildingJob")
                .WithAll<NavAgent, HumanData, HumanInBuildingData>()
                .WithNone<NavNeedsDestination, HumanInBuildingSystemData>()
                .ForEach((Entity human, int entityInQueryIndex, in HumanBuildingData humanBuildingData) =>
                {
                    commandBuffer.AddComponent(entityInQueryIndex, human, new HumanInBuildingSystemData
                    {
                        Building = humanBuildingData.Location == Location.Work ? humanBuildingData.Work : humanBuildingData.Residence
                    });
                })
                .ScheduleParallel();

            var humans = Query.ToEntityArray(Allocator.TempJob);
            var humanInBuildingSystemDataAll = Query.ToComponentDataArray<HumanInBuildingSystemData>(Allocator.TempJob);
            //var humanDataAll = Query.ToComponentDataArray<HumanData>(Allocator.TempJob);

            Entities
                .WithName("HumanInBuildingJob")
                .WithStoreEntityQueryInField(ref Query)
                .WithReadOnly(humans)
                //.WithReadOnly(humanDataAll)
                .WithAll<NavAgent, HumanInBuildingData, HumanInBuildingSystemData>()
                .WithNone<NavNeedsDestination>()
                .ForEach((Entity human, int entityInQueryIndex, in HumanData humanData) =>
                {
                    if (humanData.Status == Status.Infected)
                    {
                        var humanInBuildingData = humanInBuildingSystemDataAll[entityInQueryIndex];

                        for (int i = 0; i < humans.Length; i++)
                        {
                            if (i == entityInQueryIndex)
                            {
                                continue;
                            }

                            if (humanInBuildingSystemDataAll[i].Building == humanInBuildingData.Building)
                            {
                                var data = new HumanInBuildingSystemData
                                {
                                    Building = humanInBuildingSystemDataAll[i].Building,
                                    Contacts = humanInBuildingSystemDataAll[i].Contacts + 1
                                };

                                SetComponent(humans[i], data);
                            }
                        }
                    }
                })
                .WithDisposeOnCompletion(humans)
                .WithDisposeOnCompletion(humanInBuildingSystemDataAll)
                //.WithDisposeOnCompletion(humanDataAll)
                .Schedule();

            Entities
                .WithName("HumanExitBuildingJob")
                .WithAll<NavAgent, HumanData>()
                .WithNone<HumanInBuildingData>()
                .ForEach((Entity human, int entityInQueryIndex, in HumanInBuildingSystemData data) =>
                {
                    commandBuffer.RemoveComponent<HumanInBuildingSystemData>(entityInQueryIndex, human);

                    // Deal with data
                })
                .ScheduleParallel();

            ECB.AddJobHandleForProducer(Dependency);
        }
    }
}