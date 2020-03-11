// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Extensions.Configuration.DaprSecretStore
{
    /// <summary>
    /// The <see cref="IDaprSecretStoreManager"/> instance used to control secret loading.
    /// </summary>
    public interface IDaprSecretStoreManager
    {
        /// <summary>
        /// Maps secret to a configuration key.
        /// </summary>
        /// <param name="secret">The secret name.</param>
        /// <returns>Configuration key name to store secret value.</returns>
        string GetKey(string secret);

        /// <summary>
        /// Gets or sets the secret's metadata.
        /// </summary>
        /// <param name="secret">The secret name.</param>
        /// <returns>A dictionary with the metadata.</returns>
        Dictionary<string, string> Metadata(string secret);
    }
}