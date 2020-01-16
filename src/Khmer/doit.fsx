#load "../../.paket/load/netcoreapp3.0/main.group.fsx"

open NodaTime
open System.IO

[<Literal>]
let root_dir = __SOURCE_DIRECTORY__ + "/../.."

let sampleDate = File.ReadAllLines(root_dir + "/data/sample-date.txt").[0]

let dateTimePattern s =
    NodaTime.Text.OffsetDateTimePattern.CreateWithCurrentCulture(s)

let pattern = dateTimePattern "ddd MMM dd yyyy HH':'mm':'ss o<G>"

let zws = "\u200b"  // U+200B ZERO WIDTH SPACE
let zws2 = "\u200b\u200b"  // Some dates have double ZWSs in them

let zwsRegex = "\u200b{2,}" |> System.Text.RegularExpressions.Regex
let fixZws s = zwsRegex.Replace(s, zws)

let tryParseDate (s : string) =
    let parseResult =
        // All times in the data are in Indochina time, so we can simplify our data conversion by assuming that timezone
        // We also change "GMT+0700" to "+07:00" since that's the format that NodaTime expects for offsets
        s.Replace(zws2, " ").Replace(zws, " ").Replace("GMT + 0700 ( Indochina Time ) ", "+07:00")
        |> pattern.Parse
    if parseResult.Success then Some parseResult.Value else None

let args = fsi.CommandLineArgs |> Array.skip 1 // We don't care about the name of our script
let files = if args.Length > 0 then args else Array.singleton "data/alltext.txt"

type Article = {
    headline : string
    date : OffsetDateTime
    article : string
}

let invariantDateTimeFormat = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat

let outputArticleIfValid (maybeArticle : Article option) =
    match maybeArticle with
    | None -> ()
    | Some article ->
        printfn "\\ti %s" (article.headline |> fixZws)
        printfn "\\co %s" (article.date.ToString("uuuu'-'MM'-'dd'T'HH':'mm':'sso<m>", invariantDateTimeFormat))
        printfn "\\tx %s" (article.article |> fixZws)

// Don't warn about incomplete matches on "let [|headline; dateLine; article|] = lines" below,
// since Seq.windowed guarantees that the returned arrays will contain exactly 3 items each time
#nowarn "25"

for filename in files do
    let mutable previousArticle = None
    for lines in File.ReadLines(root_dir + "/" + filename) |> Seq.windowed 3 do
        let [|headline; dateLine; article|] = lines
        // If we see (date), (text), (date), that means the *previous* article was bad
        match tryParseDate headline, tryParseDate article with
        | Some date1, Some date2 ->
            // printfn "%A signals an invalid article, but %A is fine" date1 date2
            previousArticle <- None
        | _ ->
            ()
        match tryParseDate dateLine with
        | None -> ()
        | Some date ->
            // printfn "Good parse on %s" (date.ToString())
            outputArticleIfValid previousArticle
            previousArticle <- Some { headline = headline; date = date; article = article }
    // Don't forget to output the last article!
    outputArticleIfValid previousArticle

// There are 50 articles with a headline and no body; we'll just throw those out. Thing is, when you find a gap of 2 instead of 3, it's actually the *previous* article
// that was bad. The one you're currently looking at is fine. So that's a bit tricky.

// With this, we parse 85,378 articles. There were 85,428 total. We know 50 are bad, so the numbers match up.
