﻿// Licensed to the Genometric organization (https://github.com/Genometric) under one or more agreements.
// The Genometric organization licenses this file to you under the GNU General Public License v3.0 (GPLv3).
// See the LICENSE file in the project root for more information.

using Genometric.GeUtilities.Intervals.Model;
using Genometric.GeUtilities.Intervals.Parsers;
using Genometric.GeUtilities.Intervals.Parsers.Model;
using Genometric.MSPC.CLI.Exporter;
using Genometric.MSPC.Core;
using Genometric.MSPC.Core.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Genometric.MSPC.CLI
{
    internal class Orchestrator
    {
        private readonly Config _options;
        private readonly Mspc _mspc;
        private readonly List<Bed<Peak>> _samples;
        private Dictionary<uint, string> _samplesDict
        {
            get
            {
                return _samples.ToDictionary(x => x.FileHashKey, x => Path.GetFileNameWithoutExtension(x.FileName));
            }
        }

        private List<Attributes> _allAttributes
        {
            get
            {
                var attributes = new List<Attributes>();
                foreach (var att in Enum.GetValues(typeof(Attributes)).Cast<Attributes>())
                    attributes.Add(att);
                return attributes;
            }
        }

        public string OutputPath { set; get; }

        internal Orchestrator(Config options)
        {
            _options = options;
            _mspc = new Mspc();
            _mspc.StatusChanged += _mspc_statusChanged;
            _samples = new List<Bed<Peak>>();
            if (OutputPath == null)
                OutputPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "session_" +
                      DateTime.Now.ToString("yyyyMMdd_HHmmssfff", CultureInfo.InvariantCulture);
        }

        public Bed<Peak> LoadSample(string fileName, ParserConfig parserConfig)
        {
            var bedParser = new BedParser(parserConfig)
            {
                PValueFormat = parserConfig.PValueFormat,
                DefaultValue = parserConfig.DefaultValue,
                DropPeakIfInvalidValue = parserConfig.DropPeakIfInvalidValue
            };
            var parsedSample = bedParser.Parse(fileName);
            _samples.Add(parsedSample);
            return parsedSample;
        }

        internal void Run()
        {
            foreach (var sample in _samples)
                _mspc.AddSample(sample.FileHashKey, sample);
            _mspc.RunAsync(_options);
            _mspc.Done.WaitOne();
        }
        private void _mspc_statusChanged(object sender, ValueEventArgs e)
        {
            Console.WriteLine("[" + e.Value.Step + "/" + e.Value.StepCount + "] " + e.Value.Message);
        }

        internal void Export(List<Attributes> attributesToExport = null)
        {
            if (attributesToExport == null)
                attributesToExport = _allAttributes;

            var exporter = new Exporter<Peak>();
            var options = new Options(
                path: OutputPath,
                includeHeader: true,
                attributesToExport: attributesToExport);

            exporter.Export(
                _samplesDict,
                _mspc.GetResults(), _mspc.GetConsensusPeaks(), options);
        }

        internal void WriteSummaryStats(List<Attributes> exportedAttributes = null)
        {
            if (exportedAttributes == null)
                exportedAttributes = _allAttributes;

            var lines = SummaryStats.Create(_samples, _samplesDict, _mspc.GetResults(), _mspc.GetConsensusPeaks(), exportedAttributes);
            SummaryStats.WriteToConsole(lines);
        }
    }
}
