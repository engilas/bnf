using System;
using System.Collections.Generic;
using System.Linq;

namespace Lang
{
    public class OctConverter
    {
        public static string Convert(double value)
        {
            bool negative = false;
            if (value < 0)
            {
                value *= -1;
                negative = true;
            }

            var intPart = (int) value;
            var floatPart = value - intPart;

            var digits = new List<char>();


            var p = floatPart;

            for (int i = 0; i < 8; i++)
            {
                var mult = p * 8;
                var p1 = (int) mult;
                var p2 = mult - p1;
                digits.Add(p1.ToString()[0]);
                if (Math.Abs(p2) < 0.001) break;
                p = p2;
            }

            return $"{(negative ? "-" : "")}{System.Convert.ToString(intPart, 8)}.{new string(digits.ToArray())}";
        }
    }
}
