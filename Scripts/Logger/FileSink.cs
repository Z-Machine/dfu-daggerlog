using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Mods.DaggerLog
{
    public sealed class FileSink : IDisposable
    {
        private const string CLEAR_COMMAND = "$___CLEAR_FILE___$";
        private const int MAX_LOG_FILES = 5;

        private readonly BlockingCollection<string> _queue;
        private readonly Task _writeTask;
        private readonly CancellationTokenSource _cts;
        readonly string filePath;

        public FileSink(string filePath) {
            this.filePath = filePath;
            _queue = new BlockingCollection<string>(new ConcurrentQueue<string>());

            RotateLogFile();

            _cts = new CancellationTokenSource();
            _writeTask = Task.Run(ProcessQueueAsync);
        }

        public void WriteLine(Logger logger, LogType logType, string message) =>
            _queue.Add(message);

        public void ClearFile() => _queue.Add(CLEAR_COMMAND);

        public void Dispose() {
            _cts.Cancel();
            _queue.CompleteAdding();

            try {
                // Wait for task to finish.
                _writeTask.Wait();
            }
            catch(Exception)
            { }

            _cts.Dispose();
            _queue.Dispose();
        }

        private void RotateLogFile() {
            try {
                if (!File.Exists(filePath)) return;
                var timestamp = Utils.GetFileTimestamp();
                var directoryName = Path.GetDirectoryName(filePath);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                var fileExtension = Path.GetExtension(filePath);

                var newFileName = $"{fileNameWithoutExtension}_{timestamp}{fileExtension}";
                var newFilePath = Path.Combine(directoryName, newFileName);

                File.Move(filePath, newFilePath);
                
                var logFiles = Directory.GetFiles(directoryName, $"{fileNameWithoutExtension}_*{fileExtension}")
                    .OrderByDescending(File.GetCreationTimeUtc)
                    .Skip(MAX_LOG_FILES);
                
                foreach (var file in logFiles) {
                    try {
                        File.Delete(file);
                    }
                    catch(Exception)
                    { }
                }
            }
            catch(Exception)
            { }
        }

        private async Task ProcessQueueAsync() {
            try {
                using (var writer = new StreamWriter(filePath, true))
                {
                    while (!_cts.Token.IsCancellationRequested || !_queue.IsCompleted)
                    {
                        if (_queue.TryTake(out var logEntry, Timeout.Infinite))
                        {
                            if (logEntry == CLEAR_COMMAND)
                            {
                                // Truncate file w/o closing.
                                writer.BaseStream.SetLength(0);
                            }
                            else
                            {
                                await writer.WriteLineAsync(logEntry);
                                await writer.FlushAsync();
                            }
                        }
                    }
                }
            }
            catch(Exception)
            { }
        }
    }
}