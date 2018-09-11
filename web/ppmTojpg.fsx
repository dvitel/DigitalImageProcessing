open System
open System.Drawing
open System.IO

let pgmTojpg(fileName: string, outfile: string) = 
    use file = new FileStream(fileName, FileMode.Open)
    use memory = new MemoryStream() 
    file.CopyTo(memory)
    memory.Flush()
    let bytes = memory.GetBuffer()
    printfn "BYTES: %d" bytes.Length
    let rec takeBytesWhileNewLine i = 
        if i >= bytes.Length then 
            None 
        else 
            if (bytes.[i] = byte '\r') && (i + 1 < bytes.Length) && (bytes.[i + 1] = byte '\n') then 
                Some(i, i+2)
            elif (bytes.[i] = byte '\n') then
                Some (i, i+1)
            else takeBytesWhileNewLine (i+1)
    let nextStart = 
        match takeBytesWhileNewLine 0 with 
        | None -> failwithf "[Bitmap.FromPGM] Empty file %s" fileName
        | Some (_, next) -> next
    printfn "PREAMBLE SKIPPED: %d" nextStart
    let width, height, nextStart = 
        match takeBytesWhileNewLine nextStart with 
        | None -> failwithf "[Bitmap.FromPGM] No sizes for image %s" fileName
        | Some (endIndex, ns) -> 
            let sizesLine = System.Text.Encoding.ASCII.GetString(bytes.[nextStart..endIndex-1])
            let sizes = sizesLine.Split([| " " |], StringSplitOptions.RemoveEmptyEntries)
            match Int32.TryParse(sizes.[0]), Int32.TryParse(sizes.[1]) with 
            | (true, width), (true, height) -> width, height, ns
            | _ -> failwithf "[Bitmap.FromPGM] Wrong string for sizes: %s in file %s" sizesLine fileName
    printfn "WIDTH: %d, HEIGHT: %d, NEXT: %d" width height nextStart
    let maxColor, nextStart = 
        match takeBytesWhileNewLine nextStart with 
        | None -> failwithf "[Bitmap.FromPGM] No max color in file %s" fileName         
        | Some (_, nextStart) -> 255, nextStart //TODO - go advanced 
    use bmp = new Bitmap(width, height)
    printfn "INDEX FOUND: %d" nextStart
    for x = 0 to width-1 do
        for y = 0 to height-1 do 
            let i = y * width + x + nextStart
            try             
            let byte = int(bytes.[i])
            bmp.SetPixel(x, y, Color.FromArgb(byte, byte, byte))
            with e -> 
                printfn "ERROR: (%d, %d), %d" x y i
                reraise()
    bmp.Save(outfile, Imaging.ImageFormat.Jpeg)

let inFile = """C:\Users\dvitel\Documents\University of South Florida\Lessons\Digital Image Processing\HW\DIPCODE_VC\iptool\input\baboon.pgm""";;
let outFile = """C:\Users\dvitel\Documents\University of South Florida\Lessons\Digital Image Processing\HW\DIPCODE_VC\iptool\input\baboon.jpg""";;
pgmTojpg(inFile, outFile);;

let inFile2 = """C:\Users\dvitel\Documents\University of South Florida\Lessons\Digital Image Processing\HW\DIPCODE_VC\iptool\output\baboon_add50.pgm""";;
let outFile2 = """C:\Users\dvitel\Documents\University of South Florida\Lessons\Digital Image Processing\HW\DIPCODE_VC\iptool\output\baboon_add50.jpg""";;
pgmTojpg(inFile2, outFile2);;

let inFile3 = """C:\Users\dvitel\Documents\University of South Florida\Lessons\Digital Image Processing\HW\DIPCODE_VC\iptool\output\baboon_bi2_100_200.pgm""";;
let outFile3 = """C:\Users\dvitel\Documents\University of South Florida\Lessons\Digital Image Processing\HW\DIPCODE_VC\iptool\output\baboon_bi2_100_200.jpg""";;
pgmTojpg(inFile3, outFile3);;

let inFile4 = """C:\Users\dvitel\Documents\University of South Florida\Lessons\Digital Image Processing\HW\DIPCODE_VC\iptool\output\baboon_bi100.pgm""";;
let outFile4 = """C:\Users\dvitel\Documents\University of South Florida\Lessons\Digital Image Processing\HW\DIPCODE_VC\iptool\output\baboon_bi100.jpg""";;
pgmTojpg(inFile4, outFile4);;

let inFile5 = """C:\Users\dvitel\Documents\University of South Florida\Lessons\Digital Image Processing\HW\DIPCODE_VC\iptool\output\baboon_s50.pgm""";;
let outFile5 = """C:\Users\dvitel\Documents\University of South Florida\Lessons\Digital Image Processing\HW\DIPCODE_VC\iptool\output\baboon_s50.jpg""";;
pgmTojpg(inFile5, outFile5);;

let inFile6 = """C:\Users\dvitel\Documents\University of South Florida\Lessons\Digital Image Processing\HW\DIPCODE_VC\iptool\output\baboon_s200.pgm""";;
let outFile6 = """C:\Users\dvitel\Documents\University of South Florida\Lessons\Digital Image Processing\HW\DIPCODE_VC\iptool\output\baboon_s200.jpg""";;
pgmTojpg(inFile6, outFile6);;

