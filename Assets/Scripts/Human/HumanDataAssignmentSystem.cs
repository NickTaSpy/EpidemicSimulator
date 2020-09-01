using Epsim.Profile.Data;
using Reese.Nav;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace Epsim.Human
{
    public class HumanDataAssignmentSystem : SystemBase
    {
        public SimProfile SimProfile;

        private Random GenderRand = new Random();
        private Random AgeRangeRand = new Random();


        private EntityCommandBufferSystem ECB;

        protected override void OnCreate()
        {
            GenderRand.InitState();
            AgeRangeRand.InitState();
            ECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commandBuffer = ECB.CreateCommandBuffer();

            var simProfile = SimProfile;
            var genderRand = GenderRand;
            var ageRangeRand = AgeRangeRand;
            var agePercentages = simProfile.PopulationProfile.AgeDistribution.CalculatePercentagesFromRatios();

            Entities
                .WithName("HumanDataAssignmentJob")
                .WithAll<NavAgent>()
                .WithNone<HumanData, HumanScheduleData>()
                .ForEach((Entity human, int entityInQueryIndex, int nativeThreadIndex) =>
                {
                    var ageProb = ageRangeRand.NextFloat(1f);

                    int age = -1;
                    int i = 0;
                    float sum = agePercentages[i];
                    while (age == -1 && i < agePercentages.Length)
                    {
                        if (ageProb < sum)
                        {
                            age = (simProfile.PopulationProfile.AgeDistribution.Ranges[i].To + simProfile.PopulationProfile.AgeDistribution.Ranges[i].From) / 2;
                            break;
                        }

                        sum += agePercentages[i];
                        i += 1;
                    }

                    commandBuffer.AddComponent(human, new HumanData
                    {
                        Male = genderRand.NextFloat(1f) < simProfile.PopulationProfile.MalePercent,
                        Age = age,
                        InfectionProbability = 0.1f,
                        RecoveryProbability = 0.1f,
                        Status = Status.Susceptible,
                        TransmissionsRemaining = 4
                    });

                    commandBuffer.AddComponent(human, new HumanScheduleData
                    {
                        WorkStart = HoursToMs(simProfile.ScheduleProfile.WorkStart),
                        WorkEnd = HoursToMs(simProfile.ScheduleProfile.WorkStart + simProfile.ScheduleProfile.WorkDurationHours)
                    });
                })
                .WithoutBurst()
                .Run();

            ECB.AddJobHandleForProducer(Dependency);
        }

        private float HoursToMs(int hours) => hours * 3600000;
    }
}