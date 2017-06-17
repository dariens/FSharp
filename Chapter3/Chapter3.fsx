

let squareAndAdd a b = a * a + b

let squareAndAddf (a: float) b = a * a + b

let s = "Couldn't put Humpty"

//s.Length

//s.[13]

//s.[13..16]

//s = "Couldn't put Humpty"

let complete = "Couldn't put Humpty" + " " + "together again"

/// WORKING WITH CONDITIONALS: && and ||

let round x =
    if x >= 100 then 100
    elif x < 0 then 0
    else x

//round 105

let roundP x =
    match x with
    | _ when x >= 100 -> 100
    | _ when x < 0 -> 0
    | _ -> x

let round2 (x, y) =
   if x >= 100 || y >= 100 then 100, 100
   elif x < 0 || y < 0 then 0, 0
   else x, y

let rec factorial n = if n <=1 then 1 else n * factorial (n - 1)

//factorial 5


// example of List.length

let rec length l =
    match l with
    | [] -> 0
    | h :: t -> 1 + length t

//length [0;1;2;3;4;5]

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

let rec repeatFetch url n =
    if n > 0 then
        let html = http url
        printfn "fetched <<< %s >>> on iteration %d" html n
        repeatFetch url (n - 1)

//repeatFetch "http://news.bbc.co.uk" 3


// mutually recursive functions

let rec even n = (n = 0u) || odd(n - 1u)
and odd n = (n <> 0u) && even (n - 1u)

//even 9u
//odd 10u

/// LISTS

let oddPrimes = [3; 5; 7; 11]
let morePrimes = [13; 17]
let primes = 2 :: (oddPrimes @ morePrimes)

// Immutability
let people = ["Adam";"Dominic";"James"]

//"Chris" :: people

//people

let numbers = [1;2;3;4;5]

//List.filter (fun x -> x%2 = 0) numbers

//List.map (fun x -> x%2 = 0) numbers

let key = ["1";"2";"3";"4";"5"]

let zipped = List.zip key numbers

let keyUnzipped, numUnzipped = List.unzip zipped

let numArray = List.toArray numbers

let people2 =
    [("Adam", None);
     ("Eve", None);
     ("Cain", Some(Some "Adam", None));
     ("Abel", Some(Some "Adam", Some "Eve"))]

let fetch url =
    try Some (http url)
    with :? System.Net.WebException -> None

//match (fetch "http://www.nature.tsts") with
//    | Some text -> printfn "text = %s" text
//    | None -> printfn "**** no web page found"


/// Getting Started with Pattern matching

let isLikelySecretAgent url agent =
    match (url, agent) with
    | "http://www.control.org", 99 -> true
    | "http://www.control.org", 86 -> true
    | "http://www.kaos.org", _ -> true
    | _ -> false


let printFirst xs =
    match xs with
    | h :: t -> printfn "The first item in the list is %A" h
    | [] -> printfn "No items in the list"

let showParents (name, parents) =
    match parents with
    | Some (Some dad, Some mum) -> printfn "%s has father %s and mother %s" name dad mum
    | Some (Some dad, None) -> printfn "%s has father %s and no mother" name dad
    | Some (None, Some mum) -> printfn "%s has no father and mother %s" name mum
    | Some (None, None) -> printfn "%s has no parents" name
    | None -> printfn "%s has no parents!" name

//for person in people2 do showParents person

/// Matching on structured values

let highLow a b =
    match (a, b) with
    | ("lo", lo), ("hi", hi) -> (lo, hi)
    | ("hi", hi), ("lo", lo) -> (hi, lo)
    | _ -> failwith "expected both a high and a low value"

//highLow ("hi", 300) ("lo", 100)
//highLow ("lo", 100) ("hi", 300)

let sign x =
    match x with
    | _ when x < 0 -> -1
    | _ when x > 0 -> 1
    | _ -> 0

let getValue a =
    match a with
    | (("lo" | "low"), v) -> Some v
    | ("hi", v) | ("high", v) -> Some v
    | _ -> None



let values = [("lo",5);("low", 6);("hi",10); ("high", 11); ("hiLow", 13)]
               |> List.map getValue


let sites = ["http://www.bing.com";"http://www.google.com"]

let fetch2 url = (url, http url)


let fetchList = List.map fetch2 sites


/// USING FUNCTION VALUES

let primes2 = [2; 3; 5; 7]

let primeCubes = List.map (fun n -> n * n * n) primes2

let resultsOfFetch = List.map (fun url -> (url, http url)) sites

//List.map (fun (_,p) -> String.length p) resultsOfFetch

let delimiters = [| ' '; '\n'; '\t'; '<'; '>'; '='|]

let getWords (s: string) = s.Split delimiters

let getStats site =
    let url = "http://" + site
    let html = http url
    let hwords = html |> getWords
    let hrefs = html |> getWords |> Array.filter (fun s -> s = "href")
    (site, html.Length, hwords.Length, hrefs.Length)

let sites3 = ["www.bing.com"; "www.google.com"; "search.yahoo.com"]

//sites3 |> List.map getStats 

/// Using Fluent Notation on Collections

//[1;2;3] |> List.map (fun x -> x * x * x)

let google = http "http://www.google.com"

let countLinks2 http = http |> getWords |> Array.filter (fun s -> s = "href") |> Array.length

let countLinks = getWords >> Array.filter (fun s -> s = "href") >> Array.length

//google |> countLinks

/// Building Functions with Partial Application

let shift (dx, dy) (px, py) = (px + dx, py + dy)
let shiftRight = shift (1, 0)
let shiftUp = shift (0, 1)
let shiftLeft = shift (-1, 0)
let shiftDown = shift(0, -1)

/// Using Local Functions

open System.Drawing

let remap (r1:RectangleF) (r2: RectangleF) =
    let scalex = r2.Width / r1.Width
    let scaley = r2.Height / r1.Height
    let mapx x = r2.Left + (x - r1.Left) * scalex
    let mapy y = r2.Top + (y - r1.Top) * scaley
    let mapp (p:PointF) = PointF(mapx p.X, mapy p.Y)
    mapp

let rect1 = RectangleF(100.0f, 100.0f, 100.0f, 100.0f)
let rect2 = RectangleF(50.0f, 50.0f, 200.0f, 200.0f)

let mapp = remap rect1 rect2

//mapp (PointF(100.0f, 100.0f))
//mapp (PointF(150.0f, 150.0f))
//mapp (PointF(200.0f, 200.0f))

open System

let time f =
    let start = DateTime.Now
    let res = f()
    let finish = DateTime.Now
    (res, finish - start)

//time (fun () -> http "http://www.newscientist.com")

/// Using Object Methods as First-Class Functions

open System.IO

//[ "file1.txt"; "file2.txt"; "file3.sh" ]
//    |> List.map Path.GetExtension

let f = (Console.WriteLine : string -> unit)



