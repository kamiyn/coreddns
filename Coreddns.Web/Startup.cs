using Coreddns.Core.Entities.DdnsDb;
using Coreddns.Core.Model;
using Coreddns.Core.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace Coreddns.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CoreDdnsOptions>(Configuration);
            services.AddMemoryCache();

            services.AddMvc();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "DdnsRegister", Version = "v1" });
            });

#if false
            // メモリデータベース
            {
                services.AddDbContext<DdnsDbContext>(optionsBuilder =>
                {
                    var options = optionsBuilder
                    .UseInMemoryDatabase(databaseName: "Startup")
                    .Options;
                    using (var context = new DdnsDbContext((DbContextOptions<DdnsDbContext>)options))
                    {
                        context.ddnshost.Add(new ddnshost
                        {
                            name = "test01",
                            hash = "hash01",
                            isvalid = true,
                        });
                        context.SaveChanges();
                    }
                });
            }
#endif

            services.AddDbContext<DdnsDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            // ログイン用のデータベースに利用する場合
            //services.AddIdentity<ApplicationUser, IdentityRole>()
            //    .AddEntityFrameworkStores<DdnsDbContext>()
            //    .AddDefaultTokenProviders();

            services.AddTransient<IEtcdRepostitory, EtcdRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                //{
                //    HotModuleReplacement = true
                //});
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DdnsRegister v1");
                });
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                //routes.MapSpaFallbackRoute(
                //    name: "spa-fallback",
                //    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
