
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
        /// Método principal  del aplicativo
        /// </summary>
        public static void Main()
        {
           CreateHostBuilder().Build().Run();
        }

        /// <summary>
        /// Creación, configuración y ejecución del host
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
