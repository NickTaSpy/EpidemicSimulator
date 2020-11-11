using Epsim.Human;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
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

        [SerializeField] private TMP_Text SusceptibleUI;
        [SerializeField] private TMP_Text InfectedUI;
        [SerializeField] private TMP_Text RecoveredUI;

        private int FrameCount = 0;
        private DateTime FirstDate;

        private EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        private StatisticsCollectionSystem Statistics;
        private ScheduleSystem ScheduleSystem;

        private void Awake()
        {
            Statistics = EntityManager.World.GetOrCreateSystem<StatisticsCollectionSystem>();
            ScheduleSystem = EntityManager.World.GetOrCreateSystem<ScheduleSystem>();
            FirstDate = ScheduleSystem.DateTime;

            LineChart.ClearData();
        }

        private void Update()
        {
            if (++FrameCount >= FramesInterval)
            {
                FrameCount = 0;
                Statistics.Update();
            }
        }

        private void LateUpdate()
        {
            if (FrameCount == 0)
            {
                UpdateGraph();
            }
        }

        private void UpdateGraph()
        {
            SusceptibleUI.text = Statistics.SusceptibleCount.ToString();
            InfectedUI.text = Statistics.InfectedCount.ToString();
            RecoveredUI.text = Statistics.RecoveredCount.ToString();

            float hour = (float)(ScheduleSystem.DateTime - FirstDate).TotalHours;

            LineChart.AddData("Susceptible", new List<float> { hour, Statistics.SusceptibleCount });
            LineChart.AddData("Infected", new List<float> { hour, Statistics.InfectedCount });
            LineChart.AddData("Recovered", new List<float> { hour, Statistics.RecoveredCount });
        }
    }
}
