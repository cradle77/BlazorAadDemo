using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebAssembly;

namespace Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services
                .AddHttpClient("weatherapi", client =>
                {
                    client.BaseAddress = new Uri("https://localhost:5002");
                })
                .AddHttpMessageHandler(sp =>
                {
                    var handler = sp.GetRequiredService<AuthorizationMessageHandler>()
                        .ConfigureHandler(new[] { "https://localhost:5002" },
                        scopes: new[] { "api://blazor-demo/weatherapi" });

                    return handler;
                });

            builder.Services.AddSingleton(sp =>
            {
                var factory = sp.GetService<IHttpClientFactory>();

                return factory.CreateClient("weatherapi");
            });

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
                options.UserOptions.RoleClaim = "roles";
            }).AddAccountClaimsPrincipalFactory<ArrayClaimsPrincipalFactory<RemoteUserAccount>>();

            builder.Services.RemoveAll<IAuthorizationPolicyProvider>();

            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            await builder.Build().RunAsync();
        }
    }
}
