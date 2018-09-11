namespace web

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open web.Console

module Program =
    let exitCode = 0

    let BuildWebHost args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .Build()

    [<EntryPoint>]
    let main args =
        if args.Length >= 1 && args.[0] = "-c" then 
            printfn "[dip] Console mode is used"
            //console mode args.[1] should contains json file
            if args.Length >=2 then
                let p = Path.GetFullPath(Path.Combine(System.IO.Directory.GetCurrentDirectory(), args.[1]))
                printfn "Config file %s" p
                let config = 
                    ConfigurationBuilder()
                        .AddJsonFile(p, optional=true)
                        .Build()
                let imgs = config.GetSection("images").Get<ImageConfig[]>()
                
                printfn "Found %d images" imgs.Length
                let timer = System.Diagnostics.Stopwatch()
                imgs |> Array.iter(fun img -> 
                        printfn "[%s] --> %s" img.inputFile img.outputFile
                        img.Process(timer))
            else 
                printfn "[dip] No input config specified"
        else 
            BuildWebHost(args).Run()
        exitCode
