using System.Data;
using System.Data.SqlClient;

namespace SignalR_AspNet.SignalR
{
  public class ServiceBrokerSQL
  {
    private string nombreMensaje = "";
    private string cadenaConexion = "";
    private string comandoEscucha = "";
    private SqlConnection conexion;

    public delegate void MensajeRecibido(object sender, string nombreMensaje);
    public event MensajeRecibido? OnMensajeRecibido = null;

    /// <summary>
    /// Inicializador.
    /// </summary>
    /// <param name="cadenaConexion"></param>
    /// <param name="comandoEscucha"></param>
    public ServiceBrokerSQL(string connectionString, string SQLquery, string nombreMensaje)
    {
      if (connectionString == "" || SQLquery == "") throw new ApplicationException("Debe indicar la cadena de conexión y el comando de escucha");
      this.cadenaConexion = connectionString;
      this.comandoEscucha = SQLquery;
      this.nombreMensaje = nombreMensaje;
      // Iniciar la escucha. El usuario debe tener permisos de SUBSCRIBE QUERY NOTIFICATIONS. La base de datos debe tener SSB enabled. ALTER DATABASE NombreBaseDatos SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE.
      SqlDependency.Start(cadenaConexion);
      // Crear la conexión a base de datos.
      conexion = new SqlConnection(cadenaConexion);
    }

    /// <summary>
    /// Destructor.
    /// </summary>
    ~ServiceBrokerSQL()
    {
      // Detener la escucha antes de salir.
      SqlDependency.Stop(cadenaConexion);
    }


    /// <summary>
    /// Obtener mensajes desde la base de datos.
    /// </summary>
    /// <returns></returns>
    public void IniciarEscucha()
    {
      try
      {
        // Crear el comando de escucha. Debe incluir el esquema y no usar *. Ejemplo: SELECT campo FROM dbo.Tabla
        SqlCommand cmd = new SqlCommand(comandoEscucha, conexion);
        cmd.CommandType = CommandType.Text;

        // Limpiar cualquier notificación existente
        cmd.Notification = null;

        // Crear una dependencia para el comando
        SqlDependency dependency = new SqlDependency(cmd);

        // Añadir un controlador del evento
        dependency.OnChange += new OnChangeEventHandler(OnChange);

        // Abrir la conexión si es necesario
        if (conexion.State == ConnectionState.Closed) conexion.Open();

        // Obtener los mensajes y luego cerrar la conexión
        cmd.ExecuteReader(CommandBehavior.CloseConnection);
      }
      catch (Exception)
      {
        throw;
      }
    }


    /// <summary>
    /// Detener escucha
    /// </summary>
    public void DetenerEscucha()
    {
      SqlDependency.Stop(cadenaConexion);
    }


    /// <summary>
    /// Controlador para el evento OnChange de SqlDependency
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnChange(object sender, SqlNotificationEventArgs e)
    {
      SqlDependency dependency = (SqlDependency)sender;

      // Las notificaciones son un aviso de un sólo disparo, así que se debe eliminar el existente para poder agregar uno nuevo.
      dependency.OnChange -= OnChange;

      // Disparar el evento
      if (OnMensajeRecibido != null)
      {
        OnMensajeRecibido(this, new string(nombreMensaje));
      }
    }
  }
}
