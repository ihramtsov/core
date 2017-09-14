﻿using System;
using System.Threading;
using Vostok.ClusterClient.Transport.Http.Vostok.Http.ToCore.ThreadManagement;
using Vostok.Logging;

namespace Vostok.ClusterClient.Transport.Http.Vostok.Http.Common.Utility
{
    internal class ThreadPoolMonitor
    {
        public ThreadPoolMonitor()
        {
            syncObject = new object();
            lastReportTimestamp = DateTime.MinValue;
        }

        public static readonly ThreadPoolMonitor Instance = new ThreadPoolMonitor();

        public void ReportAndFixIfNeeded(ILog log)
        {
            ThreadPool.GetMinThreads(out var minWorkerThreads, out var minIocpThreads);

            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxIocpThreads);

            ThreadPool.GetAvailableThreads(out var availableWorkerThreads, out var availableIocpThreads);

            var busyWorkerThreads = maxWorkerThreads - availableWorkerThreads;
            var busyIocpThreads = maxIocpThreads - availableIocpThreads;

            if (busyWorkerThreads < minWorkerThreads && busyIocpThreads < minIocpThreads)
                return;

            var currentTimestamp = DateTime.UtcNow;

            lock (syncObject)
            {
                if (currentTimestamp - lastReportTimestamp < minReportInterval)
                    return;

                lastReportTimestamp = currentTimestamp;
            }

            log.Warn("[HttpClient] Looks like you're kinda low on ThreadPool, buddy. Workers: {0}/{1}/{2}, IOCP: {3}/{4}/{5} (busy/min/max).", 
                busyWorkerThreads, minWorkerThreads, maxWorkerThreads, 
                busyIocpThreads, minIocpThreads, maxIocpThreads);

            var currentMultiplier = Math.Min(minWorkerThreads / Environment.ProcessorCount, minIocpThreads / Environment.ProcessorCount);
            if (currentMultiplier < 128)
            {
                log.Info(@"[HttpClient] I will configure ThreadPool for you, buddy!");
                ThreadPoolUtility.SetUp(log, 128);
            }
        }

        private readonly object syncObject;
        private DateTime lastReportTimestamp;

        private static readonly TimeSpan minReportInterval = TimeSpan.FromSeconds(1);
    }
}