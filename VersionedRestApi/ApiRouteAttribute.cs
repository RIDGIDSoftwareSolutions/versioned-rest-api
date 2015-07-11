using System;
using System.Configuration;
using System.Configuration.Abstractions;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Routing;
using ConfigurationManager = System.Configuration.Abstractions.ConfigurationManager;

namespace VersionedRestApi
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ApiRouteAttribute : Attribute, IDirectRouteFactory, IHttpRouteInfoProvider
    {
        public int[] AcceptedVersions { get; set; }
        public int? StartingVersion { get; set; }
        public string Name { get; set; }
        public string Template { get; private set; }
        public int Order { get; set; }
        internal string RoutePrefix { get; set; }

        internal IConfigurationManager ConfigurationManager = new ConfigurationManager();

        internal const string APP_KEY_CURRENT_API_VERSION = "currentApiVersion";

        public ApiRouteAttribute(string template)
        {
            ValidateTemplateIsNotNullOrBlank(template);
            if (template.StartsWith("/"))
            {
                throw new ArgumentException("The route cannot start with a forward slash ('/') since it will be prefixed with the api version (e.g. api/v2/).");
            }

            Template = template;
        }

        public RouteEntry CreateRoute(DirectRouteFactoryContext context)
        {
            Template = this.BuildRoute();
            Contract.Assert(context != null);

            IDirectRouteBuilder builder = context.CreateBuilder(Template);
            Contract.Assert(builder != null);

            builder.Name = Name;
            builder.Order = Order;
            return builder.Build();
        }

        private static void ValidateTemplateIsNotNullOrBlank(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentNullException("template");
            }
        }

        internal virtual string BuildRoute()
        {
            if (AcceptedVersions == null)
            {
                AcceptedVersions = GetSupportedVersions(StartingVersion, ConfigurationManager);
            }
            else
            {
                if (StartingVersion.HasValue)
                {
                    throw new InvalidOperationException("Either 'AcceptedVersions' or 'StartingVersion' can be set, but not both.");
                }
                if (AcceptedVersions.Any(version => version < 1))
                {
                    throw new InvalidOperationException("The explicitly specified AcceptedVersion values must all be positive integers.");
                }
            }
            string routePrefix = "api/v{version:int:regex(" + string.Join("|", AcceptedVersions) + ")}";
            return routePrefix + "/" + Template;
        }

        private static int[] GetSupportedVersions(int? startVersion, IConfigurationManager configurationManager)
        {
            string currentApiVersionStringValue = configurationManager.AppSettings.AppSetting(APP_KEY_CURRENT_API_VERSION);

            int currentApiVersion = ValidateCurrentApiVersionAndStartVersion(startVersion, currentApiVersionStringValue);

            if (!startVersion.HasValue)
            {
                startVersion = 1;
            }

            int numberOfVersions = 1 + currentApiVersion - startVersion.Value;

            var supportedVersions = new int[numberOfVersions];

            for (int i = 0; i < numberOfVersions; i++)
            {
                supportedVersions[i] = startVersion.Value + i;
            }
            return supportedVersions;
        }

        private static int ValidateCurrentApiVersionAndStartVersion(int? startVersion, string currentApiVersionStringValue)
        {
            int currentApiVersion;
            bool validInteger = int.TryParse(currentApiVersionStringValue, out currentApiVersion);

            if (!validInteger || currentApiVersion < 1)
            {
                throw new ConfigurationErrorsException("The 'currentApiVersion' app setting must be a positive integer.");
            }

            if (startVersion.HasValue && startVersion > currentApiVersion)
            {
                throw new InvalidOperationException("The 'StartingVersion' cannot be greater than the 'currentApiVersion' specified in the config.");
            }

            return currentApiVersion;
        }
    }
}