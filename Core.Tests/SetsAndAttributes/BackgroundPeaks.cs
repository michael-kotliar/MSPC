﻿// Licensed to the Genometric organization (https://github.com/Genometric) under one or more agreements.
// The Genometric organization licenses this file to you under the GNU General Public License v3.0 (GPLv3).
// See the LICENSE file in the project root for more information.

using Genometric.GeUtilities.Intervals.Model;
using Genometric.GeUtilities.Intervals.Parsers.Model;
using Genometric.MSPC.Core.Model;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace Genometric.MSPC.Core.Tests.SetsAndAttributes
{
    public class BackgroundPeaks
    {
        private readonly string _chr = "chr1";
        private readonly char _strand = '*';

        private ReadOnlyDictionary<uint, Result<Peak>> GenerateAndProcessBackgroundPeaks()
        {
            var sA = new Bed<Peak>();
            sA.Add(new Peak(left: 10, right: 20, value: 1e-2, summit: 15, name: "Peak"), _chr, _strand);

            var sB = new Bed<Peak>();
            sB.Add(new Peak(left: 5, right: 12, value: 1e-4, summit: 10, name: "Peak"), _chr, _strand);

            var mspc = new MSPC<Peak>(new PeakConstructor());
            mspc.AddSample(0, sA);
            mspc.AddSample(1, sB);

            var config = new Config(ReplicateType.Biological, 1e-4, 1e-8, 1e-4, 2, 1F, MultipleIntersections.UseLowestPValue);

            // Act
            return mspc.Run(config);
        }

        [Fact]
        public void AssignBackgroundAttribute()
        {
            // Arrange & Act
            var res = GenerateAndProcessBackgroundPeaks();

            // Assert
            foreach (var s in res)
                Assert.True(s.Value.Chromosomes[_chr].Get(Attributes.Background).Count() == 1);
        }

        [Fact]
        public void BackgroundPeaksShouldNotHaveAnyOtherAttributes()
        {
            // Arrange & Act
            var res = GenerateAndProcessBackgroundPeaks();

            // Assert
            foreach (var s in res)
                Assert.True(
                    !s.Value.Chromosomes[_chr].Get(Attributes.Weak).Any() &&
                    !s.Value.Chromosomes[_chr].Get(Attributes.Stringent).Any() &&
                    !s.Value.Chromosomes[_chr].Get(Attributes.Confirmed).Any() &&
                    !s.Value.Chromosomes[_chr].Get(Attributes.Discarded).Any() &&
                    !s.Value.Chromosomes[_chr].Get(Attributes.TruePositive).Any() &&
                    !s.Value.Chromosomes[_chr].Get(Attributes.FalsePositive).Any());
        }

        [Fact]
        public void NonOverlappingBackgroundPeaks()
        {
            // Arrange
            var sA = new Bed<Peak>();
            sA.Add(new Peak(left: 10, right: 20, value: 1e-2, summit: 15, name: "Peak"), _chr, _strand);

            var sB = new Bed<Peak>();
            sB.Add(new Peak(left: 50, right: 60, value: 1e-4, summit: 55, name: "Peak"), _chr, _strand);

            var mspc = new MSPC<Peak>(new PeakConstructor());
            mspc.AddSample(0, sA);
            mspc.AddSample(1, sB);

            var config = new Config(ReplicateType.Biological, 1e-4, 1e-8, 1e-4, 2, 1F, MultipleIntersections.UseLowestPValue);

            // Act
            var res = mspc.Run(config);

            foreach (var s in res)
                Assert.True(s.Value.Chromosomes[_chr].Get(Attributes.Background).Count() == 1);
        }

        [Fact]
        public void BackgroundOverlappingNonBackground()
        {
            // Arrange
            var sA = new Bed<Peak>();
            sA.Add(new Peak(left: 10, right: 20, value: 1e-2, summit: 15, name: "Peak"), _chr, _strand);

            var sB = new Bed<Peak>();
            sB.Add(new Peak(left: 50, right: 60, value: 1e-8, summit: 55, name: "Peak"), _chr, _strand);

            var mspc = new MSPC<Peak>(new PeakConstructor());
            mspc.AddSample(0, sA);
            mspc.AddSample(1, sB);

            var config = new Config(ReplicateType.Biological, 1e-4, 1e-8, 1e-4, 2, 1F, MultipleIntersections.UseLowestPValue);

            // Act
            var res = mspc.Run(config);

            Assert.True(
                res[0].Chromosomes[_chr].Get(Attributes.Background).Count() == 1 &&
                res[1].Chromosomes[_chr].Get(Attributes.Background).Count() == 0);
        }

        [Fact]
        public void ProcessedBackgroundPeakEqualsInput()
        {
            // Arrange
            var sA = new Bed<Peak>();
            var sAP = new Peak(left: 10, right: 20, value: 1e-2, summit: 15, name: "Peak");
            sA.Add(sAP, _chr, _strand);

            var sB = new Bed<Peak>();
            var sBP = new Peak(left: 50, right: 60, value: 1e-4, summit: 55, name: "Peak");
            sB.Add(sBP, _chr, _strand);

            var mspc = new MSPC<Peak>(new PeakConstructor());
            mspc.AddSample(0, sA);
            mspc.AddSample(1, sB);

            var config = new Config(ReplicateType.Biological, 1e-4, 1e-8, 1e-4, 2, 1F, MultipleIntersections.UseLowestPValue);

            // Act
            var res = mspc.Run(config);

            // Assert

            Assert.True(
                res[0].Chromosomes[_chr].Get(Attributes.Background).ToList()[0].Source.Equals(sAP) &&
                res[1].Chromosomes[_chr].Get(Attributes.Background).ToList()[0].Source.Equals(sBP));
        }
    }
}
