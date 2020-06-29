using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Epsim.Profile.Data
{
    [Serializable]
    public class PopulationProfile
    {
        /// <summary>
        /// Total population.
        /// </summary>
        public int Population;

        /// <summary>
        /// Age distribution in ranges. E.g. [5-10]: 5%
        /// </summary>
        public Pyramid AgeDistribution;

        /// <summary>
        /// Percentage of people who are males.
        /// </summary>
        public float MalePercent;

        public PopulationProfile()
        {

        }

        public PopulationProfile(int population, Pyramid ageDistribution, float malePercent)
        {
            Population = population;
            AgeDistribution = ageDistribution;
            MalePercent = malePercent;
        }
    }
}
