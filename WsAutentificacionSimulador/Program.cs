
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace WsAutentificacionSimulador
{
    /// <summary>
    /// Clase principal del aplicativo
    /// </summary>
    public class Program
    {
        protected Program() { }

        /// <summary>
        /// M�todo principal  del aplicativo
        /// </summary>
        public static void Main()
        {
           CreateHostBuilder().Build().Run();
        }

        /// <summary>
        /// Creaci�n, configuraci�n y ejecuci�n del host
        /// </summary>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder() =>
             Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
