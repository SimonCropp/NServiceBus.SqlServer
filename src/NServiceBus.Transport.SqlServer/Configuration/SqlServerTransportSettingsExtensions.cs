﻿#pragma warning disable PS0013 // A Func used as a method parameter with a Task, ValueTask, or ValueTask<T> return type argument should have at least one CancellationToken parameter type argument unless it has a parameter type argument implementing ICancellableContext

namespace NServiceBus
{
#if SYSTEMDATASQLCLIENT
    using System.Data.SqlClient;
#else
    using Microsoft.Data.SqlClient;
#endif
    using System;
    using System.Threading.Tasks;
    using System.Transactions;
    using Transport.SqlServer;

    /// <summary>
    /// Provides support for <see cref="UseTransport{T}"/> transport APIs.
    /// </summary>
    public static partial class SqlServerTransportSettingsExtensions
    {
        /// <summary>
        /// Configures NServiceBus to use the given transport.
        /// </summary>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "EndpointConfiguration.UseTransport(TransportDefinition)",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> UseTransport<T>(this EndpointConfiguration config)
            where T : SqlServerTransport
        {
            var transport = new SqlServerTransport();

            var routing = config.UseTransport(transport);

            var settings = new TransportExtensions<SqlServerTransport>(transport, routing);

            return settings;
        }

        /// <summary>
        ///     Sets a default schema for both input and output queues
        /// </summary>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "SqlServerTransport.DefaultSchema",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> DefaultSchema(
            this TransportExtensions<SqlServerTransport> transportExtensions, string schemaName)
        {
            transportExtensions.Transport.DefaultSchema = schemaName;

            return transportExtensions;
        }

