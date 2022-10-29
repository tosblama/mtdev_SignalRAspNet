// EJEMPLO DE USO DE SignalR PARA NOTIFICAR EVENTOS POR WEBSOCKET
// Para el ejemplo se hace uso de ServiceBroker para lanzar un socket cada vez que cambie una tabla de la base de datos.
//
// COMO PROBAR:
// Usando postman, en la opción "WebSockets Request", conectar a la URL: wss://localhost:7282/SignalR
// Enviar los siguientes mensajes (hay que incluir el caracter final):
//  1. Conectar
//  2. Enviar un mensaje para indicar que se va a utilizar el protocolo JSON: {"protocol":"json","version":1}
//  3. Enviar mensaje de prueba que llega a todos los clientes: {"type":1, "target":"EnviarMensaje", "arguments":["NombreUsuario","Hola que tal"]}
//  4. Activar escucha de cambios en SQL: {"type":1, "target":"IniciarEscuchaAlertas", "arguments":[1]}
//  5. Detener escucha de cambios en SQL: {"type":1, "target":"DetenerEscuchaAlertas", "arguments":[1]}

using Microsoft.AspNetCore.SignalR;

namespace SignalR_AspNet.SignalR
{

  public class MiClaseSignalR : Hub
  {
    // Clase que utilizaremos desde el hub
    public EscuchandoCambiosQuery sbAlertas = new EscuchandoCambiosQuery();

    /// <summary>
    /// Enviar un mensaje de respuesta todos los usuarios.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task EnviarMensaje(string user, string message)
    {
      await Clients.Caller.SendAsync("Respuesta de SignalR: ", user, message);
    }

    /// <summary>
    /// Iniciar escucha de alertas del usuario dado.
    /// </summary>
    /// <param name="idUsuario"></param>
    /// <returns></returns>
    public async Task IniciarEscuchaAlertas(int idUsuario)
    {
      // Agregar/Seleccionar cliente a nuestra clase a utilizar
      sbAlertas = EscuchandoCambiosQuery.cliente.GetOrAdd(Context.ConnectionId, sbAlertas);
      sbAlertas.callerContext = Context;
      sbAlertas.hubCallerClients = Clients;

      // Utilizar la clase
      sbAlertas.SetData(
        @"Data Source=W11-EPV;Initial Catalog=EPV;Integrated Security=true;MultipleActiveResultSets=true;", 
        "SELECT IdAlerta FROM dbo.Alertas WHERE IdUsuario=1", 
        "ALERTAS_ESCUCHA");
      sbAlertas.OnMensajeRecibido += new EscuchandoCambiosQuery.MensajeRecibido(sbAlertas_InformacionRecibida);
      sbAlertas.IniciarEscucha();
      await Clients.Caller.SendAsync("Escucha de alertas iniciada");
    }

    /// <summary>
    /// Detener escucha de alertas del usuario dado.
    /// </summary>
    /// <param name="idUsuario"></param>
    /// <returns></returns>
    public async Task DetenerEscuchaAlertas(int idUsuario)
    {
      // Seleccionar cliente que va a detener la escucha
      sbAlertas = EscuchandoCambiosQuery.cliente.GetOrAdd(Context.ConnectionId, sbAlertas);

      // Detener la escucha
      sbAlertas.OnMensajeRecibido -= sbAlertas_InformacionRecibida;
      await Clients.Caller.SendAsync("Escucha de alertas detenida");
    }

    /// <summary>
    /// Evento de cambio escuchando alertas.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="mensaje"></param>
    private static void sbAlertas_InformacionRecibida(object sender, string mensaje)
    {
      var sb = (EscuchandoCambiosQuery)sender;
      HubCallerContext hcallerContext = sb.callerContext;
      IHubCallerClients<IClientProxy> hubClients = sb.hubCallerClients;
      hubClients.Caller.SendAsync($"El mensaje {mensaje} indica un cambio el " + DateTime.Now.ToString());
    }


  }
}
