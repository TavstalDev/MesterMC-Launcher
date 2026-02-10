/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
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
    
    public static Process? StartJava(string javaPath, string jvmArguments, string gameArguments, string? logFilePath = null, bool enableGameMode = false, bool enableMangoHUd = false, Dictionary<string, string>? environmentVariables = null)
    {
        string finalJavaPath = string.IsNullOrEmpty(javaPath) ? "java" : javaPath;
    
        // Construct the full command string
        string finalProcessPath;
        bool isLinux = OSHelper.GetOperatingSystem() == EOperatingSystem.Linux;
        if (enableGameMode && isLinux)
        {
            finalProcessPath = "gamemoderun";
            if (enableMangoHUd)
                jvmArguments = $"mangohud {finalJavaPath} {jvmArguments}";
            else
                jvmArguments = finalJavaPath + " " + jvmArguments;
        }
        else if (enableMangoHUd && isLinux)
        {
            finalProcessPath = "mangohud";
            jvmArguments = finalJavaPath + " " + jvmArguments;
        }
        else 
            finalProcessPath = finalJavaPath;
        
        // Configure the process start information
        var psi = new ProcessStartInfo
        {
            FileName = finalProcessPath,
            Arguments = jvmArguments,

            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,

            UseShellExecute = false
        };

        // Add environment variables if provided
        if (environmentVariables != null)
        {
            foreach (var kvp in environmentVariables)
                psi.EnvironmentVariables[kvp.Key] = kvp.Value;
        }
        
        if (!string.IsNullOrEmpty(logFilePath) && File.Exists(logFilePath))
            File.Delete(logFilePath);
        
        // Log the process start details
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
            var writer = process.StandardInput;
            string[] gameArgs = gameArguments.Split(' ');
            writer.WriteLine(gameArgs[0]); // Write the main class or jar file first
            writer.WriteLine(Encrypt(gameArgs.Skip(1).Aggregate((a, b) => a + " " + b))); // Write the rest of the arguments as a single line
            writer.Flush();
            writer.Close();
        }

        // Start the process and return the Process object
        return process;
    }
    
    private static string Encrypt(string plainText)
    {
        // same key derivation as Java
        using var sha = SHA256.Create();
        byte[] key = sha.ComputeHash("%#dGG1UkME&vj42kTgVi9*N%J!yE"u8.ToArray());

        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = 256;
        aes.Key = key;

        // generate random IV (16 bytes)
        aes.GenerateIV();
        byte[] iv = aes.IV;

        using var encryptor = aes.CreateEncryptor();

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // combine IV + ciphertext
        byte[] result = new byte[iv.Length + cipherBytes.Length];

        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, iv.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }
}