let SPLIT_INTO = 1000

let LINE_SPLITS = SPLIT_INTO * 3

let args = fsi.CommandLineArgs |> Array.skip 1 // We don't care about the name of our script
let filename = if args.Length > 0 then args.[0] else "all-output.txt"

let splitLines (lines : string seq) =
    let mkFile n = sprintf "split-output-%03d.sfm" n |> System.IO.File.CreateText
    let mutable outputFileCount = 0
    let mutable outputFile : System.IO.StreamWriter option = None

    for lineNum, line in lines |> Seq.mapi (fun idx line -> idx,line) do
        if lineNum % LINE_SPLITS = 0 then
            match outputFile with
            | Some file -> file.Close()
            | None -> ()
            outputFileCount <- outputFileCount + 1
            outputFile <- Some (mkFile outputFileCount)
        outputFile.Value.WriteLine line
    match outputFile with
    | Some file -> file.Close()  // Last file msut be closed by hand: loop won't do it
    | None -> ()

System.IO.File.ReadLines(filename) |> splitLines
