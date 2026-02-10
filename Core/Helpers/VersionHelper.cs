/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

namespace Tavstal.KonkordLauncher.Core.Helpers;

/// <summary>
/// Provides helper methods for version comparison and parsing.
/// </summary>
public static class VersionHelper
{
    /// <summary>
    /// Compares two version strings to determine if the first version is newer than the second.
    /// </summary>
    /// <param name="versionA">The first version string to compare.</param>
    /// <param name="versionB">The second version string to compare.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="versionA"/> is newer than <paramref name="versionB"/>; otherwise, <c>false</c>.
    /// </returns>
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

    /// <summary>
    /// Parses a version string into an array of integers.
    /// </summary>
    /// <param name="version">The version string to parse (e.g., "1.2.3").</param>
    /// <returns>
    /// An array of integers representing the version parts. If a part cannot be parsed, it defaults to 0.
    /// </returns>
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