/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

namespace Tavstal.KonkordLauncher.Core.Models.Installer;

/// <summary>
/// Represents the details of a Minecraft client, including authentication and user information.
/// </summary>
public class ClientDetails
{
    /// <summary>
    /// Gets or sets the access token used for authentication.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the display name of the client.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the universally unique identifier (UUID) of the client.
    /// </summary>
    public string UUID { get; set; }

    /// <summary>
    /// Gets or sets the client ID associated with the client.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the Xbox user ID (XUID) associated with the client.
    /// </summary>
    public string Xuid { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the client is in offline mode.
    /// </summary>
    public bool IsOffline { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientDetails"/> class with the specified parameters.
    /// </summary>
    /// <param name="accessToken">The access token used for authentication.</param>
    /// <param name="displayName">The display name of the client.</param>
    /// <param name="uuid">The universally unique identifier (UUID) of the client.</param>
    /// <param name="isOffline">A value indicating whether the client is in offline mode.</param>
    /// <param name="clientId">The client ID associated with the client. Defaults to "0".</param>
    /// <param name="xuid">The Xbox user ID (XUID) associated with the client. Defaults to "0".</param>
    public ClientDetails(string? accessToken, string displayName, string uuid, bool isOffline, string clientId = "0", string xuid = "0")
    {
        AccessToken = accessToken;
        DisplayName = displayName;
        UUID = uuid;
        IsOffline = isOffline;
        ClientId = clientId;
        Xuid = xuid;
    }
}