using Epsim.Profile.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Epsim.Profile
{
    [Serializable]
    public class RangeElementUI
    {
        public Range Range;
        public Slider RatioSlider;
        public TMP_Text Text;
        public TMP_InputField Modifier;
    }
}