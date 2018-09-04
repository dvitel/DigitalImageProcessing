namespace web

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Mvc.Formatters

type HtmlOutputFormatter() as x  = 
    inherit StringOutputFormatter()
    do 
        x.SupportedMediaTypes.Add("text/html")

type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        // Add framework services.
        services
            .AddSession(fun options -> 
                options.Cookie.Name <- ".DIP.s"
                options.IdleTimeout <- TimeSpan.FromMinutes 15.
            )
            .AddMemoryCache()
            .AddMvc(fun options ->
                options.OutputFormatters.Add(HtmlOutputFormatter())
            ) |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
        app
            .UseSession()
            .UseStaticFiles()
            .UseMvc() |> ignore

    member val Configuration : IConfiguration = null with get, set