namespace SimpleAuthToken.Example.API;

public class SimpleAuthTokenMiddleware : IMiddleware
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SimpleAuthTokenMiddleware> _logger;

    public SimpleAuthTokenMiddleware(IConfiguration configuration, ILogger<SimpleAuthTokenMiddleware> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (IsRequestAuthorized(context))
        {
            await next(context);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("Invalid authentication token!");
        }
    }
    
    private bool IsRequestAuthorized(HttpContext context)
    {
        var rawToken = context.Request.Headers.ContainsKey("Authorization") ? context.Request.Headers["Authorization"].ToString() : null;
        if (string.IsNullOrEmpty(rawToken))
        {
            _logger.LogDebug("No authentication JWT token provided!");
            return false;
        }

        var secretKey = _configuration.GetValue<string?>("TokenConfig:SecretKey");
        if (string.IsNullOrEmpty(secretKey))
        {
            _logger.LogDebug("SecretKey missing or empty from application settings!");
            return false;
        }

        try
        {
            var token = string.Join("", rawToken.Split(' ').Skip(1).ToArray());
            var issuer = _configuration.GetValue("TokenConfig:Issuer", string.Empty);
            var isValid = TokenProvider.ValidateToken(token, secretKey, issuer, out var publicKey);
            _logger.LogInformation("The provided Auth token with the public key [{Key}], is: {State}", publicKey, isValid ? "VALID" : "INVALID");

            return isValid;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to validate JWT token");
            return false;
        }
    }
}