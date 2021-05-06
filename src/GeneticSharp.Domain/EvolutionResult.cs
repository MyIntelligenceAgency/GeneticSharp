﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Infrastructure.Framework.Collections;

namespace GeneticSharp.Domain
{
    /// <summary>
    /// The IEvolutionResult interface represent the result of a genetic algorithm evolution. It aggregates the evolution time and the resulting population.
    /// </summary>
    public interface IEvolutionResult
    {
        double Fitness { get; }
        IPopulation Population { get; }
        TimeSpan TimeEvolving { get;  }

        int GenerationsNumber { get; }

    }

    /// <summary>
    /// The EvolutionResult class represent the result of a genetic algorithm evolution. It aggregates the evolution time and the resulting population.
    /// </summary>
    [DebuggerDisplay("Fit:{Fitness}, Time:{TimeEvolvingDisplay}, GenNb: {GenerationsNumber}")]
    public class EvolutionResult : IEvolutionResult
    {

        public double Fitness => Population.BestChromosome.Fitness.Value;

        public IPopulation Population { get; set; }

        public string TimeEvolvingDisplay => TimeEvolving.ToString();

        public TimeSpan TimeEvolving { get; set; }
        public int GenerationsNumber => Population.GenerationsNumber;
    }

    /// <summary>
    /// The MeanEvolutionResult class allows collecting repeated evolution results and computing mean statistical values, while skipping a percentage of extrema when Results are sorted according to a custom comparer.
    /// </summary>
    [DebuggerDisplay("Fit:{Fitness}  -  {TestSettings}, Time:{TimeEvolvingDisplay}, GenNb:{GenerationsNumber}")]
    public class MeanEvolutionResult : IEvolutionResult
    {
        private SortedSet<IEvolutionResult> _results;

        public object TestSettings { get; set; }

        public Func<IEvolutionResult, IEvolutionResult, int> ResultComparer { get; set; } =
            (result1, result2) => Math.Sign(result1.Fitness - result2.Fitness);

        public double SkipExtremaPercentage { get; set; } = 0.1;

        public SortedSet<IEvolutionResult> Results
        {
            get
            {
                if (_results == null)
                {
                    _results = new SortedSet<IEvolutionResult>(new DynamicComparer<IEvolutionResult>(ResultComparer));
                }
                return _results;
            }
        }

        public double Fitness => Results.Count > 0 ? GetScopedResults().Sum(r => r.Fitness) / GetScopedResults().Count() : 0;

        public IPopulation Population => Results.Count > 0 ? GetScopedResults().First().Population : null;

        public TimeSpan TimeEvolving => Results.Count > 0 ? TimeSpan.FromTicks(GetScopedResults().Sum(r => r.TimeEvolving.Ticks) / GetScopedResults().Count()) : TimeSpan.Zero;

        public string TimeEvolvingDisplay => TimeEvolving.ToString();
        public int GenerationsNumber => Results.Count > 0 ? GetScopedResults().Sum(r => r.Population.GenerationsNumber) / GetScopedResults().Count() : 0;


        protected IEnumerable<IEvolutionResult> GetScopedResults()
        {
            var skipNb = Convert.ToInt32( Math.Floor(Results.Count * SkipExtremaPercentage));
            return Results.Skip(skipNb).Take(Results.Count - 2 * skipNb);
        }


    }


}