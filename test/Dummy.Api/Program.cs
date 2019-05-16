namespace Dummy.Api
{
    using System.IO;
    using System.Net;
    using Microsoft.AspNetCore.Hosting;

    public class Program
    {
        public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
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
                .UseStartup<Startup>();
    }
}
