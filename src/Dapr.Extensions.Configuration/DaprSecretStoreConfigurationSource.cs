// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

using System.Net.Http;
using Dapr.Client;

namespace Microsoft.Extensions.Configuration.DaprSecretStore
{
    /// <summary>
    /// Represents Dapr Secret Store as an <see cref="IConfigurationSource"/>.
    /// </summary>
    public class DaprSecretStoreConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// Gets or sets the store name.
        /// </summary>
        public string Store { get; set; }

        /// <summary>
        /// Gets or sets the secret names.
        /// </summary>
        public DaprSecretDescriptor[] Secrets { get; set; }

        /// <summary>
        /// Gets or sets the http client.
        /// </summary>
        public DaprClient Client { get; set; }

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new DaprSecretStoreConfigurationProvider(Store, Secrets, Client);
        }
    }
}