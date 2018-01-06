﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace org.neurul.Brain.Port.Adapter.In.Http
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
