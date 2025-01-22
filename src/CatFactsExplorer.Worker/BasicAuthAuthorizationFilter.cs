using Hangfire.Dashboard;

public class BasicAuthAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    private readonly string _login;
    private readonly string _password;

    public BasicAuthAuthorizationFilter(BasicAuthAuthorizationFilterOptions options)
    {
        _login = options.Login;
        _password = options.PasswordClear;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var authHeader = httpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic "))
        {
            httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
            httpContext.Response.StatusCode = 401;
            return false;
        }

        var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
        var credentials = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
        var parts = credentials.Split(':');

        return parts.Length == 2 && parts[0] == _login && parts[1] == _password;
    }
}