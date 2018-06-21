using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using PrizeSelection.Api;

namespace PrizeSelection.IntegrationTest
{
    public class TestServerFixture : IDisposable
    {
        private readonly TestServer _testServer;

        public HttpClient Client { get; }

        #region Constants

        private const string LocalEnvironmentKey = "local";
        private const string ConfigFileName = "config";
        private const string ConfigFileExtension = "json";
        #endregion

        public TestServerFixture()
        {
            var builder = new WebHostBuilder()
                .UseContentRoot(GetContentRootPath())
                .UseEnvironment("Development")
                .ConfigureAppConfiguration((hostingContext, config) => ConfigureConfiguration(hostingContext, config))
                .UseStartup<Startup>();  // Uses Start up class from your API Host project to configure the test server 


            _testServer = new TestServer(builder);
            Client = _testServer.CreateClient();

            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Client.BaseAddress = new Uri("http://localhost");

        }

        private string GetContentRootPath()
        {
            var testProjectPath = PlatformServices.Default.Application.ApplicationBasePath;
            //var relativePathToHostProject = @"..\..\..\..\..\..\PrizeSelection.Api";
            var relativePathToHostProject = @"..\..\..\..\PrizeSelection.Api";
            string combinedPath = Path.Combine(testProjectPath, relativePathToHostProject);

            return combinedPath;
        }

        public static IConfigurationBuilder ConfigureConfiguration(WebHostBuilderContext hostingContext, IConfigurationBuilder builder)
        {
            var env = hostingContext.HostingEnvironment;

            builder.SetBasePath(Directory.GetCurrentDirectory());

            builder.AddJsonFile("hosting.json", optional: true);

            if (env.IsEnvironment(LocalEnvironmentKey))
            {
                builder.AddJsonFile($"{ConfigFileName}.{LocalEnvironmentKey}.{ConfigFileExtension}", optional: true);
            }
            else
            {
                builder.AddJsonFile($"{ConfigFileName}.{ConfigFileExtension}");
            }

            builder.AddEnvironmentVariables();

            return builder;
        }

        public void Dispose()
        {
            Client.Dispose();
            _testServer.Dispose();
        }
    }
}
