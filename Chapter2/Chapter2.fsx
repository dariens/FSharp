
/// Split at string into words at spaces
let splitAtSpaces (text: string) =
    text.Split ' '
    |> Array.toList

/// Analyze a string for duplicate words.
let wordCount text =
    let words = splitAtSpaces text
    let numWords = words.Length
    let distinctWords = List.distinct words
    let numDups = numWords - distinctWords.Length
    (numWords, numDups)

///Analyze a string for duplicate words and display the results.
let showWordCount text =
    let numWords, numDups = wordCount text
    printfn "--> %d words in the text" numWords
    printfn "--> %d duplicate words" numDups

let powerOfFour n =
    let nSquared = n * n
    nSquared * nSquared

/// Outscoping a variable.
let powerOfFourPlusTwo n =
    printfn "%i" n
    let n = n * n
    printfn "%i" n
    let n = n * n
    printfn "%i" n
    let n = n + 2
    printfn "%i" n

let powerOfFourPlusTwoTimesSix n =
    let n3 =
        let n1 = n * n
        let n2 = n1 * n1
        n2 + 2
    let n4 = n3 * 6
    n4

/// USING DATA STRUCTURES:

["abc"; "ABC"]
   |> List.map (fun x -> x.ToUpper())
   |> List.distinct

let length (inp: 'T list) = inp.Length

let site1 = ("www.cnn.com", 10)
let site2 = ("news.bbc.com", 5)
let site3 = ("www.msnbc.com", 4)
let sites = (site1, site2, site3)

snd site1

let url, relevance = site1
let siteA, siteB, siteC = sites

let two = printfn "Hello World"; 1 + 1
let four = two + two

/// Using the .NET networking libraries from F#
open System.IO
open System.Net

/// Get the contents of the URL via a web request
let http (url: string) =
    let req = WebRequest.Create(url)
    let resp = req.GetResponse()
    let stream = resp.GetResponseStream()
    let reader = new StreamReader(stream)
    let html = reader.ReadToEnd()
    resp.Close()
    html

http "http://news.bbc.co.uk"

http "http://www.google.com"