        /// <summary>
        ///     Sets a default schema for both input and output queues
        /// </summary>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "SqlServerTransport.DefaultCatalog",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> DefaultCatalog(
            this TransportExtensions<SqlServerTransport> transportExtensions, string catalogName)
        {
            transportExtensions.Transport.DefaultCatalog = catalogName;

            return transportExtensions;
        }

        /// <summary>
        ///     Specifies custom schema for given endpoint.
        /// </summary>
        /// <param name="endpointName">Endpoint name.</param>
        /// <param name="schema">Custom schema value.</param>
        /// <param name="transportExtensions">The transport settings to configure.</param>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "RoutingSettings.UseSchemaForEndpoint",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> UseSchemaForEndpoint(
            this TransportExtensions<SqlServerTransport> transportExtensions, string endpointName, string schema)
        {
            transportExtensions.Routing().UseSchemaForEndpoint(endpointName, schema);

            return transportExtensions;
        }

        /// <summary>
        /// Overrides schema value for given queue. This setting will take precedence over any other source of schema
        /// information.
        /// </summary>
        /// <param name="queueName">Queue name.</param>
        /// <param name="schema">Custom schema value.</param>
        /// <param name="transportExtensions">The transport settings to configure.</param>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "SqlServerTransport.SchemaAndCatalog.UseSchemaForQueue",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> UseSchemaForQueue(
            this TransportExtensions<SqlServerTransport> transportExtensions, string queueName, string schema)
        {
            transportExtensions.Transport.SchemaAndCatalog.UseSchemaForQueue(queueName, schema);

            return transportExtensions;
        }

        /// <summary>
        ///  Specifies custom schema for given endpoint.
        /// </summary>
        /// <param name="endpointName">Endpoint name.</param>
        /// <param name="catalog">Custom catalog value.</param>
        /// <param name="transportExtensions">The transport settings to configure.</param>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "RoutingSettings.UseCatalogForEndpoint",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> UseCatalogForEndpoint(
            this TransportExtensions<SqlServerTransport> transportExtensions, string endpointName, string catalog)
        {
            transportExtensions.Routing().UseCatalogForEndpoint(endpointName, catalog);

            return transportExtensions;
        }

        /// <summary>
        /// Specifies custom schema for given queue.
        /// </summary>
        /// <param name="queueName">Queue name.</param>
        /// <param name="catalog">Custom catalog value.</param>
        /// <param name="transportExtensions">The transport settings to configure.</param>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "SqlServerTransport.SchemaAndCatalog.UseCatalogForQueue",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> UseCatalogForQueue(
            this TransportExtensions<SqlServerTransport> transportExtensions, string queueName, string catalog)
        {
            transportExtensions.Transport.SchemaAndCatalog.UseCatalogForQueue(queueName, catalog);

            return transportExtensions;
        }

        /// <summary>
        /// Overrides the default time to wait before triggering a circuit breaker that initiates the endpoint shutdown
        /// procedure in case there are numerous errors while trying to receive messages.
        /// </summary>
        /// <param name="waitTime">Time to wait before triggering the circuit breaker.</param>
        /// <param name="transportExtensions">The transport settings to configure.</param>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "SqlServerTransport.TimeToWaitBeforeTriggeringCircuitBreaker",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> TimeToWaitBeforeTriggeringCircuitBreaker(
            this TransportExtensions<SqlServerTransport> transportExtensions, TimeSpan waitTime)
        {
            transportExtensions.Transport.TimeToWaitBeforeTriggeringCircuitBreaker = waitTime;

            return transportExtensions;
        }

        /// <summary>
        /// Specifies connection factory to be used by sql transport.
        /// </summary>
        /// <param name="connectionString">Sql Server instance connection string.</param>
        /// <param name="transportExtensions">The transport settings to configure.</param>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "configuration.UseTransport(new SqlServerTransport(string connectionString))",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> ConnectionString(
            this TransportExtensions<SqlServerTransport> transportExtensions, string connectionString)
        {
            transportExtensions.Transport.ConnectionString = connectionString;

            return transportExtensions;
        }

        /// <summary>
        /// Configures the transport to use the given func as the connection string.
        /// </summary>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            Message = "This transport does not support a connection string.",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> ConnectionString(
            this TransportExtensions<SqlServerTransport> transportExtensions, Func<string> connectionString)
        {
            transportExtensions.Transport.ConnectionString = connectionString.Invoke();

            return transportExtensions;
        }

        /// <summary>
        /// Specifies connection factory to be used by sql transport.
        /// </summary>
        /// <param name="sqlConnectionFactory">Factory that returns connection ready for usage.</param>
        /// <param name="transportExtensions">The transport settings to configure.</param>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "SqlServerTransport.ConnectionFactory",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> UseCustomSqlConnectionFactory(
            this TransportExtensions<SqlServerTransport> transportExtensions,
            Func<Task<SqlConnection>> sqlConnectionFactory)
        {
            transportExtensions.Transport.ConnectionFactory = async (_) => await sqlConnectionFactory().ConfigureAwait(false);

            return transportExtensions;
        }

        /// <summary>
        /// Allows the <see cref="IsolationLevel" /> and transaction timeout to be changed for the
        /// <see cref="TransactionScope" /> used to receive messages.
        /// </summary>
        /// <remarks>
        /// If not specified the default transaction timeout of the machine will be used and the isolation level will be set to
        /// <see cref="IsolationLevel.ReadCommitted" />.
        /// </remarks>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "SqlServerTransport.TransactionScope",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> TransactionScopeOptions(
            this TransportExtensions<SqlServerTransport> transportExtensions,
            TimeSpan? timeout = null,
            IsolationLevel? isolationLevel = null)
        {
            if (timeout.HasValue)
            {
                transportExtensions.Transport.TransactionScope.Timeout = timeout.Value;
            }

            if (isolationLevel.HasValue)
            {
                transportExtensions.Transport.TransactionScope.IsolationLevel = isolationLevel.Value;
            }

            return transportExtensions;
        }

        /// <summary>
        ///     Allows changing the queue peek delay, and the peek batch size.
        /// </summary>
        /// <param name="delay">The delay value</param>
        /// <param name="peekBatchSize">The peek batch size</param>
        /// <param name="transportExtensions">The transport settings to configure.</param>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "SqlServerTransport.QueuePeeker",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> QueuePeekerOptions(
            this TransportExtensions<SqlServerTransport> transportExtensions,
            TimeSpan? delay = null,
            int? peekBatchSize = null)
        {
            if (delay.HasValue)
            {
                transportExtensions.Transport.QueuePeeker.Delay = delay.Value;
            }

            transportExtensions.Transport.QueuePeeker.MaxRecordsToPeek = peekBatchSize;

            return transportExtensions;
        }

        /// <summary>
        /// Configures native delayed delivery.
        /// </summary>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "SqlServerTransport.DelayedDelivery",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static DelayedDeliverySettings NativeDelayedDelivery(
            this TransportExtensions<SqlServerTransport> transportExtensions) =>
            new DelayedDeliverySettings(transportExtensions.Transport.DelayedDelivery);

        /// <summary>
        /// Configures publish/subscribe behavior.
        /// </summary>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "SqlServerTransport.Subscriptions",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static SubscriptionSettings
            SubscriptionSettings(this TransportExtensions<SqlServerTransport> transportExtensions) =>
            new SubscriptionSettings(transportExtensions.Transport.Subscriptions);

        /// <summary>
        /// Instructs the transport to purge all expired messages from the input queue before starting the processing.
        /// </summary>
        /// <param name="purgeBatchSize">Size of the purge batch.</param>
        /// <param name="transportExtensions">The transport settings to configure.</param>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "SqlServerTransport.PurgeOnStartup",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> PurgeExpiredMessagesOnStartup(
            this TransportExtensions<SqlServerTransport> transportExtensions,
            int? purgeBatchSize)
        {
            transportExtensions.Transport.ExpiredMessagesPurger.PurgeOnStartup = true;
            transportExtensions.Transport.ExpiredMessagesPurger.PurgeBatchSize = purgeBatchSize;

            return transportExtensions;
        }

        /// <summary>
        /// Instructs the transport to create a computed column for inspecting message body contents.
        /// </summary>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6811",
            ReplacementTypeOrMember = "SqlServerTransport.CreateMessageBodyComputedColumn",
            Note = "Should not be converted to an ObsoleteEx until API mismatch described in issue is resolved.")]
        public static TransportExtensions<SqlServerTransport> CreateMessageBodyComputedColumn(
            this TransportExtensions<SqlServerTransport> transportExtensions)
        {
            transportExtensions.Transport.CreateMessageBodyComputedColumn = true;
            return transportExtensions;
        }
    }

    /// <summary>
    /// Configuration extensions for Message-Driven Pub-Sub compatibility mode
    /// </summary>
    public static partial class MessageDrivenPubSubCompatibilityModeConfiguration
    {
        /// <summary>
        /// Enables compatibility with endpoints running on message-driven pub-sub
        /// </summary>
        /// <param name="transportExtensions">The transport to enable pub-sub compatibility on</param>
        [PreObsolete("https://github.com/Particular/NServiceBus/issues/6471",
               Note = "Hybrid pub/sub support cannot be obsolete until there is a viable migration path to native pub/sub",
               Message = "Hybrid pub/sub is no longer supported, use native pub/sub instead")]
        public static SubscriptionMigrationModeSettings EnableMessageDrivenPubSubCompatibilityMode(
            this TransportExtensions<SqlServerTransport> transportExtensions)
        {
            var subscriptionMigrationModeSettings = transportExtensions.Routing().EnableMessageDrivenPubSubCompatibilityMode();

            return subscriptionMigrationModeSettings;
        }
    }
}