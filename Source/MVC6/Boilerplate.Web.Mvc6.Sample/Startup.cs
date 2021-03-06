﻿namespace MvcBoilerplate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Boilerplate.Web.Mvc;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Diagnostics;
    using Microsoft.AspNet.Hosting;
    using Microsoft.AspNet.Mvc;
    using Microsoft.AspNet.Mvc.OptionDescriptors;
    using Microsoft.Framework.ConfigurationModel;
    using Microsoft.Framework.DependencyInjection;
    using Microsoft.Framework.Logging;
    using MvcBoilerplate.Constants;

    /// <summary>
    /// The main start-up class for the application.
    /// </summary>
    public class Startup
    {
        #region Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="environment">The environment the application is running under. This can be Development, 
        /// Staging or Production by default.</param>
        public Startup(IHostingEnvironment environment)
        {
            this.Configuration = new Configuration()
                .AddJsonFile("config.json") // Add configuration from the config.json file.
                .AddEnvironmentVariables(); // Add configuration specific to the default Development, Staging or Production environments.
        } 

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the configuration, where key value pair settings can be stored. See 
        /// http://docs.asp.net/en/latest/fundamentals/configuration.html?highlight=configuration
        /// </summary>
        public IConfiguration Configuration { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Configures the services to add to the ASP.NET MVC 6 Injection of Control (IoC) container. This method gets 
        /// called by the ASP.NET runtime. See
        /// http://blogs.msdn.com/b/webdev/archive/2014/06/17/dependency-injection-in-asp-net-vnext.aspx
        /// </summary>
        /// <param name="services">The services collection or IoC container.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add IOptions<AppSettings> to the services container. Use it to check what environment the application is
            // running under. Development, Staging or Production by default.
            services.Configure<AppSettings>(this.Configuration.GetSubKey("AppSettings"));

            // Add many MVC services to the services container.
            services.AddMvc();

            services.ConfigureMvc(mvcOptions =>
            {
                ConfigureAntiforgeryTokens(mvcOptions.AntiForgeryOptions);
                ConfigureViewEngines(mvcOptions.ViewEngines);
                
                // TODO: Decide whether to keep this. Also remove Microsoft.AspNet.Mvc.Xml.
                // Add the XML formatter, so that you can input and output types to XML. This is useful if you are 
                // building an API and want to have the choice. See
                // http://www.strathweb.com/2015/04/asp-net-mvc-6-formatters-xml-browser-requests/
                // mvcOptions.AddXmlDataContractSerializerFormatter();
            });

            services.ConfigureRouting(routeOptions =>
            {
                // routeOptions.AppendTrailingSlash = true;
                routeOptions.LowercaseUrls = true;
            });

            // Add your own custom services here e.g.

            // Singleton - Only one instance is ever created and returned.
            // services.AddSingleton<IDatabaseService, DatabaseService>();

            // Scoped - A new instance is created and returned for each request/response cycle.
            // services.AddScoped<IDatabaseService, DatabaseService>();

            // Transient - A new instance is created and returned each time.
            // services.AddTransient<IDatabaseService, DatabaseService>();
        }
        
        /// <summary>
        /// Configures the application and HTTP request pipeline. Configure is called after ConfigureServices is 
        /// called by the ASP.NET runtime.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="environment">The environment the application is running under. This can be Development, 
        /// Staging or Production by default.</param>
        /// <param name="loggerfactory">The logger factory.</param>
        public void Configure(
            IApplicationBuilder application, 
            IHostingEnvironment environment, 
            ILoggerFactory loggerfactory)
        {
            // Add the following to the request pipeline only in development environment.
            if (environment.IsEnvironment(EnvironmentName.Development))
            {
                // Add the console logger, which logs events to the Console, including errors and trace information.
                loggerfactory.AddConsole();

                // Browse to /runtimeinfo to see information about the runtime that is being used and the packages that 
                // are included in the application. See http://docs.asp.net/en/latest/fundamentals/diagnostics.html
                application.UseRuntimeInfoPage();

                // Allow updates to your files in Visual Studio to be shown in the browser. You can use the Refresh 
                // browser link button in the Visual Studio toolbar or Ctrl+Alt+Enter to refresh the browser.
                application.UseBrowserLink();

                // When an error occurs, displays a detailed error page with full diagnostic information. It is unsafe
                // to use this in production. See http://docs.asp.net/en/latest/fundamentals/diagnostics.html
                application.UseErrorPage(ErrorPageOptions.ShowAll);
            }
            else // Staging or Production environments.
            {
                // Add Error handling middleware which catches all application specific errors and send the request to 
                // the following path or controller action.
                application.UseErrorHandler("/error/internalservererror");
            }

            // Give the ASP.NET MVC Boilerplate Url Helper access to the HttpContext, so it can generate absolute URL's.
            Boilerplate.Web.Mvc.UrlHelperExtensions.Configure(
                application.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            
            // Add static files to the request pipeline e.g. hello.html or world.css.
            application.UseStaticFiles();

            // Add MVC to the request pipeline.
            application.UseMvc();

            application.Map("/throw", throwApp =>
            {
                throwApp.Run(context => { throw new Exception("Application Exception"); });
            });
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Configures the anti-forgery tokens. See 
        /// http://www.asp.net/mvc/overview/security/xsrfcsrf-prevention-in-aspnet-mvc-and-web-pages
        /// </summary>
        /// <param name="antiForgeryOptions">The anti-forgery token options.</param>
        private void ConfigureAntiforgeryTokens(AntiForgeryOptions antiForgeryOptions)
        {
            // Rename the Anti-Forgery cookie from "__RequestVerificationToken" to "f". This adds a little security 
            // through obscurity and also saves sending a few characters over the wire. 
            antiForgeryOptions.CookieName = "f";

            // Rename the form input name from "__RequestVerificationToken" to "f" for the same reason above e.g.
            // <input name="__RequestVerificationToken" type="hidden" value="..." />
            antiForgeryOptions.FormFieldName = "f";

            // If you have enabled SSL. Uncomment this line to ensure that the Anti-Forgery 
            // cookie requires SSL to be sent across the wire. 
            antiForgeryOptions.RequireSSL = true;
        }

        private void ConfigureViewEngines(IList<ViewEngineDescriptor> viewEngines)
        {
            // TODO: Remove Web Forms view engine.
        }

        #endregion
    }
}
