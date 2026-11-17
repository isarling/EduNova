using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
namespace EduNova.Filters;
public class OperacionExceptionFilter(ILogger<OperacionExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not (DbUpdateException or DbException)) return;
        logger.LogWarning(context.Exception, "Una operación de persistencia no se completó.");
        const string mensaje = "No se completó la operación. Los datos pueden haber cambiado o estar relacionados con otros registros. Revisa el estado antes de volver a intentarlo.";
        if (context.HttpContext.Request.Headers.Accept.ToString().Contains("application/json"))
            context.Result = new ObjectResult(new { success = false, message = mensaje }) { StatusCode = 409 };
        else context.Result = new ViewResult { ViewName = "~/Views/Shared/OperacionNoCompletada.cshtml", StatusCode = 409 };
        context.ExceptionHandled = true;
    }
}
