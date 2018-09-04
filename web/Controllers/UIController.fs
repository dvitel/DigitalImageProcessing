namespace web.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Http
open System.IO

[<Route("api/[controller]")>]
type UIController () =
    inherit Controller()

    [<HttpGet>]
    [<Produces("text/html")>]
    member this.Get(): Task<string> =
        async {
            this.HttpContext.Session.SetString("id", Guid.NewGuid().ToString())
            let! fileContent = Async.AwaitTask(File.ReadAllTextAsync(System.IO.Path.Combine("wwwroot", "index.html")))
            return fileContent
        } |> Async.StartAsTask