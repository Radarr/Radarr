using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Exceptions;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Host;
using NzbDrone.SignalR;
using Radarr.Api.V3.System;
using Radarr.Host.AccessControl;
using Radarr.Http;
using Radarr.Http.Authentication;
using Radarr.Http.ErrorManagement;
using Radarr.Http.Frontend;
using Radarr.Http.Middleware;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Radarr.Host
{
    public class WebHostController : IHostController
    {
        private readonly IContainer _container;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IFirewallAdapter _firewallAdapter;
        private readonly RadarrErrorPipeline _errorHandler;
        private readonly Logger _logger;
        private IWebHost _host;

        public WebHostController(IContainer container,
                                 IRuntimeInfo runtimeInfo,
                                 IConfigFileProvider configFileProvider,
                                 IFirewallAdapter firewallAdapter,
                                 RadarrErrorPipeline errorHandler,
                                 Logger logger)
        {
            _container = container;
            _runtimeInfo = runtimeInfo;
            _configFileProvider = configFileProvider;
            _firewallAdapter = firewallAdapter;
            _errorHandler = errorHandler;
            _logger = logger;
        }

        public void StartServer()
        {
            if (OsInfo.IsWindows)
            {
                if (_runtimeInfo.IsAdmin)
                {
                    _firewallAdapter.MakeAccessible();
                }
            }

            var bindAddress = _configFileProvider.BindAddress;
            var enableSsl = _configFileProvider.EnableSsl;
            var sslCertPath = _configFileProvider.SslCertPath;

            var urls = new List<string>();

            urls.Add(BuildUrl("http", bindAddress, _configFileProvider.Port));

            if (enableSsl && sslCertPath.IsNotNullOrWhiteSpace())
            {
                urls.Add(BuildUrl("https", bindAddress, _configFileProvider.SslPort));
            }

            _host = new WebHostBuilder()
                .UseUrls(urls.ToArray())
                .UseKestrel(options =>
                {
                    if (enableSsl && sslCertPath.IsNotNullOrWhiteSpace())
                    {
                        options.ConfigureHttpsDefaults(configureOptions =>
                        {
                            X509Certificate2 certificate;

                            try
                            {
                                certificate = new X509Certificate2(sslCertPath, _configFileProvider.SslCertPassword, X509KeyStorageFlags.DefaultKeySet);
                            }
                            catch (CryptographicException ex)
                            {
                                if (ex.HResult == 0x2 || ex.HResult == 0x2006D080)
                                {
                                    throw new RadarrStartupException(ex, $"The SSL certificate file {sslCertPath} does not exist");
                                }

                                throw new RadarrStartupException(ex);
                            }

                            configureOptions.ServerCertificate = certificate;
                        });
                    }
                })
                .ConfigureKestrel(serverOptions =>
                {
                    serverOptions.AllowSynchronousIO = true;
                    serverOptions.Limits.MaxRequestBodySize = null;
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddProvider(new NLogLoggerProvider());
                    logging.SetMinimumLevel(LogLevel.Warning);
                })
                .ConfigureServices(services =>
                {
                    // So that we can resolve containers with our TinyIoC services
                    services.AddSingleton(_container);
                    services.AddSingleton<IControllerActivator, ControllerActivator>();

                    // Bits used in our custom middleware
                    services.AddSingleton(_container.Resolve<RadarrErrorPipeline>());
                    services.AddSingleton(_container.Resolve<ICacheableSpecification>());

                    // Used in authentication
                    services.AddSingleton(_container.Resolve<IAuthenticationService>());

                    services.AddRouting(options => options.LowercaseUrls = true);

                    services.AddResponseCompression();

                    services.AddCors(options =>
                    {
                        options.AddPolicy(VersionedApiControllerAttribute.API_CORS_POLICY,
                            builder =>
                            builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());

                        options.AddPolicy("AllowGet",
                            builder =>
                            builder.AllowAnyOrigin()
                            .WithMethods("GET", "OPTIONS")
                            .AllowAnyHeader());
                    });

                    services
                    .AddControllers(options =>
                    {
                        options.ReturnHttpNotAcceptable = true;
                    })
                    .AddApplicationPart(typeof(SystemController).Assembly)
                    .AddApplicationPart(typeof(StaticResourceController).Assembly)
                    .AddJsonOptions(options =>
                    {
                        STJson.ApplySerializerSettings(options.JsonSerializerOptions);
                    });

                    services
                    .AddSignalR()
                    .AddJsonProtocol(options =>
                    {
                        options.PayloadSerializerOptions = STJson.GetSerializerSettings();
                    });

                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy("UI", policy =>
                        {
                            policy.AuthenticationSchemes.Add(_configFileProvider.AuthenticationMethod.ToString());
                            policy.RequireAuthenticatedUser();
                        });

                        options.AddPolicy("SignalR", policy =>
                        {
                            policy.AuthenticationSchemes.Add("SignalR");
                            policy.RequireAuthenticatedUser();
                        });

                        // Require auth on everything except those marked [AllowAnonymous]
                        options.DefaultPolicy = new AuthorizationPolicyBuilder("API")
                        .RequireAuthenticatedUser()
                        .Build();
                    });

                    services.AddAppAuthentication(_configFileProvider);
                })
                .Configure(app =>
                {
                    app.UseMiddleware<LoggingMiddleware>();
                    app.UsePathBase(new PathString(_configFileProvider.UrlBase));
                    app.UseExceptionHandler(new ExceptionHandlerOptions
                    {
                        AllowStatusCode404Response = true,
                        ExceptionHandler = _errorHandler.HandleException
                    });

                    app.UseRouting();
                    app.UseCors();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseResponseCompression();
                    app.Properties["host.AppName"] = BuildInfo.AppName;

                    app.UseMiddleware<VersionMiddleware>();
                    app.UseMiddleware<UrlBaseMiddleware>(_configFileProvider.UrlBase);
                    app.UseMiddleware<CacheHeaderMiddleware>();
                    app.UseMiddleware<IfModifiedMiddleware>();

                    app.Use((context, next) =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api/v1/command", StringComparison.CurrentCultureIgnoreCase))
                        {
                            context.Request.EnableBuffering();
                        }

                        return next();
                    });

                    app.UseWebSockets();

                    app.UseEndpoints(x =>
                    {
                        x.MapHub<MessageHub>("/signalr/messages").RequireAuthorization("SignalR");
                        x.MapControllers();
                    });

                    // This is a side effect of haing multiple IoC containers, TinyIoC and whatever
                    // Kestrel/SignalR is using. Ideally we'd have one IoC container, but that's non-trivial with TinyIoC
                    // TODO: Use a single IoC container if supported for TinyIoC or if we switch to another system (ie Autofac).
                    _container.Register(app.ApplicationServices);
                    _container.Register(app.ApplicationServices.GetService<IHubContext<MessageHub>>());
                    _container.Register(app.ApplicationServices.GetService<IActionDescriptorCollectionProvider>());
                    _container.Register(app.ApplicationServices.GetService<EndpointDataSource>());
                    _container.Register(app.ApplicationServices.GetService<DfaGraphWriter>());
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .Build();

            _logger.Info("Listening on the following URLs:");

            foreach (var url in urls)
            {
                _logger.Info("  {0}", url);
            }

            _host.Start();
        }

        public async void StopServer()
        {
            _logger.Info("Attempting to stop OWIN host");

            await _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();
            _host = null;

            _logger.Info("Host has stopped");
        }

        private string BuildUrl(string scheme, string bindAddress, int port)
        {
            return $"{scheme}://{bindAddress}:{port}";
        }
    }
}
