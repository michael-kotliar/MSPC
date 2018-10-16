﻿// Licensed to the Genometric organization (https://github.com/Genometric) under one or more agreements.
// The Genometric organization licenses this file to you under the GNU General Public License v3.0 (GPLv3).
// See the LICENSE file in the project root for more information.

using Genometric.GeUtilities.Intervals.Parsers.Model;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Genometric.MSPC.CLI.Tests")]
namespace Genometric.MSPC.CLI
{
    static class Program
    {
        public static void Main(string[] args)
        {
            string mspcCannotContinue = "\r\nMSPC cannot continue.";
            var cliOptions = new CommandLineOptions();
            try
            {
                cliOptions.Parse(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + mspcCannotContinue);
                return;
            }

            if (cliOptions.Input.Count < 2)
            {
                Console.WriteLine(string.Format(
                    "At least two samples are required; {0} is given.{1}",
                    cliOptions.Input.Count, mspcCannotContinue));
                return;
            }

            foreach (var file in cliOptions.Input)
                if (!File.Exists(file))
                {
                    Console.WriteLine(string.Format("Missing file: {0}{1}", file, mspcCannotContinue));
                    return;
                }

            var orchestrator = new Orchestrator(cliOptions.Options);

            var bedColumns = new BedColumns();
            if (cliOptions.ParserConfig != null)
            {
                var parser = new ParserConfig();
                bedColumns = parser.ParseBed(cliOptions.ParserConfig);
            }

            var et = new Stopwatch();
            foreach (var file in cliOptions.Input)
            {
                Console.WriteLine(string.Format("Parsing sample: {0}", file));
                et.Restart();

                var parsedSample = orchestrator.LoadSample(file, bedColumns);
                et.Stop();
                Console.WriteLine("Done...  ET:\t{0}", et.Elapsed.ToString());
                Console.WriteLine("Read peaks#:\t{0}", parsedSample.IntervalsCount.ToString("N0", CultureInfo.InvariantCulture));
                Console.WriteLine("Min p-value:\t{0}", string.Format("{0:E3}", parsedSample.PValueMin.Value));
                Console.WriteLine("Max p-value:\t{0}", string.Format("{0:E3}", parsedSample.PValueMax.Value));
                Console.WriteLine("");
            }

            Console.WriteLine("Analysis started ...");
            et.Restart();
            orchestrator.Run();

            Console.WriteLine("\n\rSaving results ...");
            orchestrator.Export();

            et.Stop();
            Console.WriteLine(" ");
            Console.WriteLine(string.Format("All processes successfully finished [Analysis ET: {0}]", et.Elapsed.ToString()));
            Console.WriteLine(" ");
        }
    }
}