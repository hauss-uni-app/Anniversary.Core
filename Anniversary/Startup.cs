using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Anniversary.Common.Helper;
using Anniversary.Extensions;
using Anniversary.Infrastructure;
using Anniversary.Options;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Consul;
using DnsClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Resilience;
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;

namespace Anniversary
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;

        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddAuthentication("Bearer")
            //   .AddIdentityServerAuthentication(options =>
            //   {
            //       options.Authority = "http://localhost:5000";
            //       options.RequireHttpsMetadata = false;
            //        //options.ApiName = "api";//�����ô˲������������нӿ�ȫ��ʹ��Ȩ��
            //    });

            //Binding AppSetting ServiceDiscoveryOptions
            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));

            //AddSingleton IDnsQuery
            services.AddSingleton<IDnsQuery>(d =>
            {
                var serviceConfiguration = d.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
                return new LookupClient(serviceConfiguration.Consul.DnsEndpoint.ToIPEndPoint());
            });

            //AddSingleton IConsulClient
            services.AddSingleton<IConsulClient>(s => new ConsulClient(cfg =>
            {
                var servicesConfiguration = s.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
                if (!string.IsNullOrEmpty(servicesConfiguration.Consul.HttpEndpoint))
                {
                    cfg.Address = new Uri(servicesConfiguration.Consul.HttpEndpoint);
                }
            }));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //AddSingleton ResilienveClientFactory
            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ResilienceHttpClient>>();
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var retryCount = 5;
                var exceptionCountAllowedBeforeBreaking = 5;
                return new ResilienceClientFactory(logger, httpContextAccessor, retryCount, exceptionCountAllowedBeforeBreaking);
            });

            // AddSingleton IHttpClient
            services.AddSingleton<IHttpClient>(sp =>
            {
                return sp.GetRequiredService<ResilienceClientFactory>().GetResilienceHttpClient();
            });

            services.AddCorsSetup();

            services.AddSingleton(new Appsettings(Env.ContentRootPath));

            services.AddSqlsugarSetup();
            services.AddDbSetup();
            services.AddControllers();

            //services.AddWechatApiClient();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                //c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Anniversary API",
                    Description = "ASP.NET Core Web API",
                    //TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "��ICP��20028382��",
                        Email = string.Empty,
                        Url = new Uri("http://www.beian.miit.gov.cn/"),
                    },
                    //License = new OpenApiLicense
                    //{
                    //    Name = "��ICP��20028382��",
                    //    Url = new Uri("www.beian.miit.gov.cn"),
                    //}
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;
            //builder.RegisterType<AdvertisementServices>().As<IAdvertisementServices>();


            #region ���нӿڲ�ķ���ע��


            var servicesDllFile = Path.Combine(basePath, "Anniversary.Services.dll");
            var repositoryDllFile = Path.Combine(basePath, "Anniversary.Repository.dll");

            if (!(File.Exists(servicesDllFile) && File.Exists(repositoryDllFile)))
            {
                throw new Exception("Repository.dll��service.dll ��ʧ����Ϊ��Ŀ�����ˣ�������Ҫ��F6���룬��F5���У����� bin �ļ��У���������");
            }



            // AOP ���أ������Ҫ��ָ���Ĺ��ܣ�ֻ��Ҫ�� appsettigns.json ��Ӧ��Ӧ true ���С�
            //var cacheType = new List<Type>();
            //if (Appsettings.app(new string[] { "AppSettings", "RedisCachingAOP", "Enabled" }).ObjToBool())
            //{
            //    builder.RegisterType<BlogRedisCacheAOP>();
            //    cacheType.Add(typeof(BlogRedisCacheAOP));
            //}
            //if (Appsettings.app(new string[] { "AppSettings", "MemoryCachingAOP", "Enabled" }).ObjToBool())
            //{
            //    builder.RegisterType<BlogCacheAOP>();
            //    cacheType.Add(typeof(BlogCacheAOP));
            //}
            //if (Appsettings.app(new string[] { "AppSettings", "TranAOP", "Enabled" }).ObjToBool())
            //{
            //    builder.RegisterType<BlogTranAOP>();
            //    cacheType.Add(typeof(BlogTranAOP));
            //}
            //if (Appsettings.app(new string[] { "AppSettings", "LogAOP", "Enabled" }).ObjToBool())
            //{
            //    builder.RegisterType<BlogLogAOP>();
            //    cacheType.Add(typeof(BlogLogAOP));
            //}

            // ��ȡ Service.dll ���򼯷��񣬲�ע��
            var assemblysServices = Assembly.LoadFrom(servicesDllFile);
            builder.RegisterAssemblyTypes(assemblysServices)
                      .AsImplementedInterfaces()
                      .InstancePerDependency()
                      .EnableInterfaceInterceptors();//����Autofac.Extras.DynamicProxy;
                                                     //.InterceptedBy(cacheType.ToArray());//����������������б�����ע�ᡣ

            // ��ȡ Repository.dll ���򼯷��񣬲�ע��
            var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);
            builder.RegisterAssemblyTypes(assemblysRepository)
                   .AsImplementedInterfaces()
                   .InstancePerDependency();

            #endregion

            #region û�нӿڲ�ķ����ע��

            //��Ϊû�нӿڲ㣬���Բ���ʵ�ֽ��ֻ���� Load ������
            //ע�����ʹ��û�нӿڵķ��񣬲������ʹ�� AOP ���أ��ͱ�������Ϊ�鷽��
            //var assemblysServicesNoInterfaces = Assembly.Load("Blog.Core.Services");
            //builder.RegisterAssemblyTypes(assemblysServicesNoInterfaces);

            #endregion

            #region û�нӿڵĵ����� class ע��

            //ֻ��ע������е��鷽��
            //builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(Love)))
            //    .EnableClassInterceptors()
            //    .InterceptedBy(cacheType.ToArray());

            builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(IHttpContextAccessor)));
            #endregion

        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApplicationLifetime lifetime,
            IOptions<ServiceDiscoveryOptions> options, IConsulClient consul)
        {
            //DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
            //defaultFilesOptions.DefaultFileNames.Clear();
            //defaultFilesOptions.DefaultFileNames.Add("index.html");
            //app.UseDefaultFiles(defaultFilesOptions);
            //app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            lifetime.ApplicationStarted.Register(() =>
            {
                RegisterService(app, options, consul);
            });

            lifetime.ApplicationStopped.Register(() =>
            {
                DeRegisterService(app, options, consul);
            });

            app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });
            app.UseCors("LimitRequests");

            //app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }

        private void RegisterService(IApplicationBuilder app,
            IOptions<ServiceDiscoveryOptions> serviceOptions,
            IConsulClient consul)
        {
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(p => new Uri(p));

            foreach (var address in addresses)
            {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";

                var httpCheck = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                    Interval = TimeSpan.FromSeconds(30),
                    HTTP = new Uri(address, "HealthCheck").OriginalString
                };

                var registration = new AgentServiceRegistration()
                {
                    Check = httpCheck,
                    Address = address.Host,
                    ID = serviceId,
                    Name = serviceOptions.Value.ServiceName,
                    Port = address.Port
                };

                consul.Agent.ServiceRegister(registration).GetAwaiter().GetResult();
            }
        }

        private void DeRegisterService(IApplicationBuilder app, IOptions<ServiceDiscoveryOptions> serviceOptions, IConsulClient consul)
        {
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(p => new Uri(p));

            foreach (var address in addresses)
            {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";
                consul.Agent.ServiceDeregister(serviceId).GetAwaiter().GetResult();
            }
        }
    }
}
