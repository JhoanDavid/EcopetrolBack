using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;

namespace Productividad
{
    public class Program
    {

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>

            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, builder) =>
             {
                 var settings = builder.Build();
                 var vaultName = settings["KeyVault:Vault"];
                 builder.AddAzureKeyVault($"https://{vaultName}.vault.azure.net/",
                 settings["KeyVault:ClientId"],
                 settings["KeyVault:ClientSecret"]);
             })
             .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
