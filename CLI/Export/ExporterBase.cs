﻿// Licensed to the Genometric organization (https://github.com/Genometric) under one or more agreements.
// The Genometric organization licenses this file to you under the GNU General Public License v3.0 (GPLv3).
// See the LICENSE file in the project root for more information.

using Genometric.MSPC.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Genometric.GeUtilities.IGenomics;
using Genometric.MSPC.Core.Model;

namespace Genometric.MSPC.CLI.Exporter
{
    public class ExporterBase<P>
        where P : IChIPSeqPeak, new()
    {
        public string samplePath { set; get; }
        protected string sessionPath { set; get; }
        protected bool includeBEDHeader { set; get; }
        protected Result<P> data { set; get; }
        protected ReadOnlyDictionary<string, SortedList<P, P>> mergedReplicates { set; get; }

        private readonly string _header = "chr\tstart\tstop\tname\t-1xlog10(p-value)\txSqrd\t-1xlog10(Right-Tail Probability)";

        protected void Export(Attributes attribute)
        {
            string fileName = samplePath + Path.DirectorySeparatorChar + attribute.ToString() + ".bed";
            using (File.Create(fileName))
            using (StreamWriter writter = new StreamWriter(fileName))
            {
                if (includeBEDHeader)
                    writter.WriteLine(_header);

                foreach (var chr in data.Chromosomes)
                {
                    var sortedDictionary = from entry in chr.Value.Get(attribute) orderby entry ascending select entry;

                    foreach (var item in sortedDictionary)
                    {
                        writter.WriteLine(
                            chr.Key + "\t" +
                            item.Source.Left.ToString() + "\t" +
                            item.Source.Right.ToString() + "\t" +
                            item.Source.Name + "\t" +
                            ConvertPValue(item.Source.Value) + "\t" +
                            Math.Round(item.XSquared, 3) + "\t" +
                            ConvertPValue(item.RTP));
                    }
                }
            }
        }
        
        protected void ExportConsensusPeaks()
        {
            using (File.Create(sessionPath + Path.DirectorySeparatorChar + "MergedReplicates.bed")) { }
            using (StreamWriter writter = new StreamWriter(sessionPath + Path.DirectorySeparatorChar + "MergedReplicates.bed"))
            {
                if (includeBEDHeader)
                    writter.WriteLine("chr\tstart\tstop\tname\tX-squared");

                foreach (var chr in mergedReplicates)
                {
                    foreach (var item in chr.Value)
                    {
                        writter.WriteLine(
                            chr.Key + "\t" +
                            item.Value.Left.ToString() + "\t" +
                            item.Value.Right.ToString() + "\t" +
                            item.Value.Name + "\t" +
                            Math.Round(item.Value.Value, 3));
                    }
                }
            }
        }

        private string ConvertPValue(double pValue)
        {
            if (pValue != 0)
                return (Math.Round((-1) * Math.Log10(pValue), 3)).ToString();
            return "0";
        }
    }
}
