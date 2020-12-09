﻿using System;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Domain.Selections;

namespace GeneticSharp.Domain.Metaheuristics
{


    public enum MatchingTechnique
    {
        Neighbor,
        Randomize,
        RouletteWheel,
        Best
    }


    /// <summary>
    /// The matching MetaHeuristic offers various techniques to match specific parents for mating and applies the crossover operator to them
    /// </summary>
    public class MatchMetaHeuristic : ContainerMetaHeuristic
    {

        /// <summary>
        /// This internal class serves exposing methods to generate a random wheel from population and relative individual fitnesses
        /// </summary>
        private abstract class ReuseRouletteWheelSelection : RouletteWheelSelection
        {

            public new static IList<IChromosome> SelectFromWheel(int number, IList<IChromosome> chromosomes,
                IList<double> rouletteWheel, Func<double> getPointer)
            {
                return RouletteWheelSelection.SelectFromWheel(number, chromosomes, rouletteWheel, getPointer);
            }

            public new static void CalculateCumulativePercentFitness(IList<IChromosome> chromosomes,
                IList<double> rouletteWheel)
            {
                RouletteWheelSelection.CalculateCumulativePercentFitness(chromosomes, rouletteWheel);
            }

        }

        public int NumberOfMatches { get; set; } = 1;

        public MatchMetaHeuristic() : this (1) { }

        public MatchMetaHeuristic(int numberOfMatches) : this(new DefaultMetaHeuristic(), numberOfMatches) { }

        public MatchMetaHeuristic(IMetaHeuristic subMetaHeuristic, int numberOfMatches) : base(subMetaHeuristic)
        {
            NumberOfMatches = numberOfMatches;
        }

        public ParamScope RouletteCachingScope { get; set; } = ParamScope.Generation | ParamScope.MetaHeuristic;

        public List<MatchingTechnique> MatchingTechniques { get; set; }

        public override IList<IChromosome> MatchParentsAndCross(IEvolutionContext ctx, ICrossover crossover, float crossoverProbability, IList<IChromosome> parents)
        {

            if (ShouldRun(crossoverProbability, CrossoverProbabilityStrategy,StaticCrossoverProbability, out var subProbability))
            {
                var toReturn = new List<IChromosome>(NumberOfMatches * crossover.ChildrenNumber);

                for (int matchIndex = 0; matchIndex < NumberOfMatches; matchIndex++)
                {
                    var firstParent = parents[ctx.Index + matchIndex];

                    var selectedParents = new List<IChromosome>(crossover.ParentsNumber) { firstParent };
                    for (int i = 0; i < crossover.ParentsNumber - 1; i++)
                    {
                        var currentMatchingProcess = MatchingTechniques[i];
                        switch (currentMatchingProcess)
                        {
                            case MatchingTechnique.Neighbor:
                                var newParentIdx = ctx.Index + i;
                                if (newParentIdx < parents.Count)
                                {
                                    selectedParents.Add(parents[newParentIdx]);
                                }
                                break;
                            case MatchingTechnique.Randomize:
                                var targetIdx = RandomizationProvider.Current.GetInt(0, parents.Count);
                                selectedParents.Add(parents[targetIdx]);
                                break;
                            case MatchingTechnique.RouletteWheel:
                                var dynamicRouletteParameter = new MetaHeuristicParameter<IList<double>>()
                                {
                                    Scope = RouletteCachingScope,
                                    Generator = (h, c) => {
                                        var tempRoulette = new List<double>(parents.Count);
                                        ReuseRouletteWheelSelection.CalculateCumulativePercentFitness(parents, tempRoulette);
                                        return tempRoulette;
                                    }
                                };
                                var currentRoulette = dynamicRouletteParameter.GetOrAdd(this, ctx, "currentRouletteWheel");
                                selectedParents.Add(ReuseRouletteWheelSelection.SelectFromWheel(1, parents, currentRoulette,
                                    () => RandomizationProvider.Current.GetDouble())[0]);
                                break;
                            case MatchingTechnique.Best:
                                selectedParents.Add(ctx.Population.CurrentGeneration.BestChromosome);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    var subContext = ctx.GetIndividual(0);
                    var matchResult = base.MatchParentsAndCross(subContext, crossover, subProbability, selectedParents);
                    if (matchResult != null)
                    {
                        toReturn.AddRange(matchResult);
                    }
                }

                return toReturn;


            }

            return null;

        }
      
    }
}