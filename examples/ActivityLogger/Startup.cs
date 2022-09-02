namespace ActivityLogger;

internal class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        logger.LogInformation("Configure pipeline.");

        try
        {
            throw new Exception("This is an example of an exception occurring.");
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error occurred configuring the request pipeline.");
        }

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async context =>
            {
                logger.LogInformation("Hello world request.");
                await context.Response.WriteAsync("Hello World!");
            });
        });
    }
}