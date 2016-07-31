using log4net.Core;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace gelf4net.Appender
{
    public class AsyncGelfUdpAppender : GelfUdpAppender
    {

        private readonly ConcurrentQueue<LoggingEvent> _pendingTasks;
        private readonly ManualResetEvent _manualResetEvent;
        private bool _onClosing;

        public AsyncGelfUdpAppender()
        {
            _pendingTasks = new ConcurrentQueue<LoggingEvent>();
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
                _pendingTasks.Enqueue(loggingEvent);
            }
        }
        private void Start()
        {
            Debug.WriteLine("[Gelf4net] Start Async Appender");
            if (_onClosing)
            {
                Debug.WriteLine("[Gelf4net] Closing");
                return;
            }
            var thread = new Thread(LogMessages);
            thread.Start();
        }
        private void LogMessages()
        {
            while (!_onClosing)
            {
                LoggingEvent loggingEvent;
                while (!_pendingTasks.TryDequeue(out loggingEvent))
                {
                    Thread.Sleep(10);
                    if (_onClosing)
                    {
                        try
                        {
                            Debug.WriteLine("[Gelf4net] Appending");
                            base.Append(_pendingTasks.ToArray());
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
                    Debug.WriteLine("[Gelf4net] Appending 2");
                    base.Append(loggingEvent);
                }
            }
            _manualResetEvent.Set();
        }
        protected override void OnClose()
        {
            Debug.WriteLine("[Gelf4net] Closing Async Appender");
            _onClosing = true;
            _manualResetEvent.WaitOne(TimeSpan.FromSeconds(10));
            Debug.WriteLine("[Gelf4net] Logging thread has stopped");
            base.OnClose();
        }
    }
}