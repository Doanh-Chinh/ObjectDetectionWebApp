using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ML;
using OnnxObjectDetectionWeb.Infrastructure;
using OnnxObjectDetectionWeb.Services;
using OnnxObjectDetectionWeb.Utilities;
using OnnxObjectDetection;
using OnnxObjectDetectionWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace OnnxObjectDetectionWeb
{
    public class Startup
    {
        private readonly string _onnxModelFilePath;
        private readonly string _mlnetModelFilePath;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _onnxModelFilePath = CommonHelpers.GetAbsolutePath(Configuration["MLModel:OnnxModelFilePath"]);
            _mlnetModelFilePath = CommonHelpers.GetAbsolutePath(Configuration["MLModel:MLNETModelFilePath"]);

            var onnxModelConfigurator = new OnnxModelConfigurator(new TinyYoloModel(_onnxModelFilePath));  // initial model pipeline

            onnxModelConfigurator.SaveMLNetModel(_mlnetModelFilePath);  // save model into zip file
        }
        

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DbConnection")));

            services.AddScoped<IDetectionBoxRepository, DetectionBoxRepository>();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddControllers();
            services.AddRazorPages();

            services.AddPredictionEnginePool<ImageInputData, TinyYoloPrediction>().
               FromFile(_mlnetModelFilePath);

            services.AddTransient<IImageFileWriter, ImageFileWriter>();
            services.AddTransient<IObjectDetectionService, ObjectDetectionService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
