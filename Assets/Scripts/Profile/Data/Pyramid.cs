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
