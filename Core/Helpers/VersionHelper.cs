/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

namespace Tavstal.KonkordLauncher.Core.Helpers;

public static class VersionHelper
{
    public static bool isNewer(string versionA, string versionB)
    {
        int[] verA = ParseVersionString(versionA);
        int[] verB = ParseVersionString(versionB);
        
        int length = Math.Max(verA.Length, verB.Length);
        for (int i = 0; i < length; i++)
        {
            int partA = i < verA.Length ? verA[i] : 0;
            int partB = i < verB.Length ? verB[i] : 0;

            if (partA > partB)
                return true;
            if (partA < partB)
                return false;
        }
        return false; // Versions are equal
     }
    
    public static int[] ParseVersionString(string version)
    {
        var parts = version.Split('.');
        var versionNumbers = new int[parts.Length];

        for (int i = 0; i < parts.Length; i++)
        {
            if (int.TryParse(parts[i], out int number))
            {
                versionNumbers[i] = number;
            }
            else
            {
                versionNumbers[i] = 0; // Default to 0 if parsing fails
            }
        }

        return versionNumbers;
    }
}