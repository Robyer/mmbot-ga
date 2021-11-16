﻿using MMBotGA.dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MMBotGA
{
    internal class Backtest : IBacktest
    {
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly ApiLease _api;
        private readonly BacktestData _data;
        private readonly Func<BacktestRequest, ICollection<RunResponse>, double> _fitnessEvaluator;
        private readonly IDictionary<Api, Context> _contexts = new Dictionary<Api, Context>();

        private Minfo _minfo;

        public Backtest(ApiLease api, BacktestData data, Func<BacktestRequest, ICollection<RunResponse>, double> fitnessEvaluator = null)
        {
            _api = api;
            _data = data;
            _fitnessEvaluator = fitnessEvaluator ?? FitnessEvaluators.NpaRRR;
        }

        public async Task<double> TestAsync(BacktestRequest request)
        {
            var api = await _api.LeaseAsync();
            try
            {
                await _semaphore.WaitAsync();
                Context context = null;
                try
                {
                    if (!_contexts.TryGetValue(api, out context))
                    {
                        _contexts[api] = context = new Context(this, api);
                    }
                    if (_minfo == null)
                    {
                        _minfo = await context.GetMinfoAsync();
                    }                    
                }
                finally
                {
                    _semaphore.Release();
                }
                request.RunRequest.Minfo = _minfo;
                request.RunRequest.Balance = _data.Balance;
                return await context.TestAsync(request);
            }
            finally
            {
                _api.EndLease(api);
            }
        }

        private class Context
        {
            private readonly Backtest _backtest;
            private readonly Api _api;

            private FileIdResponse _dataset;
            private readonly SemaphoreSlim _semaphore = new(1);

            public Context(Backtest backtest, Api api)
            {
                _backtest = backtest;
                _api = api;
            }

            private async Task InitAsync()
            {
                var data = await CsvLoader.LoadAsync(_backtest._data.SourceFile, _backtest._data.Reverse);
                _dataset = await _api.UploadAsync(data);
            }

            private async Task CheckInitAsync()
            {
                try
                {
                    await _semaphore.WaitAsync();
                    if (_dataset == null)
                    {
                        await InitAsync();
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            public async Task<Minfo> GetMinfoAsync()
            {
                return await _api.GetInfoAsync(_backtest._data.Broker, _backtest._data.Pair);
            }

            public async Task<double> TestAsync(BacktestRequest request)
            {
                await CheckInitAsync();
                return await EvaluateAsync(request);
            }

            private async Task<double> EvaluateAsync(BacktestRequest request)
            {
                for (var i = 0; i < 3; i++)
                {
                    try
                    {
                        return await DoEvaluateAsync(request);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        try
                        {
                            await Task.Delay(5000);
                            await InitAsync();
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine(ee);
                        }
                    }
                }
                return 0;
            }

            private async Task<double> DoEvaluateAsync(BacktestRequest request)
            {
                request.GenTradesRequest.Source = _dataset.Id;
                var trades = await _api.GenerateTradesAsync(request.GenTradesRequest);

                request.RunRequest.Source = trades.Id;
                var response = await _api.RunAsync(request.RunRequest);

                return _backtest._fitnessEvaluator(request, response);
            }
        }
    }
}
