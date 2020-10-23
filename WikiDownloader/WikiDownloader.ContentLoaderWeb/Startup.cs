using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using WikiDownloader.Abstractions.Services;
using WikiDownloader.ContentLoaderWeb.HostedServices;
using WikiDownloader.DAL.Mongo;
using WikiDownloader.Services;

namespace WikiDownloader.ContentLoaderWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private const string MongoDbConnString = "mongodb://localhost:27017/?ssl=false";

        private const string SimpleWikiUrl = "https://simple.wikipedia.org/w/index.php";
        private const string TinyWikiUrl = "https://tn.wikipedia.org/w/index.php";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddTransient<HttpClient>();
            services.AddTransient<IMongoClient>(context => new MongoClient(MongoDbConnString));
            services.AddTransient(context => new WikiPageDownloaderService(new Uri(SimpleWikiUrl)));
            services.AddTransient<IWikiDownloaderStorage, MongoWikiDownloadStorage>();
            services.AddHostedService<ContentDownloaderJob>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
