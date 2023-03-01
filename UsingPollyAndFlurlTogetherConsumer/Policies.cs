using Polly;
using Polly.Retry;
using Polly.Timeout;
using Polly.CircuitBreaker;
using Polly.Wrap;
using System.Diagnostics;

namespace UsingPollyAndFlurlTogetherConsumer
{
    public static class Policies
    {
        private static AsyncTimeoutPolicy<HttpResponseMessage> TimeoutPolicy
        {
            get
            {
                return Policy.TimeoutAsync<HttpResponseMessage>(2, (context, timeSpan, task) =>
                {
                    Debug.WriteLine($"[App|Policy]: Timeout delegate fired after {timeSpan.Seconds} seconds");
                    return Task.CompletedTask;
                });
            }
        }

        private static AsyncRetryPolicy<HttpResponseMessage> RetryPolicy
        {
            get
            {
                return Policy
                    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .Or<TimeoutRejectedException>()
                    .WaitAndRetryAsync(new[]
                        {
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(2),
                            TimeSpan.FromSeconds(5)
                        },
                        (delegateResult, retryCount) =>
                        {
                            Debug.WriteLine($"[App|Policy]: Retry delegate fired, attempt {retryCount}");
                        });
            }
        }

        private static AsyncCircuitBreakerPolicy CircuitBreakerPolicy(
       int numberOfExceptionsBeforeBreaking,
       int durationOfBreakInSeconds)
        {
            return Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    numberOfExceptionsBeforeBreaking,
                    TimeSpan.FromSeconds(durationOfBreakInSeconds),
                    onBreak: (_, _) =>
                    {
                        ShowCircuitState("Open (onBreak)", ConsoleColor.Red);
                    },
                    onReset: () =>
                    {
                        ShowCircuitState("Closed (onReset)", ConsoleColor.Green);
                    },
                    onHalfOpen: () =>
                    {
                        ShowCircuitState("Half Open (onHalfOpen)", ConsoleColor.Yellow);
                    });
        }

        private static void ShowCircuitState(
            string descStatus, ConsoleColor backgroundColor)
        {
            var previousBackgroundColor = Console.BackgroundColor;
            var previousForegroundColor = Console.ForegroundColor;

            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = ConsoleColor.Black;

            Console.Out.WriteLine($" ***** Estado do Circuito: {descStatus} **** ");

            Console.BackgroundColor = previousBackgroundColor;
            Console.ForegroundColor = previousForegroundColor;
        }

        public static AsyncPolicyWrap<HttpResponseMessage> PolicyStrategy => Policy.WrapAsync(RetryPolicy, TimeoutPolicy);
    }
}