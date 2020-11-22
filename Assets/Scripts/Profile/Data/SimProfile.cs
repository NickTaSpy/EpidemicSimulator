using Epsim.Profile.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Epsim.Profile.Data
{
    [Serializable]
    [CreateAssetMenu(menuName = "Epsim/SimProfile")]
    public class SimProfile : ScriptableObject
    {
        public string Name;

        public PopulationProfile PopulationProfile;

        public ScheduleProfile ScheduleProfile;

        public VirusProfile VirusProfile;

        public SimProfile()
        {
            PopulationProfile = new PopulationProfile();
            ScheduleProfile = new ScheduleProfile();
            VirusProfile = new VirusProfile();
        }

        public SimProfile(string name, PopulationProfile populationProfile, ScheduleProfile scheduleProfile, VirusProfile virusProfile)
        {
            Name = name;
            PopulationProfile = populationProfile;
            ScheduleProfile = scheduleProfile;
            VirusProfile = virusProfile;
        }

        public SimProfile(string filename)
        {
            JsonUtility.FromJsonOverwrite(File.ReadAllText(filename, Encoding.UTF8), this);
        }

        public void Save(string filename)
        {
            File.WriteAllText(filename, JsonUtility.ToJson(this, true), Encoding.UTF8);
        }

        public static SimProfile Load(string filename)
        {
            return new SimProfile(filename);
        }
    }
}
