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

using Headquarters4DCS.Mission;
using System;

namespace Headquarters4DCS.Miz
{
    /// <summary>
    /// Creates the "l10n/DEFAULT/dictionary" entry in the MIZ file.
    /// </summary>
    public sealed class MizExporterLuaDictionary : IDisposable
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MizExporterLuaDictionary() { }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Generates the content of the Lua file.
        /// </summary>
        /// <returns>The contents of the Lua file.</returns>
        public string MakeLua()
        {
            // Dictionaries are always the same in HQ4DCSMission, just import Include\Lua\Dictionary.lua as it is.
            return HQTools.ReadIncludeLuaFile("Dictionary.lua");
        }
    }
}
