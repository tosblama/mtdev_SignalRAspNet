// ESCUCHA DE CAMBIOS EN UNA TABLA BASADA EN UNA CONSULTA
// Será utilizada desde un Hub de SignalR, por lo que requiere de las variables adicionales siguientes:
//    public HubCallerContext? callerContext { get; set; }
//    public IHubCallerClients<IClientProxy>? hubCallerClients { get; set; }
//    public static ConcurrentDictionary<string, NombreDeLaClase> cliente = new ConcurrentDictionary<string, NombreDeLaClase>();

using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalR_AspNet.SignalR
{
  public class EscuchandoCambiosQuery

  {
    // Variables que utilizaremos para el acceso a la clase desde un Hub de SignalR
    // de forma que se pueda identificar al cliente que está haciendo la llamada
    public HubCallerContext? callerContext { get; set; }
    public IHubCallerClients<IClientProxy>? hubCallerClients { get; set; }

    public static ConcurrentDictionary<string, EscuchandoCambiosQuery> cliente = new ConcurrentDictionary<string, EscuchandoCambiosQuery>();

    // Variables miembro
    public string _cadenaConexion { get; set; }
    public string _query { get; set; }
    public string _nombreMensaje { get; set; }

    public delegate void MensajeRecibido(object sender, string nombreMensaje);
    public event MensajeRecibido? OnMensajeRecibido = null;
    
    // Clase principal que utilizaremos
    ServiceBrokerSQL sb;

    /// <summary>
    /// Necesitamos que no se inicialice de inicio, ya que será usado por el Hub en diferentes llamadas que establecerán los parámetros de la query
    /// </summary>
    public EscuchandoCambiosQuery()
    {
    }

    /// <summary>
    /// Establecer datos de conexión y escucha
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="SQLquery"></param>
    /// <param name="nombreMensaje"></param>
    public void SetData(string connectionString, string SQLquery, string nombreMensaje)
    {
      _cadenaConexion = connectionString;
      _query = SQLquery;
      _nombreMensaje = nombreMensaje;
    }


    /// <summary>
    /// Iniciar escucha de cambios
    /// </summary>
    public void IniciarEscucha()
    {
      sb = new ServiceBrokerSQL(_cadenaConexion, _query, _nombreMensaje);
      sb.OnMensajeRecibido += new ServiceBrokerSQL.MensajeRecibido(sb_InformacionRecibida);
      sb.IniciarEscucha();
    }

    /// <summary>
    /// Detener escucha de cambios
    /// </summary>
    public void DetenerEscucha()
    {
      sb.DetenerEscucha();
    }


    // Evento de cambio
    private void sb_InformacionRecibida(object sender, string nombreMensaje)
    {
      if (OnMensajeRecibido != null)
      {
        OnMensajeRecibido.Invoke(this, new string("Saltó"));
        this.IniciarEscucha();
      }
    }






  }
}
