using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebScout.Web.Infrastructure.Persistence;

namespace WebScout.IntegrationTests;

/// <summary>
/// Base de datos PostgreSQL aislada para una instancia de clase de prueba.
/// <para>
/// xUnit crea una instancia nueva de la clase por cada test, así que cada test recibe su propia base,
/// clonada de la plantilla ya migrada (<c>CREATE DATABASE ... TEMPLATE</c>, unas decenas de milisegundos).
/// Al desecharse, la base se borra con <c>DROP DATABASE ... WITH (FORCE)</c>.
/// </para>
/// </summary>
public sealed class BaseDatosPostgresPruebas : IDisposable
{
    private const string CodigoObjetoEnUso = "55006";
    private const int IntentosMaximos = 10;

    private readonly string _cadenaMantenimiento;
    private bool _desechada;

    public string Nombre { get; }

    public string CadenaConexion { get; }

    public DbContextOptions<WebScoutDbContext> Opciones { get; }

    private BaseDatosPostgresPruebas(ServidorPostgresPruebas.EstadoServidor servidor)
    {
        _cadenaMantenimiento = servidor.CadenaMantenimiento;
        Nombre = $"{ServidorPostgresPruebas.PrefijoBasesPrueba}{Guid.NewGuid():N}";

        CrearDesdePlantilla(servidor.NombrePlantilla);

        CadenaConexion = new NpgsqlConnectionStringBuilder(servidor.CadenaBase.ConnectionString)
        {
            Database = Nombre
        }.ConnectionString;

        Opciones = new DbContextOptionsBuilder<WebScoutDbContext>()
            .UseNpgsql(CadenaConexion)
            .Options;
    }

    public static BaseDatosPostgresPruebas Crear() => new(ServidorPostgresPruebas.Obtener());

    private void CrearDesdePlantilla(string nombrePlantilla)
    {
        for (var intento = 1; ; intento++)
        {
            try
            {
                ServidorPostgresPruebas.Ejecutar(
                    _cadenaMantenimiento,
                    $"CREATE DATABASE \"{Nombre}\" TEMPLATE \"{nombrePlantilla}\"");
                return;
            }
            catch (PostgresException ex) when (ex.SqlState == CodigoObjetoEnUso && intento < IntentosMaximos)
            {
                // Otra clonación concurrente tiene ocupada la plantilla: reintento breve.
                Thread.Sleep(50 * intento);
            }
        }
    }

    public void Dispose()
    {
        if (_desechada)
        {
            return;
        }

        _desechada = true;

        using (var conexion = new NpgsqlConnection(CadenaConexion))
        {
            NpgsqlConnection.ClearPool(conexion);
        }

        ServidorPostgresPruebas.Ejecutar(
            _cadenaMantenimiento,
            $"DROP DATABASE IF EXISTS \"{Nombre}\" WITH (FORCE)");
    }
}
