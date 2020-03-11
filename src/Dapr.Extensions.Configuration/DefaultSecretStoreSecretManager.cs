// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Extensions.Configuration.DaprSecretStore
{
    /// <summary>
    /// Default implementation of <see cref="IDaprSecretStoreManager"/> that loads all secrets
    /// and replaces '--' with ':" in key names.
    /// </summary>
    public class DefaultDaprSecretStoreManager : IDaprSecretStoreManager
    {
        /// <inheritdoc />
        public virtual string GetKey(string secret)
        {
            return secret.Replace("--", ConfigurationPath.KeyDelimiter);
        }

        /// <inheritdoc />
        public virtual Dictionary<string, string> Metadata(string secret)
        {
            return new Dictionary<string, string>();
        }
    }
}