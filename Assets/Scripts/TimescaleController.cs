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
    public class TimescaleController : MonoBehaviour
    {
        [SerializeField] TMP_Text TimeScale;

        private EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        private ScheduleSystem ScheduleSystem;

        private void Awake()
        {
            ScheduleSystem = EntityManager.World.GetOrCreateSystem<ScheduleSystem>();
            UpdateText();
        }

        private void Update()
        {
            if (Keyboard.current.equalsKey.wasPressedThisFrame || Keyboard.current.numpadPlusKey.wasPressedThisFrame)
            {
                ScheduleSystem.SetTimeScale(Mathf.Clamp(ScheduleSystem.TimeScale * 2, 1f, 14400f));
                UpdateText();
            }

            if (Keyboard.current.minusKey.wasPressedThisFrame || Keyboard.current.numpadMinusKey.wasPressedThisFrame)
            {
                ScheduleSystem.SetTimeScale(Mathf.Clamp(ScheduleSystem.TimeScale / 2, 1f, 14400f));
                UpdateText();
            }
        }

        private void UpdateText()
        {
            TimeScale.text = ScheduleSystem.TimeScale.ToString(CultureInfo.InvariantCulture);
        }
    }
}
