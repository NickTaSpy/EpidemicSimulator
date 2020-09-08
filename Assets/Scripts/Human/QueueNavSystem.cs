﻿using Reese.Nav;
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
    public class QueueNavSystem : SystemBase
    {
        private const int EntitiesPerFrame = 2;

        private EntityCommandBufferSystem ECB;

        private readonly EntityQueryDesc QueryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<QueuedForDestination>() }
        };

        protected override void OnCreate()
        {
            ECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            // Process entities.
            var query = GetEntityQuery(QueryDesc);
            var entities= query.ToEntityArray(Allocator.TempJob);
            var dests = query.ToComponentDataArray<QueuedForDestination>(Allocator.TempJob);

            var commandBuffer = ECB.CreateCommandBuffer().AsParallelWriter();
            
            int jobCount = math.min(entities.Length, EntitiesPerFrame + 1);
            var handles = new NativeArray<JobHandle>(jobCount, Allocator.TempJob);

            for (int i = 0; i < jobCount; i++)
            {
                handles[i] = Job
                    .WithName("QueueNavJob")
                    .WithReadOnly(entities)
                    .WithReadOnly(dests)
                    .WithNativeDisableContainerSafetyRestriction(commandBuffer)
                    .WithCode(() =>
                    {
                        commandBuffer.RemoveComponent<QueuedForDestination>(i, entities[i]);
                        commandBuffer.AddComponent(i, entities[i], new NavNeedsDestination
                        {
                            Destination = dests[i].Destination
                        });
                    })
                    .WithBurst()
                    .Schedule(Dependency);
            }

            ECB.AddJobHandleForProducer(JobHandle.CombineDependencies(handles));
            JobHandle.CompleteAll(handles);

            handles.Dispose();
            entities.Dispose();
            dests.Dispose();
        }
    }
}