﻿using MMBotGA.dto;

namespace MMBotGA
{
    internal static class StrategyChromosomeExtensions
    {
        public static BacktestRequest ToBacktestRequest(this StrategyChromosome chromosome)
        {
            const bool sliding = false;
            const bool freeze = true;
            const bool dynMult = true;

            return new BacktestRequest
            {
                GenTradesRequest = new GenTradesRequest
                {
                    Sma = chromosome.Sma,
                    Stdev = chromosome.Stdev,
                    ForceSpread = 0,
                    Mult = chromosome.Mult,
                    Raise = chromosome.Raise,
                    Cap = chromosome.Cap,
                    Fall = chromosome.Fall,
                    Mode = chromosome.Mode,
                    Sliding = sliding,
                    SpreadFreeze = freeze,
                    DynMult = dynMult,
                    Reverse = false,
                    Invert = false,
                    Ifutures = false,
                    // Order2 = 50
                },
                RunRequest = new RunRequest
                {
                    // StartDate = "1.9.2021",
                    FillAtprice = true,
                    Reverse = false,
                    Invert = false,
                    NegBal = false,
                    InitPrice = 0,
                    Spend = false,
                    Config = new Config
                    {
                        Strategy = new Strategy
                        {
                            Type = "gamma",
                            Function = "keepvalue",
                            Trend = chromosome.Trend,
                            Reinvest = true,

                            Exponent = chromosome.Exponent,
                            Rebalance = chromosome.Rebalance.ToString()
                        },
                        Enabled = true,
                        AdjTimeout = 5,

                        SpreadCalcSmaHours = chromosome.Sma,
                        SpreadCalcStdevHours = chromosome.Stdev,
                        DynmultMode = chromosome.Mode,
                        DynmultSliding = sliding,
                        SpreadFreeze = freeze,
                        DynmultMult = dynMult,
                        DynmultRaise = chromosome.Raise,
                        DynmultFall = chromosome.Fall,
                        DynmultCap = chromosome.Cap,
                        SellStepMult = chromosome.Mult,
                        BuyStepMult = chromosome.Mult,
                        //SecondaryOrder = 50,
                    }
                }
            };
        }
    }
}
