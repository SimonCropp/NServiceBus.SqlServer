﻿namespace NServiceBus.Transports.SQLServer
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using Logging;

    class MessagePump : IPushMessages
    {
        public MessagePump(Func<TransportTransactionMode, ReceiveStrategy> receiveStrategyFactory, SqlConnectionFactory connectionFactory, QueueAddressParser addressParser, TimeSpan waitTimeCircuitBreaker)
        {
            this.receiveStrategyFactory = receiveStrategyFactory;
            this.connectionFactory = connectionFactory;
            this.addressParser = addressParser;
            this.waitTimeCircuitBreaker = waitTimeCircuitBreaker;
        }

        public async Task Init(Func<PushContext, Task> pipe, CriticalError criticalError, PushSettings settings)
        {
            pipeline = pipe;

            receiveStrategy = receiveStrategyFactory(settings.RequiredTransactionMode);

            peekCircuitBreaker = new RepeatedFailuresOverTimeCircuitBreaker("SqlPeek", waitTimeCircuitBreaker, ex => criticalError.Raise("Failed to peek " + settings.InputQueue, ex));
            receiveCircuitBreaker = new RepeatedFailuresOverTimeCircuitBreaker("ReceiveText", waitTimeCircuitBreaker, ex => criticalError.Raise("Failed to receive from " + settings.InputQueue, ex));

            inputQueue = new TableBasedQueue(addressParser.Parse(settings.InputQueue));
            errorQueue = new TableBasedQueue(addressParser.Parse(settings.ErrorQueue));

            if (settings.PurgeOnStartup)
            {
                using (var connection = await connectionFactory.OpenNewConnection().ConfigureAwait(false))
                {
                    var purgedRowsCount = await inputQueue.Purge(connection).ConfigureAwait(false);

                    Logger.InfoFormat("{0} messages was purged from table {1}", purgedRowsCount, settings.InputQueue);
                }
            }
        }

        public void Start(PushRuntimeSettings limitations)
        {
            runningReceiveTasks = new ConcurrentDictionary<Task, Task>();
            concurrencyLimiter = new SemaphoreSlim(limitations.MaxConcurrency);
            cancellationTokenSource = new CancellationTokenSource();

            cancellationToken = cancellationTokenSource.Token;

            messagePumpTask = Task.Run(ProcessMessages, CancellationToken.None);
        }

        public async Task Stop()
        {
            cancellationTokenSource.Cancel();

            // ReSharper disable once MethodSupportsCancellation
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
            var allTasks = runningReceiveTasks.Values.Concat(new[]
            {
                messagePumpTask
            });
            var finishedTask = await Task.WhenAny(Task.WhenAll(allTasks), timeoutTask).ConfigureAwait(false);

            if (finishedTask.Equals(timeoutTask))
            {
                Logger.Error("The message pump failed to stop with in the time allowed(30s)");
            }

            concurrencyLimiter.Dispose();
            cancellationTokenSource.Dispose();

            runningReceiveTasks.Clear();
        }

        async Task ProcessMessages()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await InnerProcessMessages().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // For graceful shutdown purposes
                }
                catch (Exception ex)
                {
                    Logger.Error("Sql Message pump failed", ex);
                    await peekCircuitBreaker.Failure(ex).ConfigureAwait(false);
                }
            }
        }

        async Task InnerProcessMessages()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                int messageCount;

                try
                {
                    using (var connection = await connectionFactory.OpenNewConnection().ConfigureAwait(false))
                    {
                        messageCount = await inputQueue.TryPeek(connection, cancellationToken).ConfigureAwait(false);

                        peekCircuitBreaker.Success();

                        if (messageCount == 0)
                        {
                            await Task.Delay(peekDelay, cancellationToken).ConfigureAwait(false);
                            continue;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    Logger.Warn("Sql peek operation failed", ex);
                    await peekCircuitBreaker.Failure(ex).ConfigureAwait(false);
                    continue;
                }

                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                for (var i = 0; i < messageCount; i++)
                {
                    await concurrencyLimiter.WaitAsync(cancellationToken).ConfigureAwait(false);

                    var tokenSource = new CancellationTokenSource();

                    var receiveTask = Task.Run(async () =>
                    {
                        try
                        {
                            await receiveStrategy.ReceiveMessage(inputQueue, errorQueue, tokenSource, pipeline)
                                .ConfigureAwait(false);

                            receiveCircuitBreaker.Success();
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn("Sql receive operation failed", ex);
                            await receiveCircuitBreaker.Failure(ex).ConfigureAwait(false);
                        }
                        finally
                        {
                            concurrencyLimiter.Release();
                        }
                    }, cancellationToken).ContinueWith(t => tokenSource.Dispose());

                    runningReceiveTasks.TryAdd(receiveTask, receiveTask);

                    // We insert the original task into the runningReceiveTasks because we want to await the completion
                    // of the running receives. ExecuteSynchronously is a request to execute the continuation as part of
                    // the transition of the antecedents completion phase. This means in most of the cases the continuation
                    // will be executed during this transition and the antecedent task goes into the completion state only 
                    // after the continuation is executed. This is not always the case. When the TPL thread handling the
                    // antecedent task is aborted the continuation will be scheduled. But in this case we don't need to await
                    // the continuation to complete because only really care about the receive operations. The final operation
                    // when shutting down is a clear of the running tasks anyway.
                    receiveTask.ContinueWith(t =>
                    {
                        Task toBeRemoved;
                        runningReceiveTasks.TryRemove(t, out toBeRemoved);
                    }, TaskContinuationOptions.ExecuteSynchronously)
                    .Ignore();
                }
            }
        }

        TableBasedQueue inputQueue;
        TableBasedQueue errorQueue;
        Func<PushContext, Task> pipeline;
        Func<TransportTransactionMode, ReceiveStrategy> receiveStrategyFactory;
        SqlConnectionFactory connectionFactory;
        QueueAddressParser addressParser;
        TimeSpan waitTimeCircuitBreaker;
        ConcurrentDictionary<Task, Task> runningReceiveTasks;
        SemaphoreSlim concurrencyLimiter;
        CancellationTokenSource cancellationTokenSource;
        CancellationToken cancellationToken;
        RepeatedFailuresOverTimeCircuitBreaker peekCircuitBreaker;
        RepeatedFailuresOverTimeCircuitBreaker receiveCircuitBreaker;

        static ILog Logger = LogManager.GetLogger<MessagePump>();
        Task messagePumpTask;
        ReceiveStrategy receiveStrategy;
        static TimeSpan peekDelay = TimeSpan.FromSeconds(1);
    }
}