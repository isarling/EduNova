using EduNova.DTOS;
using EduNova.Models;
namespace EduNova.Services;
public interface ICreditoService
{
    Task<ResultadoOperacion> CrearMatriculaAsync(CrearMatriculaDto dto);
    Task<ResultadoOperacion> RegistrarPagoAsync(RegistrarPagoDto dto);
    Task<EstadoEstudianteDto?> EstadoAsync(int estudianteId);
    Task<EstadoEstudianteDto?> EstadoPropioAsync(int usuarioId);
    Task<ResultadoOperacion> EmitirSolvenciaAsync(int estudianteId);
    Task<ReciboPago?> ReciboAutorizadoAsync(int id, int usuarioId, bool personal);
    Task<ConstanciaSolvencia?> SolvenciaAutorizadaAsync(int id, int usuarioId, bool personal);
    Task<PoliticaCreditoDto> PoliticaAsync();
    Task<string?> GuardarPoliticaAsync(PoliticaCreditoDto dto);
    Task<VincularVistaDto> VinculosAsync();
    Task<string?> VincularAsync(VincularCuentaDto dto);
    Task<CatalogosVistaDto> CatalogosAsync();
    Task<string?> GuardarCatalogoAsync(CatalogoDto dto);
    Task<string?> EliminarCatalogoAsync(string tipo, int id);
}
