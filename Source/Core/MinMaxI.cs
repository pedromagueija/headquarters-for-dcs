﻿/*
==========================================================================
This file is part of Headquarters for DCS World (HQ4DCS), a mission generator for
Eagle Dynamics' DCS World flight simulator.

HQ4DCS was created by Ambroise Garel (@akaAgar).
You can find more information about the project on its GitHub page,
https://akaAgar.github.io/headquarters-for-dcs

HQ4DCS is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

HQ4DCS is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with HQ4DCS. If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using System;

namespace Headquarters4DCS
{
    /// <summary>
    /// Stores a minimum and a maximum double value.
    /// </summary>
    public struct MinMaxI
    {
        /// <summary>
        /// The minimum value.
        /// </summary>
        public readonly int Min;

        /// <summary>
        /// The maximum value.
        /// </summary>
        public readonly int Max;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public MinMaxI(int min, int max) { Min = Math.Min(min, max); Max = Math.Max(min, max); }

        /// <summary>
        /// Constructor. Parses the min and max coordinates from a string (format is "1.2345,6.7890")
        /// </summary>
        /// <param name="minMaxString">The string containing the min and max value.</param>
        public MinMaxI(string minMaxString)
        {
            try
            {
                string[] minAndMaxString = minMaxString.Split(',');

                int val1 = HQTools.StringToInt(minAndMaxString[0]);
                int val2 = HQTools.StringToInt(minAndMaxString[1]);

                Min = Math.Min(val1, val2);
                Max = Math.Max(val1, val2);
            }
            catch (Exception)
            {
                Min = 0; Max = 0;
            }
        }

        /// <summary>
        /// Returns a random value between Min and Max.
        /// </summary>
        /// <returns>A random value.</returns>
        public int GetValue() { return HQTools.RandomInt(Min, Max + 1); }

        public static MinMaxI operator *(MinMaxI mm, int mult) { return new MinMaxI(mm.Min * mult, mm.Max * mult); }
        public static MinMaxI operator /(MinMaxI mm, int mult) { return new MinMaxI(mm.Min / mult, mm.Max / mult); }

        /// <summary>
        /// Returns the MinMax value as a string.
        /// </summary>
        /// <returns>A string</returns>
        public override string ToString() { return ToString(null); }

        /// <summary>
        /// Returns the MinMax value as a string.
        /// </summary>
        /// <param name="stringFormat">A format string.</param>
        /// <returns>A string</returns>
        public string ToString(string stringFormat)
        { return HQTools.ValToString(Min, stringFormat) + "," + HQTools.ValToString(Max, stringFormat); }

        /// <summary>
        /// Is the value passed as parameter between the min and the max values?
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if the value passed as parameter is between the min and the max values, false otherwise.</returns>
        public bool Contains(int value) { return (value >= Min) && (value <= Max); }
    }
}
