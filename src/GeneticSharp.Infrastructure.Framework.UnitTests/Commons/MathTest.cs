﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Infrastructure.Framework.Commons;
using NUnit.Framework;

namespace GeneticSharp.Infrastructure.Framework.UnitTests.Commons
{
    [TestFixture]
    public class MathTest
    {

        [Test]
        public void PositiveMod_PerformsAsExpected()
        {
            var negInt = -1;
            var positiveMod = negInt.PositiveMod(5);
            var wrongMod = negInt % 5;

            Assert.AreEqual(4, positiveMod);
            Assert.AreNotEqual(4, wrongMod);

            positiveMod = (-1.0).PositiveMod(5);
            Assert.AreEqual(4, positiveMod);

        }

        [Test]
        public void Compare_IntPow_Pow_Faster()
        {
            var ratio = 1.0;
            var repeatNb = 5;
            var maxApprox = Math.Pow(10, -10);

            var nbNumbers = (int) 10.IntPow(5);
            var maxPower = 500;
            var rnd = RandomizationProvider.Current;
            //Picking numbers between -10000 and 10000
            var rndNumbers = Enumerable.Range(0, nbNumbers).Select(i => (rnd.GetDouble(0, 1) - 1) * 20000).ToList();

            //Picking int exponents between 2 and 500
            for (int i = 0; i < maxPower; i+=50)
            {
                TimeSpan regularElapsed = TimeSpan.Zero;
                TimeSpan optimizedElapsed = TimeSpan.Zero;
                for (int r = 0; r < repeatNb; r++)
                {
                    var regularResults = new List<double>(rndNumbers.Count);
                    var sw = Stopwatch.StartNew();
                    foreach (var rndNumber in rndNumbers)
                    {
                        regularResults.Add(Math.Pow(rndNumber, i + 2));
                    }
                    regularElapsed += sw.Elapsed;
                    var optimizedResults = new List<double>(rndNumbers.Count);
                    sw.Restart();
                    foreach (var rndNumber in rndNumbers)
                    {
                        optimizedResults.Add(rndNumber.IntPow(i + 2));
                    }
                    optimizedElapsed += sw.Elapsed;
                    for (int rndIndex = 0; rndIndex < rndNumbers.Count; rndIndex++)
                    {
                        Assert.LessOrEqual(Math.Abs(regularResults[rndIndex] - optimizedResults[rndIndex])/ Math.Max(Math.Abs(regularResults[rndIndex]), double.Epsilon), maxApprox);
                    }
                }

                Assert.Greater(TimeSpan.FromTicks((long)(regularElapsed.Ticks * ratio)), optimizedElapsed  , $"failed at i = {i}");
            }
           
        }

    }
}