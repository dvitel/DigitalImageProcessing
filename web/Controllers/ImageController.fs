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

    member private this.DoNativeOp(img: Bitmap, tgtImg: Bitmap, f) = 
        if img = tgtImg then 
            let imgLock = img.LockBits(Rectangle(0, 0, img.Width, img.Height), Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format24bppRgb)
            let src = 
                { 
                    ch = imgLock.Scan0
                    width = imgLock.Width
                    height = imgLock.Height
                    stride = imgLock.Stride
                }
            f src src
            img.UnlockBits(imgLock)
        else         
            let imgLock = img.LockBits(Rectangle(0, 0, img.Width, img.Height), Imaging.ImageLockMode.ReadOnly, Imaging.PixelFormat.Format24bppRgb)
            let tgtImgLock = tgtImg.LockBits(Rectangle(0, 0, tgtImg.Width, tgtImg.Height), Imaging.ImageLockMode.WriteOnly, Imaging.PixelFormat.Format24bppRgb)        
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
            f src tgt
            img.UnlockBits(imgLock)
            tgtImg.UnlockBits(tgtImgLock)
    
    member private this.BuildResult(img: Bitmap) = 
        use memoryStream = new MemoryStream()
        img.Save(memoryStream, ImageFormat.Jpeg)
        let buffer = memoryStream.GetBuffer()
        let base64Image = System.Convert.ToBase64String(buffer)
        dict 
            [
                "mime", "image/jpg"
                "img", base64Image
            ] 
    
    [<HttpPut>]    
    member this.DoOp([<FromBody>]ops: JArray) =       
        let sessionId = this.HttpContext.Session.GetString("id")  
        match cache.TryGetValue(sessionId) with 
        | (true, (:? SessionState as state)) when ops.Count = 0 -> 
            this.Json(dict [ "ok", true :> obj ]) :> ActionResult
        | (true, (:? SessionState as state)) ->      
            let img = new Bitmap(state.img)
            let tgt =                    
                ops |> Seq.cast<JObject> |> Seq.fold(fun (img: Bitmap) jobj ->                                        
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
                        this.DoNativeOp(img, img, fun src tgt -> 
                            web.Native.addROI(src, tgt, roi, v))  
                        img                  
                    | "gray" -> 
                        let way = p.["grayWay"].Value<int>()      
                        this.DoNativeOp(img, img, fun src tgt -> 
                            web.Native.grayROI(src, tgt, roi, way))
                        img
                    | "binarize" -> 
                        let t1 = p.["t1"].Value<byte>()
                        let t2 = p.["t2"].Value<byte>()
                        this.DoNativeOp(img, img, fun src tgt -> 
                            web.Native.binarize(src, tgt, roi, t1, t2))
                        img  
                    | "gaus" -> 
                        let gs = p.["gs"].Value<float>()
                        let br = p.["br"].Value<int>()
                        let tgt = new Bitmap(img)
                        this.DoNativeOp(img, tgt, fun src tgt -> 
                            web.Native.gausFilter2D(src, tgt, roi, gs, br))
                        img.Dispose()
                        tgt
                    | _ -> img) img
            state.tgt.Dispose()
            let state = 
                {
                    img = state.img
                    tgt = tgt
                }
            cache.Set(sessionId, state) |> ignore
            this.Json(this.BuildResult state.tgt) :> ActionResult            
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