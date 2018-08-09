using log4net.Core;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Gelf4Net.Appender
{
    public class AsyncGelfUdpAppender : GelfUdpAppender
    {
        private readonly ConcurrentQueue<byte[]> _pendingTasks;
        private readonly ManualResetEvent _manualResetEvent;
        private bool _onClosing;

        public AsyncGelfUdpAppender()
        {
            _pendingTasks = new ConcurrentQueue<byte[]>();
            _manualResetEvent = new ManualResetEvent(false);
            Start();
        }

        protected override void Append(LoggingEvent[] loggingEvents)
        {
            foreach (var loggingEvent in loggingEvents)
            {
                Append(loggingEvent);
            }
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (FilterEvent(loggingEvent))
            {
                _pendingTasks.Enqueue(this.RenderLoggingEvent(loggingEvent).GzipMessage(this.Encoding));
            }
        }

        private void Start()
        {
            Debug.WriteLine("[Gelf4Net] Start Async Appender");
            if (_onClosing)
            {
                Debug.WriteLine("[Gelf4Net] Closing");
                return;
            }
            var thread = new Thread(LogMessages);
            thread.Start();
        }

        private void LogMessages()
        {
            while (!_onClosing)
            {
                byte[] loggingEvent;
                while (!_pendingTasks.TryDequeue(out loggingEvent))
                {
                    Thread.Sleep(10);
                    if (_onClosing)
                    {
                        try
                        {
                            Debug.WriteLine("[Gelf4Net] Appending");
                            base.SendMessage(_pendingTasks.ToArray());
                            break;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                            break;
                        }
                    }
                }
                if (loggingEvent != null)
                {
                    Debug.WriteLine("[Gelf4Net] Appending 2");
                    base.SendMessage(loggingEvent);
                }
            }
            _manualResetEvent.Set();
        }

        protected override void OnClose()
        {
            Debug.WriteLine("[Gelf4Net] Closing Async Appender");
            _onClosing = true;
            _manualResetEvent.WaitOne(TimeSpan.FromSeconds(10));
            Debug.WriteLine("[Gelf4Net] Logging thread has stopped");
            base.OnClose();
        }
    }
}