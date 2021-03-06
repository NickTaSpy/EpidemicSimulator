﻿using Epsim.Profile.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace Epsim.Profile
{
    public class ProfileManager : MonoBehaviour
    {
        [SerializeField] private DynamicScrollListUI ProfilesListUI;
        [SerializeField] private ProfileUI ProfileUI;

        [SerializeField] private SimProfile DefaultProfile;
        [SerializeField] private SimProfile SelectedProfile;

        [SerializeField] private TMP_InputField ProfileName;

        [SerializeField] private TMP_Text HumanCountUI;

        private SimProfile DefaultProfileCopy;
        private string ProfileFolder;

        public SimProfile Profile => SelectedProfile;

        private void Awake()
        {
            ProfileFolder = Application.dataPath + "/SimProfiles";
            Directory.CreateDirectory(ProfileFolder); // Make sure the profiles folder exists.

            LoadAllProfiles();
        }

        private void LoadAllProfiles()
        {
            var files = Directory.GetFiles(ProfileFolder, "*.json", SearchOption.AllDirectories);

            // Load default.
            CopyDefaultProfile();
            ProfilesListUI.Add("Default", () =>
            {
                CopyDefaultProfile();
                SelectProfile(DefaultProfileCopy);
            });
            SelectProfile(DefaultProfileCopy);

            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var profile = SimProfile.Load(file);
                ProfilesListUI.Add(name, () => SelectProfile(profile));
            }
        }

        public void ReloadAllProfiles()
        {
            ProfilesListUI.RemoveAll();
            LoadAllProfiles();
        }

        public void SelectProfile(SimProfile profile)
        {
            SelectedProfile = profile;
            HumanCountUI.text = profile.PopulationProfile.Population.ToString();
            ProfileName.text = profile.Name;
            ProfileUI.UpdateProfile(profile);
        }

        public void SaveProfile()
        {
            if (ProfileName.text == "Default")
            {
                return;
            }

            SelectedProfile.Name = ProfileName.text;
            SelectedProfile.Save($"{ProfileFolder}/{ProfileName.text}.json");
            ReloadAllProfiles();
        }

        private void CopyDefaultProfile()
        {
            if (DefaultProfileCopy != null)
                Destroy(DefaultProfileCopy);

            DefaultProfileCopy = Instantiate(DefaultProfile);
        }
    }
}