using Microsoft.AspNetCore.Diagnostics;

namespace GestãoDeTurmas.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{

    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Um erro inesperado aconteceu na aplicação: {Mensagem}", exception.Message);

        httpContext.Response.Redirect("Home/Error");
        return true;
    }
}
