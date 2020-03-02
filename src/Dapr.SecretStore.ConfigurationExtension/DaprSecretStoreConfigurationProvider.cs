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
        private const string DaprDefaultEndpoint = "127.0.0.1";

        public const string SecretPath = "/v1.0/secrets";

        private readonly string _store;

        private readonly DaprSecretDescriptor[] _secrets;

        private readonly DaprClient _client;

        private static string DefaultHttpPort => Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500";

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
                var result = await _client.GetSecretAsync(_store, secret.SecretName, (Dictionary<string, string>)secret.Metadata);

                foreach (var returnedKey in result.Keys)
                {
                    var key = returnedKey != "_default" ? returnedKey : secret.SecretName;
                    data.Add(key, result[returnedKey]);
                }
            }

            Data = data;
        }
    }
}