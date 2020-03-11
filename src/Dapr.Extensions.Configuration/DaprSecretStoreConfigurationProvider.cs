// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr.Client;

namespace Microsoft.Extensions.Configuration.DaprSecretStore
{
    /// <summary>
    /// A Dapr Secret Store based <see cref="ConfigurationProvider"/>.
    /// </summary>
    internal class DaprSecretStoreConfigurationProvider : ConfigurationProvider
    {
        private readonly string _store;

        private readonly DaprSecretDescriptor[] _secrets;

        private readonly DaprClient _client;

        /// <summary>
        /// Creates a new instance of <see cref="DaprSecretStoreConfigurationProvider"/>.
        /// </summary>
        /// <param name="store">Dapr Secre Store name.</param>
        /// <param name="secrets">The secrets to retrieve.</param>
        /// <param name="client">Dapr client used to retrieve Secrets</param>
        public DaprSecretStoreConfigurationProvider(string store, DaprSecretDescriptor[] secrets, DaprClient client)
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

            _store = store;
            _secrets = secrets;
            _client = client;
        }

        public override void Load() => LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        private async Task LoadAsync()
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var secret in _secrets)
            {
                var result = await _client.GetSecretAsync(_store, secret.SecretName, secret.Metadata).ConfigureAwait(false);

                foreach (var returnedKey in result.Keys)
                {
                    var key = returnedKey != "_default" ? returnedKey : secret.SecretName;
                    if (data.ContainsKey(key))
                    {
                        throw new FormatException($"A duplicate key '{key}' was found.");
                    }

                    data.Add(key, result[returnedKey]);
                }
            }

            Data = data;
        }
    }
}