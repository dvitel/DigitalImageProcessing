module web.Native
open System
open System.Runtime.InteropServices
open System.Drawing
open System.Diagnostics

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>] 
type IMG = {
    ch: IntPtr
    width: int
    height: int
    stride: int
}

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>] 
type ROI = {
    x1: int
    x2: int
    y1: int
    y2: int
}
[<System.Runtime.InteropServices.DllImport(@"dip.dll", EntryPoint="add")>]
extern void add(IMG src, IMG tgt, int v)

[<System.Runtime.InteropServices.DllImport(@"dip.dll", EntryPoint="addROI")>]
extern void addROI(IMG src, IMG tgt, ROI roi, int v)

[<System.Runtime.InteropServices.DllImport(@"dip.dll", EntryPoint="grayROI")>]
extern void grayROI(IMG src, IMG tgt, ROI roi, int grayWay)

[<System.Runtime.InteropServices.DllImport(@"dip.dll", EntryPoint="gray")>]
extern void gray(IMG src, IMG tgt, int grayWay)

[<System.Runtime.InteropServices.DllImport(@"dip.dll", EntryPoint="binarize")>]
extern void binarize(IMG src, IMG tgt, ROI roi, byte t1, byte t2)

[<System.Runtime.InteropServices.DllImport(@"dip.dll", EntryPoint="gausFilter2D")>]
extern int gausFilter2D(IMG src, IMG tgt, ROI roi, float32 gs, int br)

[<System.Runtime.InteropServices.DllImport(@"dip.dll", EntryPoint="gausFilter2D")>]
extern int gausFilter1Dx2(IMG src, IMG tgt, ROI roi, float32 gs, int br)

[<System.Runtime.InteropServices.DllImport(@"dip.dll", EntryPoint="binarizeColor")>]
extern void binarizeColor(IMG src, IMG tgt, ROI roi, int dist, byte r, byte g, byte b)

let doNativeOp(img: Bitmap, tgtImg: Bitmap, sw: Stopwatch, f) = 
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

type Op = 
    | Add 
    | Gray 
    | Binarize
    | Gaus
    | Gaus1D
    | BinarizeC
    | Unknown of string
with 
    static member Parse(str: string) = 
        match str with 
        | "add" -> Add
        | "gray" -> Gray
        | "binarize" -> Binarize
        | "gaus" -> Gaus
        | "gaus1D" -> Gaus1D
        | "binarizeC" -> BinarizeC
        | _ -> Unknown str