// <copyright file="Program.cs" company="Eppendorf AG - 2018">
// Copyright (c) Eppendorf AG - 2018. All rights reserved.
// </copyright>

namespace Eppendorf.VNCloud.StatusDataPushService
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using System.Threading;

    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
