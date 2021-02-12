﻿namespace NServiceBus.Transport.SqlServer
{
    using System;
#if SYSTEMDATASQLCLIENT
    using System.Data.SqlClient;
#else
    using Microsoft.Data.SqlClient;
#endif
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility;

    abstract class ReceiveStrategy
    {
        TableBasedQueue inputQueue;
        TableBasedQueue errorQueue;

        OnMessage onMessage;
        OnError onError;

        protected ReceiveStrategy(TableBasedQueueCache tableBasedQueueCache)
        {
            this.tableBasedQueueCache = tableBasedQueueCache;
        }

        public void Init(TableBasedQueue inputQueue, TableBasedQueue errorQueue, OnMessage onMessage, OnError onError, Action<string, Exception> criticalError)
        {
            this.inputQueue = inputQueue;
            this.errorQueue = errorQueue;

            this.onMessage = onMessage;
            this.onError = onError;
            this.criticalError = criticalError;
        }

        public abstract Task ReceiveMessage(CancellationTokenSource receiveCancellationTokenSource);

        protected async Task<Message> TryReceive(SqlConnection connection, SqlTransaction transaction, CancellationTokenSource receiveCancellationTokenSource)
        {
            var receiveResult = await inputQueue.TryReceive(connection, transaction).ConfigureAwait(false);

            if (receiveResult.IsPoison)
            {
                await errorQueue.DeadLetter(receiveResult.PoisonMessage, connection, transaction).ConfigureAwait(false);
                return null;
            }

            if (receiveResult.Successful)
            {
                if (await TryHandleDelayedMessage(receiveResult.Message, connection, transaction).ConfigureAwait(false))
                {
                    return null;
                }

                return receiveResult.Message;
            }
            receiveCancellationTokenSource.Cancel();
            return null;
        }

        protected async Task<bool> TryProcessingMessage(Message message, TransportTransaction transportTransaction)
        {
            //Do not process expired messages
            if (message.Expired == false)
            {
                var messageContext = new MessageContext(message.TransportId, message.Headers, message.Body, transportTransaction, new ContextBag());
                await onMessage(messageContext).ConfigureAwait(false);
            }

            return true;
        }

        protected async Task<ErrorHandleResult> HandleError(Exception exception, Message message, TransportTransaction transportTransaction, int processingAttempts)
        {
            message.ResetHeaders();
            try
            {
                var errorContext = new ErrorContext(exception, message.Headers, message.TransportId, message.Body, transportTransaction, processingAttempts);
                errorContext.Message.Headers.Remove(ForwardHeader);

                return await onError(errorContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                criticalError($"Failed to execute recoverability policy for message with native ID: `{message.TransportId}`", ex);

                return ErrorHandleResult.RetryRequired;
            }
            finally
            {
                message.ResetHeaders();
            }
        }

        async Task<bool> TryHandleDelayedMessage(Message message, SqlConnection connection, SqlTransaction transaction)
        {
            if (message.Headers.TryGetValue(ForwardHeader, out var forwardDestination))
            {
                message.Headers.Remove(ForwardHeader);
            }

            if (forwardDestination == null)
            {
                //This is not a delayed message. Process in local endpoint instance.
                return false;
            }
            if (forwardDestination == inputQueue.Name)
            {
                //Do not forward the message. Process in local endpoint instance.
                return false;
            }
            var outgoingMessage = new OutgoingMessage(message.TransportId, message.Headers, message.Body);

            var destinationQueue = tableBasedQueueCache.Get(forwardDestination);
            await destinationQueue.Send(outgoingMessage, TimeSpan.MaxValue, connection, transaction).ConfigureAwait(false);
            return true;
        }

        const string ForwardHeader = "NServiceBus.SqlServer.ForwardDestination";
        TableBasedQueueCache tableBasedQueueCache;
        Action<string, Exception> criticalError;
    }
}