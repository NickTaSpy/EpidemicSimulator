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
        private Unity.Mathematics.Random InfectionRand;

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

            Entities
                .WithName("HumanInBuildingJob")
                .WithStoreEntityQueryInField(ref Query)
                .WithReadOnly(humans)
                .WithReadOnly(humanInBuildingSystemDataAll)
                .WithAll<NavAgent, HumanInBuildingData, HumanInBuildingSystemData>()
                .WithNone<NavNeedsDestination>()
                .ForEach((Entity human, int entityInQueryIndex, ref HumanData humanData) =>
                {
                    if (humanData.Status == Status.Infected)
                    {
                        var humanInBuildingData = humanInBuildingSystemDataAll[entityInQueryIndex];

                        for (int i = 0; i < humans.Length; i++)
                        {
                            if (humanData.TransmissionsRemaining > 0)
                            {
                                break;
                            }

                            if (i == entityInQueryIndex) // Itself
                            {
                                continue;
                            }

                            if (humanInBuildingSystemDataAll[i].Building == humanInBuildingData.Building)
                            {
                                humanData.TransmissionsRemaining =- 1;

                                SetComponent(humans[i], new HumanInBuildingSystemData
                                {
                                    Building = humanInBuildingSystemDataAll[i].Building,
                                    Contacts = humanInBuildingSystemDataAll[i].Contacts + 1
                                });
                            }
                        }
                    }
                })
                .WithDisposeOnCompletion(humans)
                .WithDisposeOnCompletion(humanInBuildingSystemDataAll)
                .Schedule();

            var infectionRand = InfectionRand;

            Entities
                .WithName("HumanExitBuildingJob")
                .WithAll<NavAgent>()
                .WithNone<HumanInBuildingData>()
                .ForEach((Entity human, int entityInQueryIndex, ref HumanData humanData, in HumanInBuildingSystemData humanInBuildingData) =>
                {
                    commandBuffer.RemoveComponent<HumanInBuildingSystemData>(entityInQueryIndex, human);

                    if (humanData.Status == Status.Susceptible)
                    {
                        if (infectionRand.NextFloat(1f) < humanData.InfectionProbability * humanInBuildingData.Contacts)
                        {
                            humanData.Status = Status.Infected;
                        }
                    }
                })
                .ScheduleParallel();

            ECB.AddJobHandleForProducer(Dependency);
        }
    }
}