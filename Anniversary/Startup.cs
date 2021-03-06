using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Anniversary.Common.Helper;
using Anniversary.Extensions;
using Anniversary.OuterClient.Extensions;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

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
            services.AddCorsSetup();

            services.AddSingleton(new Appsettings(Env.ContentRootPath));

            services.AddSqlsugarSetup();
            services.AddDbSetup();
            services.AddControllers();

            services.AddWechatApiClient();

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
                        Name = "粤ICP备20028382号",
                        Email = string.Empty,
                        Url = new Uri("http://www.beian.miit.gov.cn/"),
                    },
                    //License = new OpenApiLicense
                    //{
                    //    Name = "粤ICP备20028382号",
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


            #region 带有接口层的服务注入


            var servicesDllFile = Path.Combine(basePath, "Anniversary.Services.dll");
            var repositoryDllFile = Path.Combine(basePath, "Anniversary.Repository.dll");

            if (!(File.Exists(servicesDllFile) && File.Exists(repositoryDllFile)))
            {
                throw new Exception("Repository.dll和service.dll 丢失，因为项目解耦了，所以需要先F6编译，再F5运行，请检查 bin 文件夹，并拷贝。");
            }



            // AOP 开关，如果想要打开指定的功能，只需要在 appsettigns.json 对应对应 true 就行。
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

            // 获取 Service.dll 程序集服务，并注册
            var assemblysServices = Assembly.LoadFrom(servicesDllFile);
            builder.RegisterAssemblyTypes(assemblysServices)
                      .AsImplementedInterfaces()
                      .InstancePerDependency()
                      .EnableInterfaceInterceptors();//引用Autofac.Extras.DynamicProxy;
                                                     //.InterceptedBy(cacheType.ToArray());//允许将拦截器服务的列表分配给注册。

            // 获取 Repository.dll 程序集服务，并注册
            var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);
            builder.RegisterAssemblyTypes(assemblysRepository)
                   .AsImplementedInterfaces()
                   .InstancePerDependency();

            #endregion

            #region 没有接口层的服务层注入

            //因为没有接口层，所以不能实现解耦，只能用 Load 方法。
            //注意如果使用没有接口的服务，并想对其使用 AOP 拦截，就必须设置为虚方法
            //var assemblysServicesNoInterfaces = Assembly.Load("Blog.Core.Services");
            //builder.RegisterAssemblyTypes(assemblysServicesNoInterfaces);

            #endregion

            #region 没有接口的单独类 class 注入

            //只能注入该类中的虚方法
            //builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(Love)))
            //    .EnableClassInterceptors()
            //    .InterceptedBy(cacheType.ToArray());

            #endregion

        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
            defaultFilesOptions.DefaultFileNames.Clear();
            defaultFilesOptions.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(defaultFilesOptions);
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            //    c.RoutePrefix = string.Empty;
            //});
            app.UseCors("LimitRequests");
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }


    }
}
