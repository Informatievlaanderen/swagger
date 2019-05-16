namespace Dummy.Api
{
    using System.IO;
    using System.Net;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;

    public class Program
    {
        public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

        public static IWebHostBuilder CreateWebHostBuilder(string[] _)
            => new WebHostBuilder()
                .UseKestrel(x =>
                {
                    x.Listen(
                        new IPEndPoint(IPAddress.Loopback, 1337),
                        listenOptions => listenOptions.UseConnectionLogging());
                })
                .UseSockets()
                .CaptureStartupErrors(true)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseWebRoot("wwwroot")
                .ConfigureLogging(x =>
                {
                    //x.SetMinimumLevel(LogLevel.Debug);
                    x.SetMinimumLevel(LogLevel.Information);
                    x.AddConsole();
                })
                .UseStartup<Startup>();
    }
}
