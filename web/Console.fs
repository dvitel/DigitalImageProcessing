namespace web.Console

open System
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Configuration.Json
open System.Collections.Generic
open web.Native
open System.Drawing
open System.IO
open System.Diagnostics
open web.Native

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
    
    member x.Process(timer: Stopwatch) =
        match x.ops.Length with 
        | 0 -> 
            printfn "[%s] noop" x.inputFile
        | _ -> 
            use img = new Bitmap(x.inputFile)
            //use tgt = new Bitmap(img) //new Bitmap(img.Width, img.Height)
            let tgt = 
                x.ops |> Array.fold(fun (img: Bitmap) op ->                 
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
                    match Op.Parse(op.id) with 
                    | Add -> 
                        match Int32.TryParse(op.parameters.GetValueOrDefault("value", "")) with 
                        | (true, v) ->
                            let ms =  
                                doNativeOp(img, img, timer, fun imgInfo tgtInfo ->
                                    addROI(imgInfo, tgtInfo, roi, v))
                            printfn "[%s] brightening+%d: %dms" x.inputFile v ms
                        | _ -> 
                            printfn "[%s] op %s requires parameter 'value'" x.inputFile op.id
                        img
                    | Gray -> 
                        let grayWayStr = op.parameters.GetValueOrDefault("way", "")
                        let grayWay, grayWayStr = 
                            match grayWayStr with 
                            | "redChannel" -> 0, grayWayStr
                            | "greenChannel" -> 1, grayWayStr
                            | "blueChannel" -> 2, grayWayStr
                            | "L1" -> 3, grayWayStr
                            | "L2" -> 4, grayWayStr
                            | _ -> 4, "L2"
                        let ms = 
                            doNativeOp(img, img, timer, fun imgInfo tgtInfo ->
                                grayROI(imgInfo, tgtInfo, roi, grayWay))
                        printfn "[%s] graying by %s: %dms" x.inputFile grayWayStr ms
                        img
                    | Binarize -> 
                        match Byte.TryParse(op.parameters.GetValueOrDefault("t1", "")), Byte.TryParse(op.parameters.GetValueOrDefault("t2", "")) with 
                        | (true, t1), (true, t2) -> 
                            let ms = 
                                doNativeOp(img, img, timer, fun imgInfo tgtInfo ->
                                    binarize(imgInfo, tgtInfo, roi, t1, t2))
                            printfn "[%s] g-binarize t1 %d, t2 %d: %dms" x.inputFile t1 t2 ms
                        | _, _ -> 
                            printfn "[%s] op %s requires parameters 't1', 't2' to be in [0,255]" x.inputFile op.id
                        img
                    | Gaus -> 
                        let call gs br = 
                            let mutable ws = 0
                            let tgt = new Bitmap(img)                            
                            let ms = 
                                doNativeOp(img, tgt, timer, fun imgInfo tgtInfo ->
                                    ws <- gausFilter2D(imgInfo, tgtInfo, roi, gs, br))
                            printfn "[%s] gaus 2D gs %.2f, br %d, ws %d: %dms" x.inputFile gs br ws ms
                            img.Dispose()                            
                            tgt                        
                        match Double.TryParse(op.parameters.GetValueOrDefault("gs", "")), Int32.TryParse(op.parameters.GetValueOrDefault("br", "")) with 
                        | (true, gs), (true, br) -> call gs br
                        | (true, gs), _ -> call gs 0
                        | _, _ ->
                            printfn "[%s] op %s requires parameter 'gs'" x.inputFile op.id
                            img
                    | Gaus1D -> 
                        let call gs br = 
                            let mutable ws = 0 
                            let tgt = new Bitmap(img)                            
                            let ms = 
                                doNativeOp(img, tgt, timer, fun imgInfo tgtInfo ->
                                    ws <- gausFilter1Dx2(imgInfo, tgtInfo, roi, gs, br))
                            printfn "[%s] gaus 1Dx2 gs %.2f, br %d, ws %d: %dms" x.inputFile gs br ws ms
                            img.Dispose()                            
                            tgt
                        match Double.TryParse(op.parameters.GetValueOrDefault("gs", "")), Int32.TryParse(op.parameters.GetValueOrDefault("br", "")) with 
                        | (true, gs), (true, br) -> call gs br
                        | (true, gs), _ -> call gs 0
                        | _, _ -> 
                            printfn "[%s] op %s requires parameter 'gs'" x.inputFile op.id    
                            img
                    | BinarizeC -> 
                        match Int32.TryParse(op.parameters.GetValueOrDefault("dist", "")), 
                            Byte.TryParse(op.parameters.GetValueOrDefault("r", "")), 
                            Byte.TryParse(op.parameters.GetValueOrDefault("g", "")),
                            Byte.TryParse(op.parameters.GetValueOrDefault("b", "")) with 
                        | (true, d), (true, r), (true, g), (true, b) -> 
                            let ms = 
                                doNativeOp(img, img, timer, fun img tgt -> 
                                    binarizeColor(img, tgt, roi, d, r, g, b))
                            printf "[%s] c-binarize d %d, rgb(%d,%d,%d): %dms" x.inputFile d r g b ms                                                   
                            img
                        | _, _, _, _ -> 
                            printfn "[%s] op %s requires parameters dist and rgb(r, g, b)" x.inputFile op.id
                            img
                    | Unknown op -> 
                        printfn "[%s] op %s is not supported yet" x.inputFile op
                        img
                ) img             
            tgt.Save(x.outputFile, Imaging.ImageFormat.Jpeg)