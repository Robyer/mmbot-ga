﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using log4net;
using MMBot.Api;
using MMBotGA.api.endpoints;

namespace MMBotGA.api
{
    internal static class ApiDefinitions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ApiDefinitions));

        private static IEnumerable<ApiEndpoint> Endpoints => new JsonConfiguredApiEndpointProvider().GetApiEndpoints();

        public static ApiLease GetLease()
        {
            return new ApiLease(Endpoints
                .Select(x => CreateBackend(x.LeaseCount, x.UseNative, x.Url, x.Username, x.Password))
                .ToArray()
            );
        }

        private static LeasableApi CreateBackend(int leaseCount, bool useNative, string url, string username, string password)
        {
            HttpMessageHandler handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(username, password),
                PreAuthenticate = true,
                MaxConnectionsPerServer = 20
            };
            if (Log.IsDebugEnabled)
            {
                handler = new LoggingHandler(handler);
            }

            IMMBotApi api = new Api(url, new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(60)
            });

            if (useNative)
            {
                api = new MMBotApiProxy(api);
            }
            return new LeasableApi(leaseCount, api);
        }
    }
}