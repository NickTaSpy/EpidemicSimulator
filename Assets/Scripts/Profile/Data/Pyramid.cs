using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Epsim.Profile.Data
{
    [Serializable]
    public class Pyramid
    {
        public List<Range> Ranges;
        public List<float> Values;

        public Pyramid(List<Range> ranges, List<float> values)
        {
            Ranges = ranges;
            Values = values;
        }

        public float[] CalculatePercentagesFromRatios()
        {
            var sum = Values.Sum();
            var results = new float[Values.Count];

            if (sum == 0)
            {
                for (int i = 0; i < Values.Count; i++)
                {
                    results[i] = 1f / 8f;
                }
            }
            else
            {
                for (int i = 0; i < Values.Count; i++)
                {
                    results[i] = Values[i] / sum;
                }
            }

            return results;
        }
    }

    [Serializable]
    public struct Range
    {
        public int From;
        public int To;

        public Range(int from, int to)
        {
            From = from;
            To = to;
        }
    }
}
