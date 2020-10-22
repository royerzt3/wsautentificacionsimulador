using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using System;

namespace WsAutentificacionSimuladorTest
{
    [TestClass]
    public class AutentificacionSimuladorControllerTest
    {
        private readonly IConfiguration _configuration;
        readonly WsAutentificacionSimulador.Controllers.AutentificacionSimuladorController ws;
        public AutentificacionSimuladorControllerTest()
        {
            ws = new WsAutentificacionSimulador.Controllers.AutentificacionSimuladorController(_configuration);
        }


        [TestMethod]
        public void TestValidaUsuario()
        {     
            string empleado = "316064";
            string empresa = "EKT";
            Assert.IsNotNull(ws.ValidaUsuarioAsync(empleado, empresa));
        }


        [TestMethod]
        public void TestObtenerUsuarios()
        {
            Assert.IsNotNull(ws.ObtenerUsuarios());
        }

        [TestMethod]
        public void TestGetFoto()
        {
            string empleado = "316064";
            string empresa = "EKT";
            string urlfoto = "https://botonrojo.socio.gs/homebr/imgs/icon1.png";
            Console.Write(ws.GetFoto(empleado, empresa));
            Console.Write(urlfoto);
            try
            {
                Assert.AreSame(urlfoto, ws.GetFoto(empleado, empresa).Result, "Es diferente");
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

        }
    }
}
