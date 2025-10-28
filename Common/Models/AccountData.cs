/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents account data, including a collection of accounts and the ID of the selected account.
/// </summary>
public class AccountData
{
    /// <summary>
    /// Gets or sets the ID of the selected account.
    /// </summary>
    [JsonPropertyName("selectedAccountId"), JsonProperty("selectedAccountId")]
    public string SelectedAccountId { get; set; }

    /// <summary>
    /// Gets or sets the list of accounts
    /// </summary>
    [JsonPropertyName("accounts"), JsonProperty("accounts")]
    public List<Account> Accounts { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountData"/> class.
    /// </summary>
    public AccountData()
    {
        SelectedAccountId = string.Empty;
        Accounts = new List<Account>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountData"/> class with the specified accounts and selected account ID.
    /// </summary>
    /// <param name="accounts">The list of accounts.</param>
    /// <param name="selectedAccountId">The ID of the selected account.</param>
    public AccountData(List<Account> accounts, string selectedAccountId)
    {
        Accounts = accounts;
        SelectedAccountId = selectedAccountId;
    }
}