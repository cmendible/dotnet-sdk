﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.Client.Test
{
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Autogenerated = Dapr.Client.Autogen.Grpc;
    using FluentAssertions;
    using Grpc.Core;
    using Grpc.Net.Client;
    using Xunit;
    using Google.Protobuf.WellKnownTypes;
    using System;
    using System.Collections.Generic;

    public class StateApiTest
    {
        [Fact]
        public async Task GetStateAsync_CanReadState()
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var task = daprClient.GetStateAsync<Widget>("testStore", "test");

            // Create Response & Respond
            var data = new Widget() { Size = "small", Color = "yellow", };
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            SendResponseWithState(data, entry);

            // Get response and validate
            var state = await task;
            state.Size.Should().Be("small");
            state.Color.Should().Be("yellow");
        }

        [Fact]
        public async Task GetStateAndEtagAsync_CanReadState()
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var task = daprClient.GetStateAndETagAsync<Widget>("testStore", "test");

            // Create Response & Respond
            var data = new Widget() { Size = "small", Color = "yellow", };
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            SendResponseWithState(data, entry, "Test_Etag");

            // Get response and validate
            var (state, etag) = await task;
            state.Size.Should().Be("small");
            state.Color.Should().Be("yellow");
            etag.Value.Should().Be("Test_Etag");
        }

        [Fact]
        public async Task GetStateAsync_CanReadEmptyState_ReturnsDefault()
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var task = daprClient.GetStateAsync<Widget>("testStore", "test", ConsistencyMode.Eventual);

            // Create Response & Respond
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            SendResponseWithState<Widget>(null, entry);

            // Get response and validate
            var state = await task;
            state.Should().BeNull();
        }

        [Theory]
        [InlineData(ConsistencyMode.Eventual, "eventual")]
        [InlineData(ConsistencyMode.Strong, "strong")]
        public async Task GetStateAsync_ValidateRequest(ConsistencyMode consistencyMode, string expectedConsistencyMode)
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var task = daprClient.GetStateAsync<Widget>("testStore", "test", consistencyMode);

            // Get Request & Validate
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            var envelope = await GrpcUtils.GetEnvelopeFromRequestMessageAsync<Autogenerated.GetStateEnvelope>(entry.Request);
            envelope.StoreName.Should().Be("testStore");
            envelope.Key.Should().Be("test");
            envelope.Consistency.Should().Be(expectedConsistencyMode);

            // Create Response & Respond
            SendResponseWithState<Widget>(null, entry);

            // Get response and validate
            var state = await task;
            state.Should().BeNull();
        }

        [Fact]
        public async Task GetStateAsync_ThrowsForNonSuccess()
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            // Create Response & Respond
            var task = daprClient.GetStateAsync<Widget>("testStore", "test");
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            var response = GrpcUtils.CreateResponse(HttpStatusCode.NotAcceptable);
            entry.Completion.SetResult(response);

            await FluentActions.Awaiting(async () => await task).Should().ThrowAsync<RpcException>();
        }

        [Fact]
        public async Task SaveStateAsync_CanSaveState()
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var widget = new Widget() { Size = "small", Color = "yellow", };
            var task = daprClient.SaveStateAsync("testStore", "test", widget);
            
            // Get Request and validate
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            var envelope = await GrpcUtils.GetEnvelopeFromRequestMessageAsync<Autogenerated.SaveStateEnvelope>(entry.Request);
            
            envelope.StoreName.Should().Be("testStore");
            envelope.Requests.Count.Should().Be(1);
            var request = envelope.Requests[0];
            request.Key.Should().Be("test");

            var stateJson = request.Value.Value.ToStringUtf8();
            var stateFromRequest = JsonSerializer.Deserialize<Widget>(stateJson);
            stateFromRequest.Size.Should().Be(widget.Size);
            stateFromRequest.Color.Should().Be(widget.Color);
        }

        [Fact]
        public async Task SaveStateAsync_CanClearState()
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var task = daprClient.SaveStateAsync<object>("testStore", "test", null);

            // Get Request and validate
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            var envelope = await GrpcUtils.GetEnvelopeFromRequestMessageAsync<Autogenerated.SaveStateEnvelope>(entry.Request);

            envelope.StoreName.Should().Be("testStore");
            envelope.Requests.Count.Should().Be(1);
            var request = envelope.Requests[0];
            request.Key.Should().Be("test");

            request.Value.Should().BeNull();
        }

        [Fact]
        public async Task SetStateAsync_ThrowsForNonSuccess()
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();
            
            var widget = new Widget() { Size = "small", Color = "yellow", };
            var task = daprClient.SaveStateAsync("testStore", "test", widget);

            // Create Response & Respond
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            var response = GrpcUtils.CreateResponse(HttpStatusCode.NotAcceptable);
            entry.Completion.SetResult(response);

            await FluentActions.Awaiting(async () => await task).Should().ThrowAsync<RpcException>();
        }

        [Fact]
        public async Task DeleteStateAsync_CanDeleteState()
        {
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var task = daprClient.DeleteStateAsync("testStore", "test");

            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            var envelope = await GrpcUtils.GetEnvelopeFromRequestMessageAsync<Autogenerated.DeleteStateEnvelope>(entry.Request);
            envelope.StoreName.Should().Be("testStore");
            envelope.Key.Should().Be("test");
        }

        [Fact]
        public async Task DeleteStateAsync_ThrowsForNonSuccess()
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var task = daprClient.DeleteStateAsync("testStore", "test");

            // Create Response & Respond
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            var response = GrpcUtils.CreateResponse(HttpStatusCode.NotAcceptable);
            entry.Completion.SetResult(response);

            await FluentActions.Awaiting(async () => await task).Should().ThrowAsync<RpcException>();
        }

        [Fact]
        public async Task GetStateEntryAsync_CanReadState()
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var task = daprClient.GetStateEntryAsync<Widget>("testStore", "test");

            // Create Response & Respond
            var data = new Widget() { Size = "small", Color = "yellow", };
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            SendResponseWithState(data, entry);

            // Get response and validate
            var state = await task;
            state.Value.Size.Should().Be("small");
            state.Value.Color.Should().Be("yellow");
        }

        [Fact]
        public async Task GetStateEntryAsync_CanReadEmptyState_ReturnsDefault()
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var task = daprClient.GetStateEntryAsync<Widget>("testStore", "test");

            // Create Response & Respond
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            SendResponseWithState<Widget>(null, entry);

            var state = await task;
            state.Key.Should().Be("test");
            state.Value.Should().BeNull();
        }

        [Fact]
        public async Task GetStateEntryAsync_CanSaveState()
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var task = daprClient.GetStateEntryAsync<Widget>("testStore", "test");

            // Create Response & Respond
            var data = new Widget() { Size = "small", Color = "yellow", };
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            SendResponseWithState(data, entry);

            var state = await task;
            state.Key.Should().Be("test");
            state.Value.Size.Should().Be("small");
            state.Value.Color.Should().Be("yellow");

            // Modify the state and save it
            state.Value.Color = "green";
            var task2 = state.SaveAsync();

            // Get Request and validate
            httpClient.Requests.TryDequeue(out entry).Should().BeTrue();
            var envelope = await GrpcUtils.GetEnvelopeFromRequestMessageAsync<Autogenerated.SaveStateEnvelope>(entry.Request);

            envelope.StoreName.Should().Be("testStore");
            envelope.Requests.Count.Should().Be(1);
            var request = envelope.Requests[0];
            request.Key.Should().Be("test");

            var stateJson = request.Value.Value.ToStringUtf8();
            var stateFromRequest = JsonSerializer.Deserialize<Widget>(stateJson);
            stateFromRequest.Size.Should().Be("small");
            stateFromRequest.Color.Should().Be("green");
        }

        [Fact]
        public async Task GetStateEntryAsync_CanDeleteState()
        {
            // Configure client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var task = daprClient.GetStateEntryAsync<Widget>("testStore", "test");

            // Create Response & Respond
            var data = new Widget() { Size = "small", Color = "yellow", };
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            SendResponseWithState(data, entry);

            var state = await task;
            state.Key.Should().Be("test");
            state.Value.Size.Should().Be("small");
            state.Value.Color.Should().Be("yellow");

            state.Value.Color = "green";
            var task2 = state.DeleteAsync();

            // Get Request and validate
            httpClient.Requests.TryDequeue(out entry).Should().BeTrue();
            var envelope = await GrpcUtils.GetEnvelopeFromRequestMessageAsync<Autogenerated.DeleteStateEnvelope>(entry.Request); 
            envelope.StoreName.Should().Be("testStore");
            envelope.Key.Should().Be("test");
        }

        [Theory]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.FirstWrite, RetryMode.Exponential, Constants.Eventual, Constants.FirstWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.FirstWrite, RetryMode.Linear, Constants.Eventual, Constants.FirstWrite, Constants.Linear)]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.LastWrite, RetryMode.Exponential, Constants.Eventual, Constants.LastWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.LastWrite, RetryMode.Linear, Constants.Eventual, Constants.LastWrite, Constants.Linear)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.FirstWrite, RetryMode.Exponential, Constants.Strong, Constants.FirstWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.FirstWrite, RetryMode.Linear, Constants.Strong, Constants.FirstWrite, Constants.Linear)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.LastWrite, RetryMode.Exponential, Constants.Strong, Constants.LastWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.LastWrite, RetryMode.Linear, Constants.Strong, Constants.LastWrite, Constants.Linear)]
        public async Task SaveStateAsync_ValidateOptions(
            ConsistencyMode consistencyMode,
            ConcurrencyMode concurrencyMode,
            RetryMode retryMode,
            string expectedConsistency,
            string expectedConcurrency,
            string expectedRetryMode)
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var widget = new Widget() { Size = "small", Color = "yellow", };
            var stateOptions = new StateOptions
            {
                Concurrency = concurrencyMode,
                Consistency = consistencyMode,
                RetryOptions = new RetryOptions
                {
                    RetryInterval = TimeSpan.FromSeconds(5),
                    RetryMode = retryMode,
                    RetryThreshold = 10
                }
            };

            var metadata = new Dictionary<string, string>();
            metadata.Add("key1", "value1");
            metadata.Add("key2", "value2");
            var task = daprClient.SaveStateAsync("testStore", "test", widget, stateOptions, metadata);

            // Get Request and validate
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            var envelope = await GrpcUtils.GetEnvelopeFromRequestMessageAsync<Autogenerated.SaveStateEnvelope>(entry.Request);
            envelope.StoreName.Should().Be("testStore");
            envelope.Requests.Count.Should().Be(1);
            var request = envelope.Requests[0];
            request.Key.Should().Be("test");
            request.Metadata.Count.Should().Be(2);
            request.Metadata.Keys.Contains("key1").Should().BeTrue();
            request.Metadata.Keys.Contains("key2").Should().BeTrue();
            request.Metadata["key1"].Should().Be("value1");
            request.Metadata["key2"].Should().Be("value2");
            request.Options.Concurrency.Should().Be(expectedConcurrency);
            request.Options.Consistency.Should().Be(expectedConsistency);
            request.Options.RetryPolicy.Pattern.Should().Be(expectedRetryMode);
            request.Options.RetryPolicy.Threshold.Should().Be(10);
            request.Options.RetryPolicy.Interval.Seconds.Should().Be(5);

            var stateJson = request.Value.Value.ToStringUtf8();
            var stateFromRequest = JsonSerializer.Deserialize<Widget>(stateJson);
            stateFromRequest.Size.Should().Be(widget.Size);
            stateFromRequest.Color.Should().Be(widget.Color);
        }

        [Theory]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.FirstWrite, RetryMode.Exponential, Constants.Eventual, Constants.FirstWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.FirstWrite, RetryMode.Linear, Constants.Eventual, Constants.FirstWrite, Constants.Linear)]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.LastWrite, RetryMode.Exponential, Constants.Eventual, Constants.LastWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.LastWrite, RetryMode.Linear, Constants.Eventual, Constants.LastWrite, Constants.Linear)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.FirstWrite, RetryMode.Exponential, Constants.Strong, Constants.FirstWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.FirstWrite, RetryMode.Linear, Constants.Strong, Constants.FirstWrite, Constants.Linear)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.LastWrite, RetryMode.Exponential, Constants.Strong, Constants.LastWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.LastWrite, RetryMode.Linear, Constants.Strong, Constants.LastWrite, Constants.Linear)]
        public async Task TrySaveStateAsync_ValidateOptions(
            ConsistencyMode consistencyMode,
            ConcurrencyMode concurrencyMode,
            RetryMode retryMode,
            string expectedConsistency,
            string expectedConcurrency,
            string expectedRetryMode)
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var widget = new Widget() { Size = "small", Color = "yellow", };
            var stateOptions = new StateOptions
            {
                Concurrency = concurrencyMode,
                Consistency = consistencyMode,
                RetryOptions = new RetryOptions
                {
                    RetryInterval = TimeSpan.FromSeconds(5),
                    RetryMode = retryMode,
                    RetryThreshold = 10
                }
            };

            var metadata = new Dictionary<string, string>();
            metadata.Add("key1", "value1");
            metadata.Add("key2", "value2");
            var task = daprClient.TrySaveStateAsync("testStore", "test", widget, "Test_Etag", stateOptions, metadata);

            // Get Request and validate
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            var envelope = await GrpcUtils.GetEnvelopeFromRequestMessageAsync<Autogenerated.SaveStateEnvelope>(entry.Request);
            envelope.StoreName.Should().Be("testStore");
            envelope.Requests.Count.Should().Be(1);
            var request = envelope.Requests[0];
            request.Key.Should().Be("test");
            request.Etag.Should().Be("Test_Etag");
            request.Metadata.Count.Should().Be(2);
            request.Metadata.Keys.Contains("key1").Should().BeTrue();
            request.Metadata.Keys.Contains("key2").Should().BeTrue();
            request.Metadata["key1"].Should().Be("value1");
            request.Metadata["key2"].Should().Be("value2");
            request.Options.Concurrency.Should().Be(expectedConcurrency);
            request.Options.Consistency.Should().Be(expectedConsistency);
            request.Options.RetryPolicy.Pattern.Should().Be(expectedRetryMode);
            request.Options.RetryPolicy.Threshold.Should().Be(10);
            request.Options.RetryPolicy.Interval.Seconds.Should().Be(5);

            var stateJson = request.Value.Value.ToStringUtf8();
            var stateFromRequest = JsonSerializer.Deserialize<Widget>(stateJson);
            stateFromRequest.Size.Should().Be(widget.Size);
            stateFromRequest.Color.Should().Be(widget.Color);
        }

        [Theory]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.FirstWrite, RetryMode.Exponential, Constants.Eventual, Constants.FirstWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.FirstWrite, RetryMode.Linear, Constants.Eventual, Constants.FirstWrite, Constants.Linear)]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.LastWrite, RetryMode.Exponential, Constants.Eventual, Constants.LastWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.LastWrite, RetryMode.Linear, Constants.Eventual, Constants.LastWrite, Constants.Linear)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.FirstWrite, RetryMode.Exponential, Constants.Strong, Constants.FirstWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.FirstWrite, RetryMode.Linear, Constants.Strong, Constants.FirstWrite, Constants.Linear)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.LastWrite, RetryMode.Exponential, Constants.Strong, Constants.LastWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.LastWrite, RetryMode.Linear, Constants.Strong, Constants.LastWrite, Constants.Linear)]
        public async Task DeleteStateAsync_ValidateOptions(
            ConsistencyMode consistencyMode,
            ConcurrencyMode concurrencyMode,
            RetryMode retryMode,
            string expectedConsistency,
            string expectedConcurrency,
            string expectedRetryMode)
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var stateOptions = new StateOptions
            {
                Concurrency = concurrencyMode,
                Consistency = consistencyMode,
                RetryOptions = new RetryOptions
                {
                    RetryInterval = TimeSpan.FromSeconds(5),
                    RetryMode = retryMode,
                    RetryThreshold = 10
                }
            };

            var task = daprClient.DeleteStateAsync("testStore", "test", stateOptions);

            // Get Request and validate
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            var envelope = await GrpcUtils.GetEnvelopeFromRequestMessageAsync<Autogenerated.DeleteStateEnvelope>(entry.Request);
            envelope.StoreName.Should().Be("testStore");
            envelope.Key.Should().Be("test");
            envelope.Options.Concurrency.Should().Be(expectedConcurrency);
            envelope.Options.Consistency.Should().Be(expectedConsistency);
            envelope.Options.RetryPolicy.Pattern.Should().Be(expectedRetryMode);
            envelope.Options.RetryPolicy.Threshold.Should().Be(10);
            envelope.Options.RetryPolicy.Interval.Seconds.Should().Be(5);
        }

        [Theory]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.FirstWrite, RetryMode.Exponential, Constants.Eventual, Constants.FirstWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.FirstWrite, RetryMode.Linear, Constants.Eventual, Constants.FirstWrite, Constants.Linear)]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.LastWrite, RetryMode.Exponential, Constants.Eventual, Constants.LastWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Eventual, ConcurrencyMode.LastWrite, RetryMode.Linear, Constants.Eventual, Constants.LastWrite, Constants.Linear)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.FirstWrite, RetryMode.Exponential, Constants.Strong, Constants.FirstWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.FirstWrite, RetryMode.Linear, Constants.Strong, Constants.FirstWrite, Constants.Linear)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.LastWrite, RetryMode.Exponential, Constants.Strong, Constants.LastWrite, Constants.Exponential)]
        [InlineData(ConsistencyMode.Strong, ConcurrencyMode.LastWrite, RetryMode.Linear, Constants.Strong, Constants.LastWrite, Constants.Linear)]
        public async Task TryDeleteStateAsync_ValidateOptions(
            ConsistencyMode consistencyMode,
            ConcurrencyMode concurrencyMode,
            RetryMode retryMode,
            string expectedConsistency,
            string expectedConcurrency,
            string expectedRetryMode)
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var stateOptions = new StateOptions
            {
                Concurrency = concurrencyMode,
                Consistency = consistencyMode,
                RetryOptions = new RetryOptions
                {
                    RetryInterval = TimeSpan.FromSeconds(5),
                    RetryMode = retryMode,
                    RetryThreshold = 10
                }
            };

            var task = daprClient.TryDeleteStateAsync("testStore", "test", "Test_Etag", stateOptions);

            // Get Request and validate
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            var envelope = await GrpcUtils.GetEnvelopeFromRequestMessageAsync<Autogenerated.DeleteStateEnvelope>(entry.Request);
            envelope.StoreName.Should().Be("testStore");
            envelope.Key.Should().Be("test");
            envelope.Etag.Should().Be("Test_Etag");
            envelope.Options.Concurrency.Should().Be(expectedConcurrency);
            envelope.Options.Consistency.Should().Be(expectedConsistency);
            envelope.Options.RetryPolicy.Pattern.Should().Be(expectedRetryMode);
            envelope.Options.RetryPolicy.Threshold.Should().Be(10);
            envelope.Options.RetryPolicy.Interval.Seconds.Should().Be(5);
        }

        private async void SendResponseWithState<T>(T state, TestHttpClient.Entry entry, ETag etag = null)
        {
            var stateAny = await ProtobufUtils.ConvertToAnyAsync(state);
            var stateResponse = new Autogenerated.GetStateResponseEnvelope();
            stateResponse.Data = stateAny;

            if (etag != null)
            {
                stateResponse.Etag = etag.Value;
            }

            var streamContent = await GrpcUtils.CreateResponseContent(stateResponse);
            var response = GrpcUtils.CreateResponse(HttpStatusCode.OK, streamContent);
            entry.Completion.SetResult(response);
        }

        private class Widget
        {
            public string Size { get; set; }

            public string Color { get; set; }
        }
    }
}
