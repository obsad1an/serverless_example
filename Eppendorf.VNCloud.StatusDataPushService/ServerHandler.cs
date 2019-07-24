// <copyright file="ServerHandler.cs" company="Eppendorf AG - 2018">
// Copyright (c) Eppendorf AG - 2018. All rights reserved.
// </copyright>

using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Eppendorf.VNCloud.StatusDataPushService
{
    public class ServerHandler
    {
        private static readonly HttpClient Client = new HttpClient();

        private readonly string _serverName;

        private readonly ServiceUtils _serviceUtils;

        private readonly string _hubName, _userId;

        private readonly string _endpoint;

        private readonly PayloadMessage _defaultPayloadMessage;

        private int count = 0;

        public ServerHandler(string connectionString, string hubName, string userId)
        {
            _userId = userId;
            _serverName = GenerateServerName();
            _serviceUtils = new ServiceUtils(connectionString);
            _hubName = hubName;
            _endpoint = _serviceUtils.Endpoint;

            _defaultPayloadMessage = new PayloadMessage
            {
                Target = "SendMessage",
                Arguments = new[]
                {
                    _serverName,
                    "Hello from server",
                }
            };
        }

        public async Task Start()
        {
            ShowHelp();
            while (true)
            {
                var argLine = "send group groupname";
                if (argLine == null)
                {
                    continue;
                }
                var args = argLine.Split(' ');

                if (args.Length == 1 && args[0].Equals("broadcast"))
                {
                    await SendRequest(args[0], _hubName);
                }
                else if (args.Length == 3 && args[0].Equals("send"))
                {
                    await SendRequest(args[1], _hubName, args[2]);
                }
                else
                {
                    Console.WriteLine($"Can't recognize command {argLine}");
                }
            }
        }

        public async Task SendRequest(string command, string hubName, string arg = null)
        {
            string url = null;
            switch (command)
            {
                case "user":
                    url = GetSendToUserUrl(hubName, arg);
                    break;
                case "group":
                    url = GetSendToGroupUrl(hubName, arg);
                    break;
                case "broadcast":
                    url = GetBroadcastUrl(hubName);
                    break;
                default:
                    Console.WriteLine($"Can't recognize command {command}");
                    break;
            }

            if (!string.IsNullOrEmpty(url))
            {
                var request = BuildRequest(url);
                Thread.Sleep(3000);

                if (count == 0)
                {
                    request = BuildDeleteRequest("https://test2-vncloud.service.signalr.net/api/v1/hubs/chathub/groups/groupname/users/user2");
                }

                if (count == 7)
                {
                    request = BuildPutRequest("https://test2-vncloud.service.signalr.net/api/v1/hubs/chathub/groups/groupname/users/user2");
                }

                count++;

                // ResponseHeadersRead instructs SendAsync to return once headers are read
                // rather than buffer the entire response. This gives a small perf boost.
                // Note that it is important to dispose of the response when doing this to
                // avoid leaving the connection open.
                using (var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (response.StatusCode != HttpStatusCode.Accepted)
                    {
                        Console.WriteLine($"Sent error: {response.StatusCode}");
                    }
                }
            }
        }

        private Uri GetUrl(string baseUrl)
        {
            return new UriBuilder(baseUrl).Uri;
        }

        private string GetSendToUserUrl(string hubName, string userId)
        {
            return $"{GetBaseUrl(hubName)}/users/{userId}";
        }

        private string GetSendToGroupUrl(string hubName, string group)
        {
            return $"{GetBaseUrl(hubName)}/groups/{group}";
        }

        private string GetBroadcastUrl(string hubName)
        {
            return $"{GetBaseUrl(hubName)}";
        }

        private string GetBaseUrl(string hubName)
        {
            return $"{_endpoint}/api/v1/hubs/{hubName.ToLower()}";
        }

        private string GenerateServerName()
        {
            return $"{Environment.MachineName}_{Guid.NewGuid():N}";
        }

        private HttpRequestMessage BuildRequest(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, GetUrl(url));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _serviceUtils.GenerateAccessToken(url, _serverName));
            
            url = GetClientUrl(url, _hubName);

            var client = _serviceUtils.GenerateAccessToken(url, _userId);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonConvert.SerializeObject(_defaultPayloadMessage), Encoding.UTF8, "application/json");

            return request;
        }

        private HttpRequestMessage BuildPutRequest(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, GetUrl(url));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _serviceUtils.GenerateAccessToken(url, _serverName));
            
            url = GetClientUrl(url, _hubName);

            var client = _serviceUtils.GenerateAccessToken(url, _userId);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //request.Content = new StringContent(JsonConvert.SerializeObject(_defaultPayloadMessage), Encoding.UTF8, "application/json");
            return request;
        }

        private HttpRequestMessage BuildDeleteRequest(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, GetUrl(url));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _serviceUtils.GenerateAccessToken(url, _serverName));
            
            url = GetClientUrl(url, _hubName);

            var client = _serviceUtils.GenerateAccessToken(url, _userId);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //request.Content = new StringContent(JsonConvert.SerializeObject(_defaultPayloadMessage), Encoding.UTF8, "application/json");
            return request;
        }

        private void ShowHelp()
        {
            Console.WriteLine("*********Usage*********\n" +
                              "send user <User Id>\n" +
                              "send group <Group Name>\n" +
                              "broadcast\n" +
                              "***********************");
        }

        private string GetClientUrl(string endpoint, string hubName)
        {
            return $"{endpoint}/client/?hub={hubName}";
        }
    }

    public class PayloadMessage
    {
        public string Target { get; set; }

        public object[] Arguments { get; set; }
    }
}
