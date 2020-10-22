/// <summary>
/// Responsable: Yesenia Murillo León
/// Fecha: Noviembre 2019
/// Proyecto: Sistema de Mercadeo de Tasas
/// Resumen: Servicio creado para el login y administración de usuarios del Sistema.
/// 
/// Método 1. Login: Este método obtiene la URL que se utiliza para el acceso por Llave Maestra, la URL
/// se conforma de un client ID, Client Secret, redirect uri, scope y acr_values. 
/// 
/// Método 2. getCode: Obtiene el código que genera Llave Maestra una vez que el usuario se ha logeado. 
/// 
/// Una vez que se valida el código se obtiene al access_token para el usuario. 
/// 
/// Método 3. validaUsuario: Se utiliza para validar si el usuario se encuentra activo en la BD, si está activo devuelve toda la información del usuario. 
/// 
/// Método 4. altaUsuario: Se utiliza para agregar un nuevo usuario a la BD.
/// 
/// Método 5. actualizarUsuario: Actualiza los datos del usuario que se encuentra previamente registrado en la DB y genera un folio DataSec para actualizar el usr en CYAAL. 
/// 
/// Método 6. getUsuarios: Obtiene todos los usuarios con status activos de la BD. 
/// 
/// Método 7: bajaUsuario: Se utiliza para cambiar el status de un usuario de activo a inactivo en la BD. 
/// 
/// Método 8: getFoto: Obtiene la url de la foto de un usuario en particular consumiedo un servicio del área de Sistemas Internos.
/// 
/// Método 9: getEstatusFolio: Valida el status de un folio de DataSec en particular
/// 
/// Método 10: ObtieneDatosCliente: Guarda los datos de conexión del cliente. 
/// </summary>

using BibliotecaSimulador.AutentificacionSimulador;
using BibliotecaSimulador.Pojos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using BibliotecaSimulador.Negocio;
using Microsoft.AspNetCore.Http;
using BibliotecaSimulador.DefiniedExceptions;
using BibliotecaSimulador.ClasesAuxiliares;
using System.Globalization;
using Newtonsoft.Json;
using System.Collections.Generic;

