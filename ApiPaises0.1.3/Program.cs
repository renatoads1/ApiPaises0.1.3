using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace ApiPaises013
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //webBuilder.ConfigureKestrel(serverOptions =>
                    //{
                    //    serverOptions.Listen(IPAddress.Loopback, 5002);
                    //}).UseStartup<Startup>();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
