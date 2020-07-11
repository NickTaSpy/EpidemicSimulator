using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using Epsim.Profile.Data;
using System.Linq;
using JetBrains.Annotations;
using Unity.Entities.UniversalDelegates;
using System.Globalization;

namespace Epsim.Profile
{
    public class ProfileUI : MonoBehaviour
    {
        // Population
        [SerializeField] private TMP_InputField Population;
        [SerializeField] private RangeElementUI[] AgeDistribution;

        // Schedule
        [SerializeField] private Slider WorkStart;
        [SerializeField] private Slider WorkDuration;

        // Virus
        [SerializeField] private TMP_InputField InitiallyInfected;

        [SerializeField] private TMP_InputField InfectionProbability;
        [SerializeField] private TMP_InputField RecoveryProbability;

        [SerializeField] private TMP_InputField ReproductionMin;
        [SerializeField] private TMP_InputField ReproductionMax;

        [SerializeField] private MapManager MapManager;

        public void OnSimulate()
        {
            MapManager.StartNewMap(new Mapbox.Utils.Vector2d(51.5077456, -0.1279042), 16); // London, UK
        }

        public void UpdateProfile(SimProfile profile)
        {
            // Population
            Population.text = profile.PopulationProfile.Population.ToString();

            for (int i = 0; i < AgeDistribution.Length; i++)
            {
                AgeDistribution[i].RatioSlider.value = profile.PopulationProfile.AgeDistribution.Values[i];
                AgeDistribution[i].Modifier.text = profile.PopulationProfile.AgeInfectabilityModifiers[i].ToString();
            }
            OnAgeDistributionChanged();

            // Schedule
            WorkStart.value = profile.ScheduleProfile.WorkStart;
            WorkDuration.value = profile.ScheduleProfile.WorkDurationHours;

            // Virus
            InitiallyInfected.text = profile.VirusProfile.InitiallyInfected.ToString();
            InfectionProbability.text = profile.VirusProfile.InfectionProbability.ToString(CultureInfo.InvariantCulture);
            RecoveryProbability.text = profile.VirusProfile.RecoveryProbability.ToString(CultureInfo.InvariantCulture);
            ReproductionMin.text = profile.VirusProfile.ReproductionMin.ToString();
            ReproductionMax.text = profile.VirusProfile.ReproductionMax.ToString();
        }

        // Population
        public void OnPopulationChanged()
        {
            ClampField(Population, 3000, 1, int.MaxValue);
        }

        public void OnAgeDistributionChanged()
        {
            float[] percentages = CalculatePercentagesFromRatios(AgeDistribution.Select(x => x.RatioSlider.value).ToArray());

            for (int i = 0; i < AgeDistribution.Length; i++)
            {
                AgeDistribution[i].Text.text = percentages[i].ToString("P2", CultureInfo.InvariantCulture);
            }
        }

        public void OnAgeInfectionModifierChanged()
        {
            for (int i = 0; i < AgeDistribution.Length; i++)
            {
                ClampField(AgeDistribution[i].Modifier, 1f, 0.001f, 100f);
            }
        }

        // Virus
        public void OnInitiallyInfectedChanged()
        {
            ClampField(InitiallyInfected, 10, 1, int.Parse(Population.text));
        }

        public void OnInfectionProbabilityChanged()
        {
            ClampField(InfectionProbability, 0.1f, 0.01f, 1f);
        }

        public void OnRecoveryProbabilityChanged()
        {
            ClampField(RecoveryProbability, 0.1f, 0.01f, 1f);
        }

        public void OnReproductionMinChanged()
        {
            int max = int.Parse(ReproductionMax.text);
            ClampField(ReproductionMin, max, 0, max);
        }

        public void OnReproductionMaxChanged()
        {
            int min = int.Parse(ReproductionMin.text);
            ClampField(ReproductionMax, min, min, int.MaxValue);
        }

        // Helpers
        private bool ClampField<T>(TMP_InputField field, T defaultValue, T min, T max) where T : struct, IComparable, IConvertible, IEquatable<T>
        {
            T value;

            try
            {
                value = (T)Convert.ChangeType(field.text, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is OverflowException)
            {
                field.text = defaultValue.ToString(CultureInfo.InvariantCulture);
                return false;
            }

            if (value.CompareTo(min) < 0)
            {
                value = min;
            }
            else if (value.CompareTo(max) > 0)
            {
                value = max;
            }

            field.text = value.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        private float[] CalculatePercentagesFromRatios(float[] ratios)
        {
            var sum = ratios.Sum();
            var results = new float[ratios.Length];

            if (sum == 0)
            {
                for (int i = 0; i < ratios.Length; i++)
                {
                    results[i] = 1f / 8f;
                }
            }
            else
            {
                for (int i = 0; i < ratios.Length; i++)
                {
                    results[i] = ratios[i] / sum;
                }
            }

            return results;
        }
    }
}