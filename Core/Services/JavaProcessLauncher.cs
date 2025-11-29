/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System.Diagnostics;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Provides functionality to launch Java processes with specified arguments.
/// </summary>
public static class JavaProcessLauncher
{
    // Logger instance for the JavaProcessLauncher module
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(JavaProcessLauncher));
    
    /// <summary>
    /// Starts a Java process with the specified parameters and logs its output.
    /// </summary>
    /// <param name="javaPath">The path to the Java executable. If null or empty, "java" is used by default.</param>
    /// <param name="arguments">The arguments to pass to the Java process.</param>
    /// <param name="logFilePath">The path to the log file where the process output will be redirected.</param>
    /// <param name="wrapperCommand">
    /// An optional wrapper command to execute the Java process. If it contains "%command%", 
    /// it will be replaced with the Java executable path and arguments.
    /// </param>
    /// <param name="environmentVariables">
    /// An optional dictionary of environment variables to set for the process.
    /// </param>
    /// <returns>
    /// A <see cref="Process"/> object representing the started Java process, or null if the process could not be started.
    /// </returns>
    public static Process? StartJava(string javaPath, string arguments, string? logFilePath = null, string? wrapperCommand = null, Dictionary<string, string>? environmentVariables = null)
    {
        string finalJavaPath = string.IsNullOrEmpty(javaPath) ? "java" : javaPath;
    
        // Construct the full command string
        string fullCommand;
        if (!string.IsNullOrEmpty(wrapperCommand))
        {
            if (wrapperCommand.Contains("%command%"))
                fullCommand = wrapperCommand.Replace("%command%", finalJavaPath) + " " + arguments;
            else
                fullCommand = wrapperCommand + (wrapperCommand.EndsWith(" ") ? "" : " ") + finalJavaPath + " " + arguments;
        }
        else
            fullCommand = finalJavaPath + " " + arguments;
        
        fullCommand = fullCommand.Replace("\"", "\\\"");
        
        // Configure the process start information
        var os = OSHelper.GetOperatingSystem();
        var psi = new ProcessStartInfo
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        // Add environment variables if provided
        if (environmentVariables != null)
        {
            foreach (var kvp in environmentVariables)
                psi.EnvironmentVariables[kvp.Key] = kvp.Value;
        }
        
        if (!string.IsNullOrEmpty(logFilePath) && File.Exists(logFilePath))
            File.Delete(logFilePath);
        
        switch (os)
        {
            case EOperatingSystem.Windows:
            {
                psi.FileName = finalJavaPath;
                psi.Arguments = arguments;
                break;
            }
            case EOperatingSystem.MacOS:
            {
                psi.FileName = "/bin/zsh";
                psi.Arguments = string.IsNullOrEmpty(logFilePath) ? 
                    $@"-c ""{fullCommand}""" 
                    : 
                    $@"-c ""{fullCommand} >> ""{logFilePath}"" 2>&1""";
                break;
            }
            case EOperatingSystem.Unknown:
            case EOperatingSystem.Linux:
            {
                psi.FileName = "/bin/sh";
                psi.Arguments = string.IsNullOrEmpty(logFilePath) ? 
                    $@"-c ""{fullCommand}""" 
                    : 
                    $@"-c ""{fullCommand} >> '{logFilePath}' 2>&1""";
                break;
            }
        }
        
        // Log the process start details
        _logger.Debug($"Java Path: {finalJavaPath}");
        _logger.Debug("Starting Java process with arguments:");
        _logger.Debug("FileName: " + psi.FileName);
        _logger.Debug("Arguments: " + psi.Arguments);

        var process = Process.Start(psi);
        if (process != null)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.EnableRaisingEvents = true;
            process.Exited += (_, _) =>
            {
                _logger.Debug($"Java process exited with code: {process.ExitCode}");
            };
        }

        // Start the process and return the Process object
        return process;
    }
    
    /// <summary>
    /// Starts a process to execute a custom command with optional environment variables.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="environmentVariables">
    /// An optional dictionary of environment variables to set for the process.
    /// </param>
    /// <returns>
    /// A <see cref="Process"/> object representing the started process, or null if the process could not be started.
    /// </returns>
    public static Process? StartCommand(string command, Dictionary<string, string>? environmentVariables = null)
    {
        // Configure the process start information
        var psi = new ProcessStartInfo()
        {
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        // Add environment variables if provided
        if (environmentVariables != null)
        {
            foreach (var kvp in environmentVariables)
                psi.EnvironmentVariables[kvp.Key] = kvp.Value;
        }

        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                psi.FileName = "cmd.exe";
                psi.Arguments = $"/C \"{command}\"";
                break;
            }
            case EOperatingSystem.MacOS:
            {
                psi.FileName = "/bin/zsh";
                psi.Arguments = $"-c \"{command}\"";
                break;
            }
            case EOperatingSystem.Unknown:
            case EOperatingSystem.Linux:
            {
                psi.FileName = "/bin/sh";
                psi.Arguments = $"-c \"{command}\"";
                break;
            }
        }
        
        var process = Process.Start(psi);
        if (process != null)
        {
            process.EnableRaisingEvents = true;
        }

        // Start the process and return the Process object
        return process;
    }
}