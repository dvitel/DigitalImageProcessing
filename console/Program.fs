// Learn more about F# at http://fsharp.org

open System
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Configuration.Json
open System.Collections.Generic
open console.Native
open System.Drawing
open System.IO
open System.Diagnostics

[<AllowNullLiteralAttribute>]
type ROIConfig() = 
    member val x1 = 0 with get, set
    member val x2 = 0 with get, set 
    member val y1 = 0 with get, set 
    member val y2 = 0 with get, set
type OpConfig() = 
    member val id = "" with get, set 
    member val parameters: Dictionary<string, string> = null with get, set
    member val roi: ROIConfig = null with get, set

type ImageConfig() = 
    member val inputFile = "" with get, set
    member val outputFile = "" with get, set
    member val ops: OpConfig[] = null with get, set
    
    member x.Process(timer: Stopwatch) = async {
        match x.ops.Length with 
        | 0 -> 
            printfn "[Img %s] noop" x.inputFile
        | _ -> 
            // use! img = async {
            //     use fileStream = new FileStream(x.inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, true)
            //     use memoryStream = new MemoryStream() 
            //     do! Async.AwaitTask(fileStream.CopyToAsync(memoryStream))
            //     do! Async.AwaitTask(memoryStream.FlushAsync())
            //     memoryStream.Seek(0L, SeekOrigin.Begin) |> ignore
            //     return new Bitmap(memoryStream)
            //     }
            use img = new Bitmap(x.inputFile)
            use tgt = new Bitmap(img) //new Bitmap(img.Width, img.Height)
            let imgLock = img.LockBits(Rectangle(0, 0, img.Width, img.Height), Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format24bppRgb)
            let tgtLock = tgt.LockBits(Rectangle(0, 0, tgt.Width, tgt.Height), Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format24bppRgb)
            let imgInfo = 
                { 
                    ch = imgLock.Scan0
                    width = imgLock.Width
                    height = imgLock.Height
                    stride = imgLock.Stride
                }
            let tgtInfo = 
                { 
                    ch = tgtLock.Scan0
                    width = tgtLock.Width
                    height = tgtLock.Height
                    stride = tgtLock.Stride
                }         
            x.ops |> Array.iter(fun op ->                 
                let roi = 
                    if op.roi = null then 
                        {
                            x1 = 0
                            x2 = img.Width
                            y1 = 0
                            y2 = img.Height
                        }
                    else 
                        {
                            x1 = op.roi.x1
                            x2 = op.roi.x2
                            y1 = op.roi.y1
                            y2 = op.roi.y2
                        }
                timer.Restart()                          
                match op.id with 
                | "add" -> 
                    match Int32.TryParse(op.parameters.["value"]) with 
                    | (true, v) -> 
                        addROI(imgInfo, tgtInfo, roi, v)
                    | _ -> 
                        printfn "[Img %s] op %s requires parameter 'value'" x.inputFile op.id
                | "gray" -> 
                    let grayWay = op.parameters.["way"] 
                    let grayWay = 
                        match grayWay with 
                        | "redChannel" -> 0
                        | "greenChannel" -> 1
                        | "blueChannel" -> 2
                        | "L1" -> 3
                        | "L2" -> 4
                        | _ -> 4 
                    grayROI(imgInfo, tgtInfo, roi, grayWay)
                | "binarize" -> 
                    match Byte.TryParse(op.parameters.["threshold1"]), Byte.TryParse(op.parameters.["threshold2"]) with 
                    | (true, t1), (true, t2) -> 
                        binarize(imgInfo, tgtInfo, roi, t1, t2)
                    | _, _ -> 
                        printfn "[Img %s] op %s requires parameters 'threshold1', 'threshold2'" x.inputFile op.id
                | "gaus" -> 
                    match Double.TryParse(op.parameters.["gs"]), Int32.TryParse(op.parameters.["br"]) with 
                    | (true, gs), (true, br) -> 
                        let ws = gausFilter2D(imgInfo, tgtInfo, roi, gs, br)
                        printfn "[Img %s] window size is %d for %f" x.inputFile ws gs 
                    | (true, gs), _ -> 
                        let ws = gausFilter2D(imgInfo, tgtInfo, roi, gs, 0)
                        printfn "[Img %s] window size is %d for %f" x.inputFile ws gs
                    | _, _ ->
                        printfn "[Img %s] op %s requires parameter 'gs'" x.inputFile op.id
                | "gaus2" -> 
                    match Double.TryParse(op.parameters.["gs"]), Int32.TryParse(op.parameters.["br"]) with 
                    | (true, gs), (true, br) -> 
                        let ws = gausFilter1Dx2(imgInfo, tgtInfo, roi, gs, br)
                        printfn "[Img %s] window size is %d for %f" x.inputFile ws gs 
                    | (true, gs), _ -> 
                        let ws = gausFilter1Dx2(imgInfo, tgtInfo, roi, gs, 0)
                        printfn "[Img %s] window size is %d for %f" x.inputFile ws gs
                    | _, _ ->
                        printfn "[Img %s] op %s requires parameter 'gs'" x.inputFile op.id                        
                | op -> 
                    printfn "[Img %s] op %s is not supported yet" x.inputFile op
                timer.Stop()
                let ms = timer.ElapsedMilliseconds                
                printfn "[Img %s] op %s done, time %dms" x.inputFile op.id ms
            )                    
            img.UnlockBits(imgLock)
            tgt.UnlockBits(tgtLock)
            tgt.Save(x.outputFile, Imaging.ImageFormat.Jpeg)
            // do! async {
            //     use memoryStream = new MemoryStream()            
            //     let format = 
            //         match System.IO.Path.GetExtension(x.outputFile) with 
            //         | ".png" -> Imaging.ImageFormat.Png
            //         | _ ->  Imaging.ImageFormat.Jpeg
            //     tgt.Save(memoryStream, format)
            //     do! Async.AwaitTask(memoryStream.FlushAsync())
            //     memoryStream.Seek(0L, SeekOrigin.Begin) |> ignore
            //     use fileStream = new FileStream(x.outputFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 4096, true)
            //     do! Async.AwaitTask(memoryStream.CopyToAsync(fileStream))
            //     }
        }        

[<EntryPoint>]
let main argv =
    let config = 
        ConfigurationBuilder()
            .SetBasePath(System.IO.Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional=true)
            .Build()
    let imgs = config.GetSection("images").Get<ImageConfig[]>()
    printfn "Found %d images" imgs.Length
    let timer = new Stopwatch()
    imgs |> Array.fold(fun acc img -> 
        async {
            do! acc 
            printfn "Processing %s --> %s" img.inputFile img.outputFile
            do! img.Process(timer)
        }) (async.Return ())
        |> Async.RunSynchronously
    0 // return an integer exit code
