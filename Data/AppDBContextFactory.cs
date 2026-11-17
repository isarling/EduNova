using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace EduNova.Data;
// La consola de migraciones no necesita iniciar el servicio de correo ni emitir tokens.
public class AppDBContextFactory : IDesignTimeDbContextFactory<AppDBContext>
{
    public AppDBContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<AppDBContextFactory>(optional: true).AddEnvironmentVariables().Build();
        var connection = configuration.GetConnectionString("EduNova")
            ?? configuration.GetConnectionString("AcademiaAula")
            ?? configuration.GetConnectionString("IsarlingProyecto")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=EduNova;Trusted_Connection=True;TrustServerCertificate=True";
        return new AppDBContext(new DbContextOptionsBuilder<AppDBContext>().UseSqlServer(connection).Options);
    }
}
