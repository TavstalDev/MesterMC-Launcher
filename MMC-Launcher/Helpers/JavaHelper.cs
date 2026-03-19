/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.MesterMC.Launcher.Models.Config.Java;

namespace Tavstal.MesterMC.Launcher.Helpers;

/// <summary>
/// Provides helper methods for working with Java installations and versions.
/// </summary>
public static class JavaHelper
{
    /// <summary>
    /// Logger instance for the JavaHelper module.
    /// </summary>
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(JavaHelper));
    
    private static List<JavaVersion> _cachedJavaVersions = [];
    private static DateTime _cacheExpiration = DateTime.MinValue;

    private static readonly Dictionary<string, JavaMirrorArchitecture> JavaSdks = new()
    {
        { 
            "windows", new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_x64_windows_hotspot_21.0.8_9.zip",
                // arm
                "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_aarch64_windows_hotspot_21.0.8_9.zip"
            )
        },
        {
            "linux", new JavaMirrorArchitecture(
                // x86_64
                "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_x64_linux_hotspot_21.0.8_9.tar.gz",
                // arm
                "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_aarch64_linux_hotspot_21.0.8_9.tar.gz"
            )
        },
        {
           "macos",  new JavaMirrorArchitecture(
               // x86_64
               "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_x64_mac_hotspot_21.0.8_9.tar.gz",
               // arm
               "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_aarch64_mac_hotspot_21.0.8_9.tar.gz"
           )
        }
    };

    /// <summary>
    /// Downloads a specific Java version and extracts it to the target directory.
    /// </summary>
    /// <param name="targetPath">The directory where the downloaded Java version will be extracted.</param>
    /// <param name="progress">
    /// An optional progress reporter to track the download progress as a percentage.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is a boolean indicating
    /// whether the download and extraction were successful.
    /// </returns>
    public static async Task<bool> DownloadJavaVersionAsync(string targetPath,
        Progress<double>? progress = null)
    {
        try
        {
            EOperatingSystem operatingSystem = OSHelper.GetOperatingSystem();
            bool isArmBased = OSHelper.IsArmBased();
            var osMirror = operatingSystem switch
            {
                EOperatingSystem.Windows => JavaSdks["windows"],
                EOperatingSystem.Linux => JavaSdks["linux"],
                EOperatingSystem.MacOS => JavaSdks["macos"],
                _ => null
            };
            if (osMirror == null)
                return false;

            string url = isArmBased ? osMirror.Arm : osMirror.X86_64;
            if (string.IsNullOrEmpty(url))
            {
                _logger.Warn(
                    $"No download URL found for Java 21 on {operatingSystem} OS {(isArmBased ? "arm" : "x64")}.");
                return false;
            }

            string tempDir = Path.Combine(Path.GetTempPath(), "mmclauncher_java");
            try
            {
                Directory.CreateDirectory(tempDir);
                string extension = url.EndsWith(".zip") ? "zip" : "tar.gz";
                string zipFilePath = Path.Combine(tempDir, $"java_21.{extension}");
                await HttpHelper.DownloadFileAsync(url, zipFilePath, progress);

                if (!File.Exists(zipFilePath))
                {
                    _logger.Error($"Java download failed: {url}");
                    return false;
                }
                
                if (extension.Equals("tar.gz"))
                {
                    await using Stream inStream = File.OpenRead(zipFilePath);
                    await using Stream gzipStream = new GZipInputStream(inStream);
                    using TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
                    tarArchive.ExtractContents(targetPath);
                }
                else
                    ZipFile.ExtractToDirectory(zipFilePath, targetPath);
            }
            finally
            {
                FileSystemHelper.DeleteDirectory(tempDir);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Exc($"Failed to download Java 21.");
            _logger.Exc(ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// Retrieves detailed information about a Java installation by executing the specified Java executable.
    /// </summary>
    /// <param name="path">The file path to the Java executable.</param>
    /// <returns>
    /// A <see cref="JavaVersion"/> object containing the Java version details, or null if the details could not be retrieved.
    /// </returns>
    public static JavaVersion? GetJavaVersionDetails(string path)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = path,
                Arguments = "-XshowSettings:properties -version",
                RedirectStandardError = true, // Output goes to stderr
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process? process = Process.Start(psi);
            if (process == null)
                return null;

            string output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            string majorVersion = string.Empty;
            string javaVersion = string.Empty;
            string architecture = string.Empty;

            foreach (var line in output.Split('\n'))
            {
                if (line.Contains("java.specification.version ="))
                    majorVersion = line.Split('=')[1].Trim();

                if (line.Contains("java.version ="))
                    javaVersion = line.Split('=')[1].Trim();

                if (line.Contains("os.arch ="))
                    architecture = line.Split('=')[1].Trim();
            }

            if (majorVersion.StartsWith("1."))
            {
                string[] parts = majorVersion.Split('.');
                if (parts.Length > 1)
                {
                    majorVersion = parts[1];
                }
                else
                {
                    _logger.Warn($"Java version format '{majorVersion}' is unexpected, defaulting to 1.");
                    majorVersion = "1"; // Default to 1 if no version is found
                }
            }

            return new JavaVersion(int.Parse(majorVersion), javaVersion, architecture, path);
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to get Java version details:");
            _logger.Error(ex.ToString());
            return null;
        }
    }
    
    /// <summary>
    /// Locates Java installations under the provided root directory and returns their parsed versions.
    /// </summary>
    /// <param name="javaRootDir">The parent directory that contains Java installation subdirectories.</param>
    /// <param name="forceRefresh">
    /// If true, clears the internal cache and forces a fresh scan.
    /// Note: cache invalidation is not thread-safe; callers should avoid concurrent calls that rely on atomic refresh.
    /// </param>
    /// <returns>
    /// A <see cref="List{JavaVersion}"/> containing discovered Java versions. The list will be empty if none are found
    /// or if <paramref name="javaRootDir"/> does not exist or contains no valid Java executables.
    /// </returns>
    public static List<JavaVersion> LocateJavaInstallations(string javaRootDir, bool forceRefresh = false)
    {
        if (forceRefresh)
        {
            _cachedJavaVersions = [];
            _cacheExpiration = DateTime.MinValue;
        }

        if (_cachedJavaVersions.Count > 0 && _cacheExpiration > DateTimeOffset.UtcNow)
            return _cachedJavaVersions;
        
        List<JavaVersion> javaVersions = [];
        List<string> javaPaths = GetJavaPaths(javaRootDir);

        foreach (var path in javaPaths)
        {
            var versionDetails = GetJavaVersionDetails(path);
            if (versionDetails == null)
                continue;

            javaVersions.Add(versionDetails);
        }

        _cachedJavaVersions = javaVersions;
        _cacheExpiration = DateTime.UtcNow.AddMinutes(10); // Cache for 10 minutes

        return javaVersions;
    }
    
    /// <summary>
    /// Enumerates subdirectories of <paramref name="javaParentDir"/> and returns paths to Java executables found.
    /// </summary>
    /// <param name="javaParentDir">The directory expected to contain Java installation folders (each with a 'bin' folder).</param>
    /// <returns>
    /// A <see cref="List{String}"/> of full paths to the Java executable (e.g. "path/to/bin/java" or "path\\to\\bin\\javaw.exe").
    /// Returns an empty list if <paramref name="javaParentDir"/> does not exist or no executables are found.
    /// </returns>
    private static List<string> GetJavaPaths(string javaParentDir)
    {
        List<string> paths = [];
        bool isWindows = OSHelper.GetOperatingSystem() == EOperatingSystem.Windows;
        if (!Directory.Exists(javaParentDir))
            return paths;

        var subDirs = Directory.GetDirectories(javaParentDir);
        foreach (var subDir in subDirs)
        {
            string javaPath = Path.Combine(subDir, "bin", isWindows ? "javaw.exe" : "java");
            if (!File.Exists(javaPath))
                continue;
            paths.Add(javaPath);
        }
        return paths;
    }
}