[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: CLSCompliant(false)]
namespace WsAutentificacionSimulador.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AutentificacionSimuladorController : ControllerBase
    {

        /// <summary>
        /// Instancia el DAO para consumir los métodos de usuarios usuarioDao
        /// Instancia la clase de logs BibliotecaSimulador.Logs.Logg _log
        /// Instancia la interfaz de Configuration
        /// </summary>
        readonly DatosUsario usuarioDao = new DatosUsario();
        private readonly BibliotecaSimulador.Logs.Logg _log;
        private IConfiguration Configuration { get; set; }
        readonly string ambiente;
        /// <summary>
        /// Inicializa la varible de Log y configuración 
        /// </summary>
        /// <param name="configuration"></param>
        public AutentificacionSimuladorController(IConfiguration configuration)
        {
            this.Configuration = configuration;
            this._log = new BibliotecaSimulador.Logs.Logg("Usuarios");
            this.ambiente = this.Configuration.GetValue<string>("Config:Ambiente");
        }

        [HttpGet("Test")]
        public string Credits()
        {
            Dictionary<string, string> creditos = new Dictionary<string, string>();

            creditos.Add("Tipo proyecto", "Servicio Web");
            creditos.Add("Nombre", "WSAutentificacion");
            creditos.Add("Version Net Core", "3.1");
            creditos.Add("Area", "Credito");
            creditos.Add("Servidor Activo", "OK");
            creditos.Add("Version", "3.1");
            creditos.Add("Cambio", "Metodos de Prueba con respuesta Json");
            return System.Text.Json.JsonSerializer.Serialize(creditos);
        }

        /// <summary>
        /// Método para obtener URL y levantar Login.
        /// Área responsable del servicio: Llave Maestra
        /// </summary>
        [HttpGet("Login")]
        public void UrlLogin()
        {
            string URL_LlaveM;
            string urlRedirect = this.Configuration.GetValue<string>("Config:URLRedirect");
            string IP = Request.Host.ToString();
            BibliotecaSimulador.Logs.PintarLog.PintaInformacion("WsAutentificacion - Login. Ambiente: " + ambiente, _log);
            try
            {
                //Obtiene URL de Login de LLave Maestra 
                URL_LlaveM = usuarioDao.LoginUsuario();
                //Obtiene los datos de conexión del cliente
                ObtieneDatosCliente("Login", "Obtiene url Llave Maestra: " + URL_LlaveM);
                Response.Redirect(URL_LlaveM);
            }
            catch (Exception e)
            {
                BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                Response.Redirect(urlRedirect);
            }
        }


        /// <summary>
        /// Obtiene el code de Llave Maestra una vez que el Usuario se Logea correctamente
        /// Llave Maestra redirecciona a este método para continuar el flujo
        /// Se valida el Code y se obtiene el token el cual se envía a la aplicación 
        /// Para obtener los datos del usuario logeado. 
        /// </summary>
        /// <param name="code">código recibido por Llave Maestra</param>
        /// <returns>void</returns>
        [HttpGet("getCode")]
        public async System.Threading.Tasks.Task ValidaCodeAsync(string code)
        {
            string llave;
            string url;
            Stopwatch time = new Stopwatch();
            try
            {
                //Obtiene url del Simulador Abonos
                url = (ambiente == "P") ? this.Configuration.GetValue<string>("Config:URLApp_P") : this.Configuration.GetValue<string>("Config:URLApp_D");
                BibliotecaSimulador.Logs.PintarLog.PintaInformacion("WsAutentificacion - getCode. Ambiente: " + ambiente, _log);
                //Obtiene los datos de conexión del cliente
                ObtieneDatosCliente("getCode", null);
                time.Start();
                //Obtiene AccessToken con base en el code recibido
                llave = await usuarioDao.ValidaCodeAsync(code);
                time.Stop();
                BibliotecaSimulador.Logs.PintarLog.PintaInformacionContador("Obtiene token", _log, time);
                //url = "http://localhost:14575/?";
                //Redirecciona al aplicativo Simulador Abonos
                Response.Redirect(url + "fcAccessToken=" + llave);
            }

            catch (Exception e)
            {
                BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
            }
        }

        /// <summary>
        /// Valida el usuario que accesa al sistema exista en BD  
        /// Si no existe no permite accesar al sistema 
        /// </summary>
        /// <param name="fcEmpleado">Id del Empleado</param>
        /// <param name="fcEmpresa">Empresa a la que pertenece el Empleado</param>
        /// <returns>Información general del usuario</returns>
        [HttpGet("validaUsuario")]
        public async System.Threading.Tasks.Task<IActionResult> ValidaUsuarioAsync(string fcEmpleado, string fcEmpresa)
        {
            InfoUsuario dtoUsr;
            try
            {
                //Obtiene los datos de conexión del cliente
                ObtieneDatosCliente("validaUsuario", null);
                //Obtiene los datos del usuario 
                dtoUsr = await usuarioDao.ValidaUsuarioAsync(fcEmpleado, fcEmpresa);
                if (dtoUsr != null)
                {
                    BibliotecaSimulador.Logs.PintarLog.PintaInformacion("Obtiene información: " + JsonConvert.SerializeObject(dtoUsr), _log);
                    //Retorna el obj con toda la información del usuario 
                    return Ok(new RespuestaOK { respuesta = dtoUsr });
                }
                else
                {
                    return BadRequest(new RespuestaError400 { errorMessage = this.Configuration.GetSection("msjErrorValidaUsr").Value });
                }
            }
            catch (Exception e)
            {

                BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                return StatusCode(StatusCodes.Status500InternalServerError, new RespuestaError { errorInfo = "Error al validar usuario " + e.Message });
            }

        }


        /// <summary>
        /// Alta de usuario en BD 
        /// En caso de que el usuario ya exista solo se realiza una actualización con base en los datos recibidos
        /// </summary>
        /// <param name="lstUsuarioFamilia">Información requerida para el alta del usuario en DB</param>
        /// <returns></returns>
        [HttpPost("altaUsuario")]
        public IActionResult AltaUsuario(InfoUsrFamilia lstUsuarioFamilia)
        {
            try
            {
                //Obtiene los datos de conexión del cliente
                ObtieneDatosCliente("altaUsuario", null);
                //Se ejecuta método en BD  
                bool respuestaSQL = usuarioDao.AltaUsuario(lstUsuarioFamilia);
                if (!respuestaSQL)
                {
                    BibliotecaSimulador.Logs.PintarLog.PintaInformacion("RespuestaBD: " + respuestaSQL + this.Configuration.GetSection("msjErrorAlta").Value, _log);
                    return BadRequest(new RespuestaError400 { errorMessage = this.Configuration.GetSection("msjErrorAlta").Value });
                }
                else
                {
                    return Ok(new RespuestaOK { respuesta = "Usuario registrado correctamente" });
                }
            }
            catch (Exception e)
            {
                BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                return StatusCode(StatusCodes.Status500InternalServerError, new RespuestaError { errorInfo = e.Message });
            }
        }


        /// <summary>
        /// Actualización de Usuario generando solicitud de DataSec por medio de un servicio
        /// Área responsable del servicio: CYAAL
        /// </summary>
        /// <param name="lstUsuarioFamilia">Información requerida para la modificación del usuario en DB</param>
        /// <returns></returns>
        [HttpPost("actualizarUsuario")]
        public IActionResult ActualizarUsuario(InfoUsrFamilia lstUsuarioFamilia)
        {
            string msjError = this.Configuration.GetSection("msjErrorAct").Value;
            try
            {
                //Obtiene los datos de conexión del cliente
                ObtieneDatosCliente("actualizarUsuario", null);
                //Se ejecuta método en BD  
                bool respuestaSQL = usuarioDao.ActualizarUsuario(out int FolioProceso, lstUsuarioFamilia);
                if (!respuestaSQL)
                {
                    this._log.WriteInfo("RespuestaBD: " + respuestaSQL + msjError);
                    return BadRequest(new RespuestaError400 { errorMessage = msjError });
                }
                else
                {

                    BibliotecaSimulador.Logs.PintarLog.PintaInformacion("Folio Proceso Actualización: " + FolioProceso + " " + this.Configuration.GetSection("msjActualización").Value, _log);
                    //Genera la instacia para actualizar el usuario enviando como parámetro: 
                    //El objeto lstUsuarioFamilia que contien la info del usuario que se va modificar. 
                    ActualizacionUsuarioDsi actualizacionUsuarioDsi = new ActualizacionUsuarioDsi(lstUsuarioFamilia);
                    //Peticiona servicio DataSec para obtener folio de Actualización del Usuario 
                    int folioDsi = actualizacionUsuarioDsi.RealizarPeticion();
                    //Se le coloca un status por default al folio una vez que se genera 
                    int status = Convert.ToInt32(Configuration.GetSection("ENPROCESO").Value, CultureInfo.CurrentCulture);

                    if (folioDsi > 0)
                    {
                        //Actualiza el registro en la tabla de procesos colocando el folio DataSec 
                        var objetoTacrProceso = actualizacionUsuarioDsi.ActualizarFolio(new TacrProcesos
                        {
                            fiFolioProceso = FolioProceso,
                            fiFolioDataSec = folioDsi,
                            fiStatusDataSec = status
                        });
                        return Ok(new RespuestaOK { respuesta = objetoTacrProceso > 0 ? $"Folio DataSec+ {folioDsi} procesado correctamente" : $"Tu solicitud de cambio no fue procesado" });
                    }
                    else
                    {
                        return BadRequest(new RespuestaError400 { errorMessage = this.Configuration.GetSection("msjErrorFolio").Value });
                    }
                }
            }
            catch (Exception e)
            {
               
                 BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                
                if (e.Message.Contains("DATASEC"))
                {
                    return BadRequest(new RespuestaError400 { errorMessage = e.Message });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new RespuestaError { errorInfo = "Error al Actualizar usuario " + e.Message });
                }
            }
        }

        /// <summary>
        /// Obtiene la lista de usuarios activos en BD 
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        [HttpGet("getUsuarios")]
        public IActionResult ObtenerUsuarios()
        {
            Usuarios lstUsr;
            try
            {
                //Obtiene los datos de conexión del cliente
                ObtieneDatosCliente("getUsuarios", "Inicia Método");
                //Se ejecuta método en BD  
                lstUsr = usuarioDao.GetUsuarios();
                if (lstUsr != null)
                {
                    return Ok(new RespuestaOK { respuesta = lstUsr });
                }
                else
                {
                    return BadRequest(new RespuestaError400 { errorMessage = this.Configuration.GetSection("msjErrorUsuarios").Value });
                }
            }
            catch (Exception e)
            {
                BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                return StatusCode(StatusCodes.Status500InternalServerError, new RespuestaError { errorInfo = "Error al obtener la lista de usuarios" + e.Message });
            }
        }


        /// <summary>
        /// Baja de Usuario en BD
        /// </summary>
        /// <param name="fiEmpleado">Id del Empleado a dar de baja</param>
        /// <param name="fiUsuario">Id del usuario que está ejecutando la acción</param>
        /// <returns></returns>
        [HttpGet("bajaUsuario")]
        public IActionResult BajaUsuario(int fiEmpleado, int fiUsuario)
        {
            string msjError = this.Configuration.GetSection("msjErrorBaja").Value;
            try
            {
                //Obtiene los datos de conexión del cliente
                ObtieneDatosCliente("bajaUsuario", null);
                //Se ejecuta método en BD  
                bool respuestaSQL = usuarioDao.BajaUsuario(fiEmpleado, fiUsuario);
                if (!respuestaSQL)
                {
                    BibliotecaSimulador.Logs.PintarLog.PintaInformacion("Respuesta: " + respuestaSQL + msjError, _log);
                    return BadRequest(new RespuestaError400 { errorMessage = msjError });
                }
                else
                {
                    this._log.WriteInfo(this.Configuration.GetSection("msjBaja").Value);
                    return Ok(new RespuestaOK { respuesta = this.Configuration.GetSection("msjBaja").Value });
                }
            }
            catch (Exception e)
            {
                BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                if (e.Message.Contains("DATASEC"))
                {
                    return BadRequest(new RespuestaError400 { errorMessage = e.Message });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new RespuestaError { errorInfo = "Error al dar de Baja el usuario" + e.Message });
                }
            }
        }

        /// <summary>
        /// Obtiene la foto del usuario invocando un servicio
        /// Área responsable del servicio: Sistemas de Comunicación Interna
        /// </summary>
        /// <param name="fcEmpleado">Id del Empleado</param>
        /// <param name="fcEmpresa">Empresa a la que pertenece el Empleado</param>
        /// <returns></returns>
        [HttpGet("getFoto")]
        public async System.Threading.Tasks.Task<IActionResult> GetFoto(string fcEmpleado, string fcEmpresa)
        {
            string urlFoto;
            try
            {
                //Obtiene los datos de conexión del cliente
                ObtieneDatosCliente("getFoto", null);
                //Obtiene la foto del usuario              
                urlFoto = await usuarioDao.FotoUsuarioAsync(fcEmpleado, fcEmpresa);
                return Ok(new RespuestaOK { respuesta = urlFoto ?? "No se obtuvo la foto del usuario" });
            }
            catch (Exception e)
            {
                //Se valida error de TimeOut o acceso a la IP del servicio de Fotos 
                BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                if (e is TimeoutException || e is UnauthorizedAccessException)
                {
                    return BadRequest(new RespuestaError400 { errorMessage = this.Configuration.GetSection("msjErrorWSFotos").Value });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new RespuestaError { errorInfo = "Ocurrió un error al obtener la foto" + e.Message });
                }
            }
        }


        /// <summary>
        /// Valida el status del folio de Baja de DataSec
        /// Área responsable del servicio: CYAAL
        /// </summary>
        /// <param name="FolioDataSec">Número de Solicitud de DataSec</param>
        /// <returns>Status de la solicitud</returns>
        [HttpGet("getEstatusFolio")]
        public IActionResult GetEstatusFolioBaja(string FolioDataSec)
        {
            try
            {
                //Obtiene los datos de conexión del cliente
                ObtieneDatosCliente("getEstatusFolio", null);
                if (!(FolioDataSec is null))
                {
                    //Valida que el formato del folio sea correcto
                    if (new ValidacionExpresionesRegulares { }.ValidarBajaFolioDataSec(FolioDataSec, out string TipoValidacion))
                    {
                        //Realiza la petición al servicio de DataSec para validar el status del folio
                        ActualizacionUsuarioDsi BusquedaFolio = new ActualizacionUsuarioDsi();
                        string EstatusRespuesta = BusquedaFolio.RealizarPeticion(Convert.ToInt32(FolioDataSec, CultureInfo.CurrentCulture));
                        return Ok(new RespuestaOK { respuesta = EstatusRespuesta });
                    }
                    else
                    {
                        return BadRequest(new RespuestaError400 { errorMessage = TipoValidacion });
                    }
                }
                else
                {
                    return BadRequest(new RespuestaError400 { errorMessage = $"No enviaste el parametro {nameof(FolioDataSec)}" });
                }
            }
            catch (DefiniedNullReferenceException e)
            {
                BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                return StatusCode(StatusCodes.Status500InternalServerError, new RespuestaError { errorInfo = $"{e.Message} \n {e.StackTrace}" });
            }
            catch (Exception e)
            {
                BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                return StatusCode(StatusCodes.Status500InternalServerError, new RespuestaError { errorInfo = $"{e.Message} \n {e.StackTrace}" });
            }
        }


        /// <summary>
        /// Obtiene los datos del cliente
        /// </summary>
        /// <param name="metodo">Nombre del método donde se invoca</param>
        /// <param name="msj">Mensaje que se desea guardar en el Log</param>
        public void ObtieneDatosCliente(string metodo, string msj)
        {
            string _IP;
            string _HOSTREMOTO;
            string _SERVER;
            try
            {
                //Obtiene el Servidor
                _SERVER = System.Environment.MachineName;
                //Obtien la IP del cliente 
                _IP = HttpContext.GetServerVariable("HTTP_IPUSUARIO");
                //Obtiene el HostRemoto en caso de que no encuentre la variable HTTP_IPUSUARIO
                _HOSTREMOTO = HttpContext.GetServerVariable("REMOTE_HOST");
                if (_IP == null || _IP == "")
                {
                    _IP = HttpContext.GetServerVariable("REMOTE_ADDR");
                }
                BibliotecaSimulador.Logs.PintarLog.PintaInformacion("IP: " + _IP.Substring(0) + " Server: " + _SERVER + " Host Remoto:" + _HOSTREMOTO + " Método: " + metodo + " Info: " + msj, _log);

            }
            catch (Exception e)
            {
                BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);

            }
        }


        [HttpPost("bajaUsuarioLlM")]
        public IActionResult BajaUsuarioLlaMa(EntBajausuario usuario)
        {
            string msjError = this.Configuration.GetSection("msjErrorBaja").Value;
            try
            {
                //Obtiene los datos de conexión del cliente
                ObtieneDatosCliente("bajaUsuario", null);
                //Se ejecuta método en BD  
                bool respuestaSQL = usuarioDao.BajaUsuarioLlaveMa(int.Parse(usuario.fiEmpleado));
                if (!respuestaSQL)
                {
                    BibliotecaSimulador.Logs.PintarLog.PintaInformacion("Respuesta: " + respuestaSQL + msjError, _log);
                    return BadRequest(new RespuestaError400 { errorMessage = msjError });
                }
                else
                {
                    this._log.WriteInfo(this.Configuration.GetSection("msjBaja").Value);
                    return Ok(new RespuestasBajaUsuario { cgSalida = "CI-101", descSalida = "Baja aplicada exitosamente." });
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("001"))
                {
                    BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                    return Ok(new RespuestasBajaUsuario { cgSalida = "CI-102", descSalida = "El usuario ya se encontraba dado de baja." });
                }
                else if (e.Message.Contains("002"))
                {
                    BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                    return Ok(new RespuestasBajaUsuario { cgSalida = "CI-103", descSalida = "No existe usuario en el sistema." });
                }
                else
                {
                    BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                    return Ok(new RespuestasBajaUsuario { cgSalida = "CI-104", descSalida = "Error al consultar el servicio Baja de Usuarios, intenta más tarde." });
                }
            }
        }

        [HttpPost("bajaUsuarioMyt")]
        public IActionResult BajaUsuariosmyt(EntBajausuario fiUsuarioC)
        {
            string msjError = this.Configuration.GetSection("msjErrorBaja").Value;
            try
            {
                //Obtiene los datos de conexión del cliente
                ObtieneDatosCliente("bajaUsuario", null);
                //Se ejecuta método en BD  
                bool respuestaSQL = usuarioDao.BajaUsuarioLlaveMyt(Int32.Parse(fiUsuarioC.fiEmpleado));
                if (!respuestaSQL)
                {
                    BibliotecaSimulador.Logs.PintarLog.PintaInformacion("Respuesta: " + respuestaSQL + msjError, _log);
                    return BadRequest(new RespuestaError400 { errorMessage = msjError });
                }
                else
                {
                    this._log.WriteInfo(this.Configuration.GetSection("msjBaja").Value);
                    return Ok(new RespuestasBajaUsuario { cgSalida = "CI-101", descSalida = "Baja aplicada exitosamente." });
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("001"))
                {
                    BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                    return Ok(new RespuestasBajaUsuario { cgSalida = "CI-102", descSalida = "El usuario ya se encontraba dado de baja." });
                }
                else if (e.Message.Contains("002"))
                {
                    BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                    return Ok(new RespuestasBajaUsuario { cgSalida = "CI-103", descSalida = "No existe usuario en el sistema." });
                }
                else
                {
                    BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                    return Ok(new RespuestasBajaUsuario { cgSalida = "CI-104", descSalida = "Error al consultar el servicio Baja de Usuarios, intenta más tarde." });
                }
            }
        }
        [HttpPost("RegistraAutorizante")]
        public IActionResult AltaAutorizante(UsuarioAutorizante autorizante)
        {
            string msjError = this.Configuration.GetSection("msjErrorBaja").Value;
            try
            {
                bool respuestaSQL = usuarioDao.AltaAutorizante(autorizante);
                if (!respuestaSQL)
                {
                    return BadRequest(new RespuestaError400 { errorMessage = msjError });
                }
                else
                {
                    return Ok(new RespuestasBajaUsuario { cgSalida = "001", descSalida = "Alta aplicada exitosamente." + respuestaSQL });
                }
            }
            catch (Exception e)
            {

                BibliotecaSimulador.Logs.PintarLog.PintaInformacionError(e, _log);
                return Ok(new RespuestasBajaUsuario { cgSalida = "002", descSalida = "Error al aplicar el alta del autorizante: " + e });

            }
        }

        [HttpPost("RegistraSolicitante")]
        public IActionResult Altasolicitante(UsuarioSolicitante solicitante)
        {
            string msjError = this.Configuration.GetSection("msjErrorBaja").Value;

            {
                bool respuestaSQL = usuarioDao.AltaSolicitante(solicitante);
                try
                {
                    if (!respuestaSQL)
                    {
                        return BadRequest(new RespuestaError400 { errorMessage = msjError });
                    }
                    else
                    {
                        return Ok(new RespuestasBajaUsuario { cgSalida = "001", descSalida = "Alta aplicada exitosamente." + respuestaSQL });
                    }
                }
                catch (Exception e)
                {

                    return Ok(new RespuestasBajaUsuario { cgSalida = "002", descSalida = "Error al aplicar el alta del solicitante: " + e });

                }
            }



        }


    }
}
