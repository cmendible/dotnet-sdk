// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.DaprSecretStore;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Represents the name and metadata for a Secret.
    /// </summary>
    public class DaprSecretDescriptor
    {
        /// <summary>
        /// Gets or sets the secret name.
        /// </summary>
        public string SecretName { get; }

        /// <summary>
        /// Gets or sets the secret's metadata.
        /// </summary>
        public IDictionary<string, string> Metadata { get; }

        /// <summary>
        /// Secret Descriptor Construcutor
        /// </summary>
        public DaprSecretDescriptor(string secretName) : this(secretName, new Dictionary<string, string>())
        {

        }

        /// <summary>
        /// Secret Descriptor Construcutor
        /// </summary>
        public DaprSecretDescriptor(string secretName, IDictionary<string, string> metadata)
        {
            SecretName = secretName;
            Metadata = metadata;
        }
    }
}