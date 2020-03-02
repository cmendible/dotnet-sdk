// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

using System;
using System.Net.Http;
using Dapr.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.DaprSecretStore;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for registering <see cref="DaprSecretStoreConfigurationProvider"/> with <see cref="IConfigurationBuilder"/>.
    /// </summary>
    public static class DaprSecretStoreConfigurationExtensions
    {
        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from the Dapr Secret Store.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="store">Dapr Secre Store name.</param>
        /// <param name="secrets">The secrets to retrieve.</param>
        /// <param name="client">Dapr client used to retrieve Secrets</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddDaprSecretStore(
            this IConfigurationBuilder configurationBuilder,
            string store,
            DaprSecretDescriptor[] secrets,
            DaprClient client)
        {
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }

            if (secrets == null)
            {
                throw new ArgumentNullException(nameof(secrets));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            configurationBuilder.Add(new DaprSecretStoreConfigurationSource()
            {
                Store = store,
                Secrets = secrets,
                Client = client
            });

            return configurationBuilder;
        }
    }
}