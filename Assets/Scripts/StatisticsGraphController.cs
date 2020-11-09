using Epsim.Human;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using XCharts;

namespace Epsim
{
    public class StatisticsGraphController : MonoBehaviour
    {
        [SerializeField] private int FramesInterval;
        [SerializeField] private LineChart LineChart;

        private int FrameCount = 0;

        private EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        private StatisticsCollectionSystem Statistics;
        private ScheduleSystem ScheduleSystem;

        private void Awake()
        {
            Statistics = EntityManager.World.GetOrCreateSystem<StatisticsCollectionSystem>();
            ScheduleSystem = EntityManager.World.GetOrCreateSystem<ScheduleSystem>();

            LineChart.ClearData();
        }

        private void Update()
        {
            if (++FrameCount == FramesInterval)
            {
                FrameCount = 0;
                Statistics.Update();
            }
        }

        private void LateUpdate()
        {
            if (FrameCount == 0)
            {
                Statistics.Complete();
                UpdateGraph();
            }
        }

        private void UpdateGraph()
        {
            float hour = (float)ScheduleSystem.DateTime.TimeOfDay.TotalHours;

            LineChart.AddData("Susceptible", new List<float> { hour, Statistics.SusceptibleCount });
            LineChart.AddData("Infected", new List<float> { hour, Statistics.InfectedCount });
            LineChart.AddData("Recovered", new List<float> { hour, Statistics.RecoveredCount });
        }
    }
}
