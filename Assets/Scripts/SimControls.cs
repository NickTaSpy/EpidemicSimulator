using Epsim.Human;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Epsim
{
    public class SimControls : MonoBehaviour
    {
        [SerializeField] TMP_Text TimeScaleText;

        [SerializeField] Material BuildingMaterial;

        private int BuildingOpacityID;

        private EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        private ScheduleSystem ScheduleSystem;

        private void Awake()
        {
            ScheduleSystem = EntityManager.World.GetOrCreateSystem<ScheduleSystem>();
            UpdateTimeScaleText();

            BuildingOpacityID = Shader.PropertyToID("_BuildingOpacity");
        }

        private void Update()
        {
            // TimeScale
            if (Keyboard.current.equalsKey.wasPressedThisFrame || Keyboard.current.numpadPlusKey.wasPressedThisFrame)
            {
                ScheduleSystem.SetTimeScale(Mathf.Clamp(ScheduleSystem.TimeScale * 2, 1f, 14400f));
                UpdateTimeScaleText();
            }

            if (Keyboard.current.minusKey.wasPressedThisFrame || Keyboard.current.numpadMinusKey.wasPressedThisFrame)
            {
                ScheduleSystem.SetTimeScale(Mathf.Clamp(ScheduleSystem.TimeScale / 2, 1f, 14400f));
                UpdateTimeScaleText();
            }

            // Building opacity
            if (Keyboard.current.zKey.wasPressedThisFrame)
            {
                BuildingMaterial.SetFloat(BuildingOpacityID, Mathf.Clamp(BuildingMaterial.GetFloat(BuildingOpacityID) - 0.5f, 0f, 1f));
            }

            if (Keyboard.current.xKey.wasPressedThisFrame)
            {
                BuildingMaterial.SetFloat(BuildingOpacityID, Mathf.Clamp(BuildingMaterial.GetFloat(BuildingOpacityID) + 0.5f, 0f, 1f));
            }
        }

        private void UpdateTimeScaleText()
        {
            TimeScaleText.text = ScheduleSystem.TimeScale.ToString("F0", CultureInfo.InvariantCulture) + "x";
        }
    }
}
