namespace web.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open System.IO
open Microsoft.AspNetCore.Http
open System.Drawing
open System.Text
open Microsoft.Extensions.Caching.Memory
open System.Drawing.Imaging
open web.Native
open Newtonsoft.Json.Linq
open System.Diagnostics
open System.Diagnostics
open System.Diagnostics

type ImageData() = 
    member val sourceImage: IFormFile = null with get, set

type SessionState = {
    //mutable src: Bitmap
    //mutable tgt: Bitmap
    img: Bitmap
    tgt: Bitmap
}    

// type Op() = 
//     member val op = "" with get, set
//     member val x0 = int with get, set
//     member val y0 = int with get, set
//     member val w = int with get, set
//     member val h = int with get, set
//     member val p: IDictionary<string, string> = null with get, set

// type Ops() = 
//     member val ops: List<Op> = null with get, set    


[<Route("api/[controller]")>]
type ImageController (cache: IMemoryCache) =
    inherit Controller()

    member private this.DoNativeOp(img: Bitmap, tgtImg: Bitmap, sw: Stopwatch, f) = 
        if img = tgtImg then 
            let imgLock = img.LockBits(Rectangle(0, 0, img.Width, img.Height), Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format24bppRgb)
            let src = 
                { 
                    ch = imgLock.Scan0
                    width = imgLock.Width
                    height = imgLock.Height
                    stride = imgLock.Stride
                }
            sw.Restart()
            f src src
            sw.Stop()
            img.UnlockBits(imgLock)
            sw.ElapsedMilliseconds
        else         
            let imgLock = img.LockBits(Rectangle(0, 0, img.Width, img.Height), Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format24bppRgb)
            let tgtImgLock = tgtImg.LockBits(Rectangle(0, 0, tgtImg.Width, tgtImg.Height), Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format24bppRgb)        
            let src = 
                { 
                    ch = imgLock.Scan0
                    width = imgLock.Width
                    height = imgLock.Height
                    stride = imgLock.Stride
                }
            let tgt = 
                { 
                    ch = tgtImgLock.Scan0
                    width = tgtImgLock.Width
                    height = tgtImgLock.Height
                    stride = tgtImgLock.Stride
                }           
            sw.Restart() 
            f src tgt
            sw.Stop()
            img.UnlockBits(imgLock)
            tgtImg.UnlockBits(tgtImgLock)
            sw.ElapsedMilliseconds
    
    member private this.BuildResult (img: Bitmap) (timings: (string*int64) list) = 
        use memoryStream = new MemoryStream()
        img.Save(memoryStream, ImageFormat.Jpeg)
        let buffer = memoryStream.GetBuffer()
        let base64Image = System.Convert.ToBase64String(buffer)
        dict 
            [
                "mime", "image/jpg" :> obj
                "img", base64Image :> obj
                "timings", 
                    timings |> List.rev |> List.map(fun (name, tm) -> 
                        dict [ "name", name :> obj; "time", tm :> obj ]) :> obj
            ] 
    
    [<HttpPut>]    
    member this.DoOp([<FromBody>]ops: JArray) =       
        let sessionId = this.HttpContext.Session.GetString("id")  
        match cache.TryGetValue(sessionId) with 
        | (true, (:? SessionState as state)) when ops.Count = 0 -> 
            let img = new Bitmap(state.img)
            this.Json(this.BuildResult img []) :> ActionResult 
        | (true, (:? SessionState as state)) ->      
            let img = new Bitmap(state.img)
            let sw = Stopwatch()
            let tgt, timings =                    
                ops |> Seq.cast<JObject> |> Seq.fold(fun (img: Bitmap, timings) jobj ->                                        
                    let op = jobj.["op"].Value<string>()
                    let x0 = jobj.["x0"].Value<int>()
                    let y0 = jobj.["y0"].Value<int>()
                    let w = jobj.["w"].Value<int>()
                    let h = jobj.["h"].Value<int>()  
                    let p = jobj.["p"] :?> JObject
                    let roi = 
                        {
                            x1 = x0
                            x2 = x0+w
                            y1 = y0
                            y2 = y0+h
                        }                
                    match op with 
                    | "add" -> 
                        let v = p.["addV"].Value<int>()   
                        let tm =      
                            this.DoNativeOp(img, img, sw, fun src tgt -> 
                                web.Native.addROI(src, tgt, roi, v))  
                        img, (sprintf "brightness+%d" v, tm)::timings
                    | "gray" -> 
                        let way = p.["grayWay"].Value<int>()      
                        let tm = 
                            this.DoNativeOp(img, img, sw, fun src tgt -> 
                                web.Native.grayROI(src, tgt, roi, way))
                        img, (sprintf "graying by %d" way, tm)::timings
                    | "binarize" -> 
                        let t1 = p.["t1"].Value<byte>()
                        let t2 = p.["t2"].Value<byte>()
                        let tm = 
                            this.DoNativeOp(img, img, sw, fun src tgt -> 
                                web.Native.binarize(src, tgt, roi, t1, t2))
                        img, (sprintf "g-binarize t1 %d, t2 %d" t1 t2, tm)::timings
                    | "gaus" -> 
                        let gs = p.["gs"].Value<float32>()
                        let br = p.["br"].Value<int>()
                        let tgt = new Bitmap(img)
                        let tm = 
                            this.DoNativeOp(img, tgt, sw, fun src tgt -> 
                                web.Native.gausFilter2D(src, tgt, roi, gs, br) |> ignore)
                        img.Dispose()
                        tgt, (sprintf "gaus 2D gs %.2f, br %d" gs br, tm)::timings
                    | "gaus1D" -> 
                        let gs = p.["gs"].Value<float32>()
                        let br = p.["br"].Value<int>()
                        let tgt = new Bitmap(img)
                        let tm = 
                            this.DoNativeOp(img, tgt, sw, fun src tgt -> 
                                web.Native.gausFilter1Dx2(src, tgt, roi, gs, br) |> ignore)
                        img.Dispose()
                        tgt, (sprintf "gaus 1Dx2 gs %.2f, br %d" gs br, tm)::timings
                    | "binarizeC" -> 
                        let dist = p.["dist"].Value<int>()
                        let r = p.["r"].Value<byte>()
                        let g = p.["g"].Value<byte>()
                        let b = p.["b"].Value<byte>()
                        let tm = 
                            this.DoNativeOp(img, img, sw, fun src tgt -> 
                                web.Native.binarizeColor(src, tgt, roi, dist, r, g, b))                        
                        img, (sprintf "c-binarize d %d, rgb(%d,%d,%d)" dist r g b, tm)::timings                           
                    | _ -> img, timings) (img, [])
            state.tgt.Dispose()
            let state = 
                {
                    img = state.img
                    tgt = tgt
                }
            cache.Set(sessionId, state) |> ignore
            this.Json(this.BuildResult state.tgt timings) :> ActionResult            
        | _ -> 
            this.BadRequest() :> ActionResult

    [<HttpPost>]
    member this.Post(op:ImageData) =         
        if op.sourceImage = null then 
           this.BadRequest() :> ActionResult
        else 
            let sessionId = this.HttpContext.Session.GetString("id")
            match cache.TryGetValue(sessionId) with 
            | (true, (:? SessionState as sessionState)) -> 
                sessionState.img.Dispose()
                sessionState.tgt.Dispose()
                use stream = op.sourceImage.OpenReadStream()
                let img = new Bitmap(stream) 
                let state = 
                    {
                        img = img
                        tgt = new Bitmap(img)
                    }
                cache.Set(sessionId, state) |> ignore                    
                this.Json(this.BuildResult sessionState.tgt) :> ActionResult
            | (_, _) -> 
                use stream = op.sourceImage.OpenReadStream()
                let src = new Bitmap(stream) 
                let state = 
                    {
                        img = src
                        tgt = new Bitmap(src)
                    }
                cache.Set(sessionId, state) |> ignore
                this.Json(this.BuildResult state.tgt) :> ActionResult