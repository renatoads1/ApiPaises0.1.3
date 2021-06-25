using ApiPaises013.Data;
using ApiPaises013.Servico;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
//para publicar no ubuntu
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;


namespace ApiPaises013
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            

            services.Configure<MongoDbrep>(
        Configuration.GetSection(nameof(MongoDbrep)));

            services.AddSingleton<IMongoDbrep>(sp =>
                sp.GetRequiredService<IOptions<MongoDbrep>>().Value);

            services.AddSingleton<PaisesService>(); 
            services.AddSingleton<RegionService>();
            services.AddSingleton<CityService>();

            services.AddControllers();
            
            //para publicar no ubuntu
            services.Configure<ForwardedHeadersOptions>(options => {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownProxies.Add(IPAddress.Parse("0.0.0.0"));
            });

            services.AddCors();
            //services.AddCors(options =>
            //{
            //    options.AddPolicy(name: MyAllowSpecificOrigins,
            //        builder =>
            //        {
            //            builder.WithOrigins("https://localhost:44396/ApiEndereco/pais",
            //                                "http://localhost:8100");
            //        });
            //});

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseForwardedHeaders();
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseForwardedHeaders();
                app.UseDeveloperExceptionPage();
            }

            //app.UseCors(MyAllowSpecificOrigins);
            // global cors policy
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
