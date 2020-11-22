using Reese.Nav;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Epsim.Human
{
    public struct DestinationRequest
    {
        public Entity Entity;
        public float3 Destination;

        public DestinationRequest(Entity entity, float3 destination)
        {
            Entity = entity;
            Destination = destination;
        }
    }

    [UpdateAfter(typeof(ScheduleSystem))]
    public class QueueNavSystem : SystemBase
    {
        private const int EntitiesPerFrame = 120;

        private EntityCommandBufferSystem ECB;

        private NativeQueue<DestinationRequest> Queue = new NativeQueue<DestinationRequest>(Allocator.Persistent);

        protected override void OnCreate()
        {
            ECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commandBuffer = ECB.CreateCommandBuffer().AsParallelWriter();
            
            int jobCount = math.min(Queue.Count, EntitiesPerFrame);
            var handles = new NativeArray<JobHandle>(jobCount, Allocator.TempJob);

            var queue = Queue;

            for (int i = 0; i < jobCount; i++)
            {
                var request = queue.Dequeue();

                handles[i] = Job
                    .WithName("QueueNavJob")
                    .WithNativeDisableContainerSafetyRestriction(commandBuffer)
                    .WithCode(() =>
                    {
                        commandBuffer.AddComponent(i, request.Entity, new NavNeedsDestination
                        {
                            Destination = request.Destination
                        });
                    })
                    .WithBurst()
                    .Schedule(Dependency);
            }

            ECB.AddJobHandleForProducer(JobHandle.CombineDependencies(handles));
            JobHandle.CompleteAll(handles);

            handles.Dispose();
        }

        protected override void OnDestroy()
        {
            Queue.Dispose();
        }

        public NativeQueue<DestinationRequest>.ParallelWriter GetParallelQueue() => Queue.AsParallelWriter();
    }
}