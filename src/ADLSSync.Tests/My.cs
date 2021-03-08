
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Azure.Identity.Extensions;

namespace ADLSSync.Tests
{
    internal static class My
    {
        //...............................................................................
        // Configuration
        //...............................................................................
        internal static IConfiguration Config => LazyConfiguration.Value;

        static readonly Lazy<IConfiguration> LazyConfiguration = new Lazy<IConfiguration>(BuildMyConfigurationOnce);


        static IConfiguration BuildMyConfigurationOnce()
        {
            try
            {
                return new ConfigurationBuilder()
                    .AddIniFile("AppSettings.ini", optional: false, reloadOnChange: true)
                    .Build();
            }
            catch(Exception err)
            {
                throw new Exception("Error preparing IConfiguration", err);
            }
        }

        //...............................................................................
        // The Servic Locator anti-pattern...
        //...............................................................................
        internal static IServiceProvider Services => LazyServiceCollection.Value;

        static readonly Lazy<IServiceProvider> LazyServiceCollection = new Lazy<IServiceProvider>(BuildMyServicesOnce);

        static IServiceProvider BuildMyServicesOnce()
        {
            try
            {
                return new ServiceCollection()
                    .AddSingleton<IConfiguration>(My.Config)
                    .AddConfidentialClientCredentialProvider()
                    .BuildServiceProvider()
                    ;
            }
            catch (Exception err)
            {
                throw new Exception("Error preparing ServiceCollection", err);
            }
        }
    }
}
