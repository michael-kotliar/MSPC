﻿// Licensed to the Genometric organization (https://github.com/Genometric) under one or more agreements.
// The Genometric organization licenses this file to you under the GNU General Public License v3.0 (GPLv3).
// See the LICENSE file in the project root for more information.

using Genometric.GeUtilities.IGenomics;
using Genometric.MSPC.Core.Model;
using Genometric.MSPC.Core.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using Genometric.GeUtilities.Intervals.Parsers.Model;

namespace Genometric.MSPC.Core
{
    public class Mspc<I>
        where I : IPeak
    {
        public event EventHandler<ValueEventArgs> StatusChanged;
        private void OnStatusValueChaned(ProgressReport value)
        {
            StatusChanged?.Invoke(this, new ValueEventArgs(value));
        }

        public AutoResetEvent Done { set; get; }
        public AutoResetEvent Canceled { set; get; }

        private Processor<I> _processor { set; get; }
        private BackgroundWorker _backgroundProcessor { set; get; }

        private ReadOnlyDictionary<uint, Result<I>> _results { set; get; }

        public int DegreeOfParallelism
        {
            set { _processor.DegreeOfParallelism = value; }
            get { return _processor.DegreeOfParallelism; }
        }

        public Mspc(IPeakConstructor<I> peakConstructor, bool trackSupportingRegions = false)
        {
            _processor = new Processor<I>(peakConstructor, trackSupportingRegions);
            _processor.OnProgressUpdate += _processorOnProgressUpdate;
            _backgroundProcessor = new BackgroundWorker();
            _backgroundProcessor.DoWork += _doWork;
            _backgroundProcessor.RunWorkerCompleted += _runWorkerCompleted;
            _backgroundProcessor.WorkerSupportsCancellation = true;
            Done = new AutoResetEvent(false);
            Canceled = new AutoResetEvent(false);
        }

        public void AddSample(uint id, Bed<I> sample)
        {
            _processor.AddSample(id, sample);
        }

        public ReadOnlyDictionary<uint, Result<I>> Run(Config config)
        {
            if (_processor.SamplesCount < 2)
                throw new InvalidOperationException(string.Format("Minimum two samples are required; {0} is given.", _processor.SamplesCount));

            _processor.Run(config, new BackgroundWorker(), new DoWorkEventArgs(null));
            _results = _processor.AnalysisResults;
            return GetResults();
        }

        public void RunAsync(Config config)
        {
            Done.Reset();
            Canceled.Reset();

            if (_processor.SamplesCount < 2)
                throw new InvalidOperationException(string.Format("Minimum two samples are required; {0} is given.", _processor.SamplesCount));

            if (_backgroundProcessor.IsBusy)
                Cancel();
            _backgroundProcessor.RunWorkerAsync(config);
        }

        public void Cancel()
        {
            Canceled.Reset();
            _backgroundProcessor.CancelAsync();
            Canceled.WaitOne();
            Done.Reset();
            Canceled.Reset();
        }

        public ReadOnlyDictionary<uint, Result<I>> GetResults()
        {
            return _results;
        }

        public ReadOnlyDictionary<string, List<ProcessedPeak<I>>> GetConsensusPeaks()
        {
            return _processor.ConsensusPeaks;
        }

        private void _doWork(object sender, DoWorkEventArgs e)
        {
            _processor.Run((Config)e.Argument, sender as BackgroundWorker, e);
            _results = _processor.AnalysisResults;
        }

        private void _runWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Canceled.Set();
            Done.Set();
        }

        private void _processorOnProgressUpdate(ProgressReport value)
        {
            OnStatusValueChaned(value);
        }
    }
}
