using System;
using System.Configuration;
using System.Configuration.Abstractions;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Routing;
using ConfigurationManager = System.Configuration.Abstractions.ConfigurationManager;

namespace VersionedRestApi
{
    /// <summary>
    /// This is a System.Attribute meant to be applied to REST actions as a means of specifying the Uri fragment
    /// of the resource (e.g. "SomeResource/") as well as a prefix for the version based on either the AcceptedVersions
    /// or StartingVersion being set. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ApiRouteAttribute : Attribute, IDirectRouteFactory, IHttpRouteInfoProvider
    {
        /// <summary>
        /// Designates the specific versions of the REST api to which this action should respond. For example, if
        /// the Acceptedversions are {1,2} then the the action will handle requests to /v1/{Resource} and 
        /// /v2/{Resource}. Cannot be used in conjunction with the StartingVersion property.
        /// </summary>
        public int[] AcceptedVersions { get; set; }
        /// <summary>
        /// Designates the first version of the API that supports this action. For example, if StartingVersion
        /// is set to 2 and the currentApiVersion in the config is set to 3, then this means the action will respond
        /// to requests to /v2/{Resource} and /v3/{Resource}. Cannot be used in conjunction with the AcceptedVersions
        /// property.
        /// </summary>
        public int StartingVersion { get; set; }
        public string Name { get; set; }
        public string Template { get; private set; }
        public int Order { get; set; }
        internal string RoutePrefix { get; set; }

        internal IConfigurationManager ConfigurationManager = new ConfigurationManager();

        internal const string APP_KEY_CURRENT_API_VERSION = "currentApiVersion";

        /// <summary>
        /// This constructor is automatically called when using the RouteAttribute annotation.
        /// </summary>
        /// <param name="template">The user-specified Uri template to which this action responds. Cannot start
        /// with a forward slash ('/')</param>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the template starts with a forward-slash,
        /// since this is automatically added.</exception>
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
                if (StartingVersion != 0)
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

        private static int[] GetSupportedVersions(int startVersion, IConfigurationManager configurationManager)
        {
            string currentApiVersionStringValue = configurationManager.AppSettings.AppSetting(APP_KEY_CURRENT_API_VERSION);

            int currentApiVersion = ValidateCurrentApiVersionAndStartVersion(startVersion, currentApiVersionStringValue);

            if (startVersion == 0)
            {
                startVersion = 1;
            }

            int numberOfVersions = 1 + currentApiVersion - startVersion;

            var supportedVersions = new int[numberOfVersions];

            for (int i = 0; i < numberOfVersions; i++)
            {
                supportedVersions[i] = startVersion + i;
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