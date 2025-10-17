using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace RealEsrgan_GUI
{
    internal class PipeServer : IDisposable
    {
        private readonly string _pipeName;
        private readonly Action<string[]> _onReceived;
        private CancellationTokenSource _cts;
        private Task _serverTask;
        private bool _disposed;

        public PipeServer(string pipeName, Action<string[]> onReceived)
        {
            _pipeName = pipeName ?? throw new ArgumentNullException(nameof(pipeName));
            _onReceived = onReceived ?? throw new ArgumentNullException(nameof(onReceived));
        }

        public void Start()
        {
            if (_serverTask != null && !_serverTask.IsCompleted) return;
            _cts = new CancellationTokenSource();
            _serverTask = Task.Run(() => RunAsync(_cts.Token));
        }

        public void Stop()
        {
            try
            {
                if (_cts == null) return;
                if (!_cts.IsCancellationRequested) _cts.Cancel();
            }
            catch
            {
                // best-effort
            }
        }

        private async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using (var server = new NamedPipeServerStream(_pipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous))
                {
                    var connectTask = server.WaitForConnectionAsync();
                    var tcs = new TaskCompletionSource<object>();

                    using (token.Register(() => tcs.TrySetResult(null)))
                    {
                        var finished = await Task.WhenAny(connectTask, tcs.Task).ConfigureAwait(false);
                        if (finished == tcs.Task)
                        {
                            // cancellation requested
                            try { server.Dispose(); } catch { }
                            break;
                        }

                        // connected - read once
                        try
                        {
                            using (var reader = new StreamReader(server))
                            {
                                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                                if (!string.IsNullOrEmpty(line))
                                {
                                    var files = line.Split('|');
                                    try { _onReceived(files); }
                                    catch
                                    {
                                        // swallow callback exceptions to keep server alive
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // ignore transient pipe/read errors and continue loop
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Send arguments to an existing pipe server instance.
        /// Returns true if the message was delivered, false otherwise.
        /// This is a helper for single-instance startup logic.
        /// </summary>
        public static bool SendArgs(string pipeName, string[] args, int retries = 10, int connectTimeoutMs = 100)
        {
            if (string.IsNullOrEmpty(pipeName)) throw new ArgumentNullException(nameof(pipeName));
            if (args == null || args.Length == 0) return false;

            try
            {
                using (var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
                {
                    int attempts = retries;
                    while (attempts-- > 0)
                    {
                        try
                        {
                            client.Connect(connectTimeoutMs);
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(50);
                        }
                    }

                    if (!client.IsConnected) return false;

                    using (var writer = new StreamWriter(client) { AutoFlush = true })
                    {
                        string argString = string.Join("|", args);
                        writer.WriteLine(argString);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            try { _serverTask?.Wait(200); } catch { }
            _cts?.Dispose();
            _disposed = true;
        }
    }
}