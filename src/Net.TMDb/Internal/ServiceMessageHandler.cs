using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.TMDb.Internal
{
    internal class ServiceMessageHandler : DelegatingHandler
    {
        public ServiceMessageHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return new AsyncExecution(() => base.SendAsync(request, cancellationToken), cancellationToken).ExecuteAsync();
        }

        internal sealed class AsyncExecution
        {
            private readonly Func<Task<HttpResponseMessage>> taskFunc;
            private readonly CancellationToken cancellationToken;

            private Task<HttpResponseMessage> previousTask;

            static readonly long UnixEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

            public AsyncExecution(Func<Task<HttpResponseMessage>> taskFunc, CancellationToken cancellationToken)
            {
                this.taskFunc = taskFunc;
                this.cancellationToken = cancellationToken;
                this.previousTask = null;
            }

            internal Task<HttpResponseMessage> ExecuteAsync()
            {
                return this.ExecuteAsyncImpl(null);
            }

            private Task<HttpResponseMessage> ExecuteAsyncContinueWith(Task<HttpResponseMessage> runningTask)
            {
                if (!runningTask.IsFaulted || this.cancellationToken.IsCancellationRequested)
                {
                    var response = runningTask.Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        if ((int)response.StatusCode == 429)
                        {
                            var delay = response.Headers.RetryAfter.Delta.Value + TimeSpan.FromSeconds(1);
                            this.previousTask = runningTask;

                            return Task.Delay(delay, this.cancellationToken)
                                .ContinueWith(this.ExecuteAsyncImpl, CancellationToken.None,
                                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default)
                                .Unwrap();
                        }
                        return ServiceRequestException.ConvertResponseAsync(response);
                    }
                    IEnumerable<string> values;
                    if (response.Headers.TryGetValues("X-RateLimit-Remaining", out values) && values.Any())
                    {
                        if (Convert.ToInt32(values.First()) == 0)
                        {
                            if (response.Headers.TryGetValues("X-RateLimit-Reset", out values) && values.Any())
                            {
                                var delayTicks = UnixEpochTicks + (Convert.ToInt64(values.First()) + 1) * TimeSpan.TicksPerSecond - DateTime.UtcNow.Ticks;
                                this.previousTask = runningTask;

                                return Task.Delay(TimeSpan.FromTicks(delayTicks), this.cancellationToken)
                                    .ContinueWith(this.ExecuteAsyncImpl, CancellationToken.None,
                                        TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default)
                                    .Unwrap();
                            }
                        }
                    }
                    return runningTask;
                }
                else
                {
                    TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
                    tcs.TrySetException(runningTask.Exception);
                    return tcs.Task;
                }
            }

            private Task<HttpResponseMessage> ExecuteAsyncImpl(Task ignore)
            {
                if (this.cancellationToken.IsCancellationRequested)
                {
                    if (this.previousTask != null)
                    {
                        return this.previousTask;
                    }
                    TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
                    tcs.TrySetCanceled();
                    return tcs.Task;
                }
                return this.taskFunc()
                    .ContinueWith(this.ExecuteAsyncContinueWith, CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default)
                    .Unwrap();
            }
        }
    }
}