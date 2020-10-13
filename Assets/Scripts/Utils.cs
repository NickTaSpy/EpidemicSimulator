using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Epsim
{
    public static class Utils
    {
        public static bool Approx(this float a, float b)
        {
            return math.abs(b - a) < math.max(0.000001f * math.max(math.abs(a), math.abs(b)), float.Epsilon * 8);
        }

        public static bool Approx(this float a, float b, float threshold)
        {
            return math.abs(b - a) < threshold;
        }
    }
}