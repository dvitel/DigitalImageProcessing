module console.Native
open System
open System.Runtime.InteropServices

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
extern int gausFilter2D(IMG src, IMG tgt, ROI roi, float gs, int br)

[<System.Runtime.InteropServices.DllImport(@"dip.dll", EntryPoint="gausFilter1Dx2")>]
extern int gausFilter1Dx2(IMG src, IMG tgt, ROI roi, float gs, int br)


