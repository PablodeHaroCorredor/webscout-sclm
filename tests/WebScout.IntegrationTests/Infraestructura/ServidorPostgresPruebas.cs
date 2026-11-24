using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;

namespace WebScout.IntegrationTests;

/// <summary>
/// Servidor PostgreSQL compartido por todo el ensamblado de pruebas de integración (AGENTS.md §3.1).
/// <para>
/// Origen del servidor, por orden de preferencia:
/// <list type="number">
///   <item>Variable de entorno <c>ConnectionStrings__DefaultConnection</c> (la define el job de CI;
///   en local puede apuntar al PostgreSQL de <c>docker-compose.dev.yml</c>, puerto 5433).</item>
///   <item>Contenedor efímero <c>postgres:17-alpine</c> levantado con Testcontainers (requiere Docker).</item>
/// </list>
/// </para>
/// <para>
/// Al arrancar crea una base plantilla con nombre único y le aplica las migraciones reales de Npgsql
/// (<c>Database.MigrateAsync()</c>). Cada instancia de clase de prueba (es decir, cada test) clona esa plantilla en una base propia
/// (<see cref="BaseDatosPostgresPruebas"/>), de modo que las pruebas siguen ejecutándose en paralelo
/// sin compartir datos. Nunca se escribe en la base indicada por la cadena de conexión: solo se usa
/// como base de mantenimiento para crear y borrar las bases de prueba.
/// </para>
/// </summary>
internal static class ServidorPostgresPruebas
{
    public const string VariableCadenaConexion = "ConnectionStrings__DefaultConnection";
    public const string PrefijoBasesPrueba = "webscout_it_";
    private const string ImagenPostgres = "postgres:17-alpine";

    private static readonly Lazy<Task<EstadoServidor>> Estado =
        new(InicializarAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    internal sealed record EstadoServidor(
        string CadenaMantenimiento,
        NpgsqlConnectionStringBuilder CadenaBase,
        string NombrePlantilla);

    /// <summary>
    /// Devuelve el servidor inicializado. Se bloquea fuera del contexto de sincronización de xUnit
    /// (<see cref="Task.Run(Func{Task})"/>) para poder usarse desde constructores de prueba.
    /// </summary>
    public static EstadoServidor Obtener() => Task.Run(() => Estado.Value).GetAwaiter().GetResult();

    private static async Task<EstadoServidor> InicializarAsync()
    {
        var cadena = Environment.GetEnvironmentVariable(VariableCadenaConexion);
        PostgreSqlContainer? contenedor = null;

        if (string.IsNullOrWhiteSpace(cadena))
        {
            contenedor = new PostgreSqlBuilder(ImagenPostgres).Build();
            await contenedor.StartAsync();
            cadena = contenedor.GetConnectionString();
        }

        var cadenaBase = new NpgsqlConnectionStringBuilder(cadena);

        // Las conexiones de mantenimiento y de la plantilla no usan pool: CREATE DATABASE ... TEMPLATE
        // falla si queda cualquier sesión abierta contra la plantilla.
        var mantenimiento = new NpgsqlConnectionStringBuilder(cadenaBase.ConnectionString) { Pooling = false };
        var nombrePlantilla = $"{PrefijoBasesPrueba}tpl_{Guid.NewGuid():N}";

        await EjecutarAsync(mantenimiento.ConnectionString, $"CREATE DATABASE \"{nombrePlantilla}\"");

        var cadenaPlantilla = new NpgsqlConnectionStringBuilder(cadenaBase.ConnectionString)
        {
            Database = nombrePlantilla,
            Pooling = false
        };

        var opciones = new DbContextOptionsBuilder<WebScoutDbContext>()
            .UseNpgsql(cadenaPlantilla.ConnectionString)
            .Options;

        await using (var db = new WebScoutDbContext(opciones, new TenantContext()))
        {
            await db.Database.MigrateAsync();
        }

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            try
            {
                if (contenedor is not null)
                {
                    // El contenedor se lleva consigo la plantilla.
                    contenedor.DisposeAsync().AsTask().GetAwaiter().GetResult();
                }
                else
                {
                    Ejecutar(mantenimiento.ConnectionString, $"DROP DATABASE IF EXISTS \"{nombrePlantilla}\" WITH (FORCE)");
                }
            }
            catch
            {
                // Limpieza de mejor esfuerzo al terminar el proceso de pruebas.
            }
        };

        return new EstadoServidor(mantenimiento.ConnectionString, cadenaBase, nombrePlantilla);
    }

    internal static void Ejecutar(string cadenaConexion, string sql)
    {
        using var conexion = new NpgsqlConnection(cadenaConexion);
        conexion.Open();
        using var comando = new NpgsqlCommand(sql, conexion);
        comando.ExecuteNonQuery();
    }

    private static async Task EjecutarAsync(string cadenaConexion, string sql)
    {
        await using var conexion = new NpgsqlConnection(cadenaConexion);
        await conexion.OpenAsync();
        await using var comando = new NpgsqlCommand(sql, conexion);
        await comando.ExecuteNonQueryAsync();
    }
}
