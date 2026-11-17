using EduNova.Models;
namespace EduNova.Services;
// Reglas puras: permiten explicar y probar las decisiones sin acceder a SQL Server.
public static class ReglasCredito
{
    public static DateTime Hoy => DateTime.UtcNow.AddHours(-6).Date;
    public static decimal Dinero(decimal valor) => decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
    public static string? ValidarNueva(PoliticaCredito p, decimal deuda, decimal nuevoTotal, decimal inicial, int activos, int nuevos, bool atrasado)
    {
        if (atrasado) return "Hay cuotas vencidas fuera del plazo permitido. Regulariza los pagos antes de matricular más cursos.";
        if (deuda > p.DeudaMaxima) return "La deuda actual supera el límite permitido.";
        if (inicial < 0 || inicial > nuevoTotal) return "El pago inicial debe estar entre cero y el total de la matrícula.";
        if (inicial < Dinero(nuevoTotal * p.PorcentajeInicial / 100m)) return "El pago inicial no alcanza el porcentaje mínimo requerido.";
        if (deuda + nuevoTotal - inicial > p.DeudaMaxima) return "La nueva inscripción superaría el límite total de deuda del estudiante.";
        var limite = deuda + nuevoTotal - inicial > 0 ? p.MaxCursosConDeuda : p.MaxCursosSinDeuda;
        if (activos + nuevos > limite) return $"Con el saldo resultante solo puede tener {limite} cursos activos simultáneamente.";
        return null;
    }
    public static string? ValidarAbono(PoliticaCredito p, decimal saldo, decimal monto)
    {
        if (monto <= 0 || Dinero(monto) != monto) return "El abono debe ser positivo y tener máximo dos decimales.";
        if (monto > saldo) return "El abono supera el saldo pendiente.";
        if (monto < p.AbonoMinimo && monto != saldo) return "El abono no alcanza el mínimo configurado. Se permite un monto menor únicamente para liquidar el saldo completo.";
        return null;
    }
    public static List<Cuota> CrearCuotas(decimal total, int cantidad, DateTime fecha, int dias)
    {
        if (cantidad < 1 || dias < 1 || total <= 0 || cantidad > total * 100m) throw new ArgumentException("Plan de cuotas inválido.");
        var centavos = (long)(Dinero(total) * 100m);
        var baseCuota = centavos / cantidad;
        var sobrantes = centavos % cantidad;
        return Enumerable.Range(1, cantidad).Select(i => new Cuota { Numero = i, Vencimiento = fecha.Date.AddDays(dias * i), Monto = (baseCuota + (i <= sobrantes ? 1 : 0)) / 100m }).ToList();
    }
    public static List<(Cuota Cuota, decimal Monto)> Distribuir(IEnumerable<Cuota> cuotas, decimal monto)
    {
        var resultado = new List<(Cuota, decimal)>();
        foreach (var c in cuotas.Where(c => c.Monto > c.Pagado).OrderBy(c => c.Vencimiento).ThenBy(c => c.Id).ThenBy(c => c.DetalleMatriculaId).ThenBy(c => c.Numero))
        {
            var parte = Math.Min(c.Monto - c.Pagado, monto);
            if (parte > 0) { resultado.Add((c, parte)); monto -= parte; }
            if (monto == 0) break;
        }
        if (monto != 0) throw new InvalidOperationException("El plan de cuotas no coincide con el saldo. Revisa los datos antes de registrar el pago.");
        return resultado;
    }
}
