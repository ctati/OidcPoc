using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace server;

// NOTE: commands to use .NET 7.0 JWT tool
// NOTE: the tool stored the signing key in User Secrets file... use VS user secrets tool to view this config
// dotnet user-jwts create --name ctati --audience ed309999-5993-46f5-8676-8f5ed7e0ddb7 --scope openid --scope profile --role admin --claim email=ctati@pa.gov
// dotnet user-jwts list
// dotnet user-jwts print {token id}

public static class JwtBearerConfiguration
{
    public static AuthenticationBuilder AddJwtBearerConfiguration(this AuthenticationBuilder builder, WebApplicationBuilder appBuilder)
    {
        return builder.AddJwtBearer(options =>
        {
            // load settings from configuration file
            Configure(appBuilder, JwtBearerDefaults.AuthenticationScheme, options);

            // NOTE: disable HTTPS for metadata endpoints in development -- 
            if (appBuilder.Environment.IsDevelopment())
                options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.ClockSkew = new System.TimeSpan(0, 0, 30);

            /// wire up code to return error details to caller
            options.Events = new JwtBearerEvents()
            {
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    // Ensure we always have an error and error description.
                    if (string.IsNullOrEmpty(context.Error))
                        context.Error = "invalid_token";
                    if (string.IsNullOrEmpty(context.ErrorDescription))
                        context.ErrorDescription = "This request requires a valid JWT access token to be provided";

                    if (context.AuthenticateFailure is SecurityTokenExpiredException authenticationException)
                    {
                        context.Response.Headers.Add("x-token-expired", authenticationException.Expires.ToString("o"));
                        context.ErrorDescription = $"The token expired on {authenticationException.Expires.ToString("o")}";
                    }

                    return context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        error = context.Error,
                        error_description = context.ErrorDescription
                    }));
                }
            };
        });
    }

    /// <summary>
    /// function mimics .net 7.0 configuration loading to help reduce future migration effort
    /// </summary>
    private const string AuthenticationKey = "Authentication";
    private const string AuthenticationSchemesKey = "Schemes";

    private static void Configure(WebApplicationBuilder builder, string? name, JwtBearerOptions options)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        var config = builder.Configuration;
        var configSection = config.GetSection($"{AuthenticationKey}:{AuthenticationSchemesKey}:{name}");
        if (configSection is null || !configSection.GetChildren().Any())
        {
            return;
        }

        var issuer = configSection[nameof(TokenValidationParameters.ValidIssuer)];
        var issuers = configSection.GetSection(nameof(TokenValidationParameters.ValidIssuers)).GetChildren().Select(iss => iss.Value).ToList();
        if (issuer is not null)
        {
            issuers.Add(issuer);
        }
        var audience = configSection[nameof(TokenValidationParameters.ValidAudience)];
        var audiences = configSection.GetSection(nameof(TokenValidationParameters.ValidAudiences)).GetChildren().Select(aud => aud.Value).ToList();
        if (audience is not null)
        {
            audiences.Add(audience);
        }
        // NOTE: authority will only be set in non-development environments as we don't typically have OIDC setup for local development
        // for local development setup up issuer and signing keys
        if (!builder.Environment.IsDevelopment())
            options.Authority = configSection[nameof(options.Authority)] ?? options.Authority;
        options.TokenValidationParameters.ValidateIssuer = issuers.Count > 0;
        options.TokenValidationParameters.ValidIssuers = issuers;
        options.TokenValidationParameters.ValidateAudience = audiences.Count > 0;
        options.TokenValidationParameters.ValidAudiences = audiences;
        options.TokenValidationParameters.ValidateIssuerSigningKey = true;
        options.TokenValidationParameters.IssuerSigningKeys = GetIssuerSigningKeys(configSection, issuers);
    }

    private const string KeysSectionName = "SigningKeys";
    private const string IssuerKey = "Issuer";
    private const string ValueKey = "Value";

    private static IEnumerable<SecurityKey> GetIssuerSigningKeys(IConfiguration configuration, List<string> issuers)
    {
        foreach (var issuer in issuers)
        {
            var signingKey = configuration.GetSection(KeysSectionName)
                .GetChildren()
                .SingleOrDefault(key => key[IssuerKey] == issuer);
            if (signingKey is not null && signingKey[ValueKey] is string keyValue)
            {
                yield return new SymmetricSecurityKey(Convert.FromBase64String(keyValue));
            }
        }
    }
}