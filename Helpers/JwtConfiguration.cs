using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Helpers
{
    public static class JwtConfiguration
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettingsSection = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettingsSection);

            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
            var chaveSecreta = Encoding.ASCII.GetBytes(jwtSettings!.SecretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(options =>
           {
               options.RequireHttpsMetadata = false;
               options.SaveToken = true;
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(chaveSecreta),
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidIssuer = jwtSettings.Issuer,
                   ValidAudience = jwtSettings.Audience,
                   NameClaimType = "nameid"
               };

               // Personaliza a resposta quando o token é inválido ou ausente
               options.Events = new JwtBearerEvents
               {
                   OnChallenge = context =>
                   {
                       context.HandleResponse(); // Suprime a resposta padrão
                       context.Response.StatusCode = 401;
                       context.Response.ContentType = "application/json";
                       var result = System.Text.Json.JsonSerializer.Serialize(new { mensagem = "Usuário não autenticado." });
                       return context.Response.WriteAsync(result);
                   }
               };
           });


            services.AddAuthorization();

            return services;
        }
    }

}
