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
        [SerializeField] private Slider MalePercent;

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
        [SerializeField] private ProfileManager ProfileManager;

        // Generic
        [SerializeField] private TMP_InputField LocationX;
        [SerializeField] private TMP_InputField LocationY;

        [SerializeField] private GameObject SimUI;

        private void Awake()
        {
            // London, UK
            //LocationX.text = 51.5077456.ToString(CultureInfo.InvariantCulture);
            //LocationY.text = (-0.1279042).ToString(CultureInfo.InvariantCulture);

            // Vancouver, CA
            LocationX.text = 49.282222.ToString(CultureInfo.InvariantCulture);
            LocationY.text = (-123.124390).ToString(CultureInfo.InvariantCulture);
        }

        public void OnSimulate()
        {
            LocationX.text = LocationX.text.Replace(',', '.');
            LocationY.text = LocationY.text.Replace(',', '.');

            if (!double.TryParse(LocationX.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
            {
                Debug.Log("Failed to parse Location X", this);
                LocationX.image.color = new Color32(255, 175, 175, 255);
                return;
            }

            LocationX.image.color = Color.white;

            if (!double.TryParse(LocationY.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
            {
                Debug.Log("Failed to parse Location Y", this);
                LocationY.image.color = new Color32(255, 175, 175, 255);
                return;
            }

            LocationY.image.color = Color.white;

            SimUI.SetActive(true);
            gameObject.SetActive(false);

            MapManager.StartNewMap(new Mapbox.Utils.Vector2d(x, y), 16);
        }

        public void UpdateProfile(SimProfile profile)
        {
            // Population
            Population.text = profile.PopulationProfile.Population.ToString();
            MalePercent.value = profile.PopulationProfile.MalePercent;

            for (int i = 0; i < AgeDistribution.Length; i++)
            {
                int listeners = AgeDistribution[i].RatioSlider.onValueChanged.GetPersistentEventCount();

                for (int j = 0; j < listeners; j++)
                    AgeDistribution[i].RatioSlider.onValueChanged.SetPersistentListenerState(j, UnityEventCallState.Off);

                AgeDistribution[i].RatioSlider.value = profile.PopulationProfile.AgeDistribution.Values[i];
                AgeDistribution[i].Modifier.text = profile.PopulationProfile.AgeInfectabilityModifiers[i].ToString(CultureInfo.InvariantCulture);

                for (int j = 0; j < listeners; j++)
                    AgeDistribution[i].RatioSlider.onValueChanged.SetPersistentListenerState(j, UnityEventCallState.RuntimeOnly);
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
            ProfileManager.Profile.PopulationProfile.Population = ClampField(Population, 3000, 1, int.MaxValue);
        }

        public void OnAgeDistributionChanged()
        {
            float[] percentages = CalculatePercentagesFromRatios(AgeDistribution.Select(x => x.RatioSlider.value).ToArray());

            for (int i = 0; i < AgeDistribution.Length; i++)
            {
                AgeDistribution[i].Text.text = percentages[i].ToString("P2", CultureInfo.InvariantCulture);
                ProfileManager.Profile.PopulationProfile.AgeDistribution.Values[i] = percentages[i];
            }
        }

        public void OnAgeInfectionModifierChanged()
        {
            for (int i = 0; i < AgeDistribution.Length; i++)
            {
                ProfileManager.Profile.PopulationProfile.AgeInfectabilityModifiers[i] = ClampField(AgeDistribution[i].Modifier, 1f, 0.001f, 100f);
            }
        }

        // Virus
        public void OnInitiallyInfectedChanged()
        {
            ProfileManager.Profile.VirusProfile.InitiallyInfected = ClampField(InitiallyInfected, 10, 1, int.Parse(Population.text));
        }

        public void OnInfectionProbabilityChanged()
        {
            ProfileManager.Profile.VirusProfile.InfectionProbability = ClampField(InfectionProbability, 0.1f, 0.01f, 1f);
        }

        public void OnRecoveryProbabilityChanged()
        {
            ProfileManager.Profile.VirusProfile.RecoveryProbability = ClampField(RecoveryProbability, 0.1f, 0.01f, 1f);
        }

        public void OnReproductionMinChanged()
        {
            int max = int.Parse(ReproductionMax.text);
            ProfileManager.Profile.VirusProfile.ReproductionMin = ClampField(ReproductionMin, max, 0, max);
        }

        public void OnReproductionMaxChanged()
        {
            int min = int.Parse(ReproductionMin.text);
            ProfileManager.Profile.VirusProfile.ReproductionMax = ClampField(ReproductionMax, min, min, int.MaxValue);
        }

        // Helpers
        /// <summary>
        /// Clamps a UI field.
        /// </summary>
        /// <returns>The value that was assigned to the UI Input Field</returns>
        private T ClampField<T>(TMP_InputField field, T defaultValue, T min, T max) where T : struct, IComparable, IConvertible, IEquatable<T>
        {
            T value;

            try
            {
                value = (T)Convert.ChangeType(field.text, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is OverflowException)
            {
                field.text = defaultValue.ToString(CultureInfo.InvariantCulture);
                return defaultValue;
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
            return value;
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