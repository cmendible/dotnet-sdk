// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
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
        /// <param name="store">Dapr secret store name.</param>
        /// <param name="secrets">The secrets to retrieve.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddDaprSecretStore(
            this IConfigurationBuilder configurationBuilder,
            string store,
            IEnumerable<DaprSecretDescriptor> secrets)
        {
            return AddDaprSecretStore(
                configurationBuilder,
                store,
                secrets,
                (builder) => { });
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from the Dapr Secret Store.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="store">Dapr secret store name.</param>
        /// <param name="secrets">The secrets to retrieve.</param>
        /// <param name="builder">Configures the Dapr client builder</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddDaprSecretStore(
            this IConfigurationBuilder configurationBuilder,
            string store,
            IEnumerable<DaprSecretDescriptor> secrets,
            Action<DaprClientBuilder> builder)
        {
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }

            if (secrets == null)
            {
                throw new ArgumentNullException(nameof(secrets));
            }

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var daprClientBuilder = new DaprClientBuilder();
            builder.Invoke(daprClientBuilder);
            var client = daprClientBuilder.Build();

            configurationBuilder.Add(new DaprSecretStoreConfigurationSource()
            {
                Store = store,
                Secrets = secrets,
                Client = client
            });

            return configurationBuilder;
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from the command line.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddDaprSecretStore(this IConfigurationBuilder builder, Action<DaprSecretStoreConfigurationSource> configureSource)
            => builder.Add(configureSource);
    }
}