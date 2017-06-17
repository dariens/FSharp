/// Chapter 4

/// IMperative Looping and Iterating

/// Simple for Loops

#load "../Chapter3/Chapter3.fsx"
open Chapter3

let repeatFetch url n =
    for i = 1 to n do
       let html = http url
       printfn "fetched <<< %s >>>" html
    printfn "Done!"

/// Simple While Loops

open System

let loopUntilSaturday() =
    while (DateTime.Now.DayOfWeek <> DayOfWeek.Saturday) do
        printfn "Still working!"
    printfn "Saturday at last!"

/// More Iteration Loops over Sequences
for (b, pj) in [("Banana 1", false); ("Banana 2", true)] do
    if pj then
       printfn "%s is in pyjamas today!" b


open System.Text.RegularExpressions

for m in Regex.Matches("All the Pretty Horses", "[a-zA-Z]+") do
    printfn "res = %s" m.Value

type DiscreteEventCounter =
    { mutable Total : int;
      mutable Positive : int;
      Name : string }

let recordEvent (s: DiscreteEventCounter) isPositive =
    s.Total <- s.Total + 1
    if isPositive then s.Positive <- s.Positive + 1

let reportStatus (s: DiscreteEventCounter) =
    printfn "We have %d %s out of %d" s.Positive s.Name s.Total

let newCounter nm =
    { Total = 0;
      Positive = 0;
      Name = nm }

let longPageCounter = newCounter "long page(s)"

let fetch url =
    let page = http url
    recordEvent longPageCounter (page.Length > 10000)
    page

fetch "http://www.smh.com.au" |> ignore

reportStatus longPageCounter

fetch "http://www.theage.com.au" |> ignore

reportStatus longPageCounter

/// Using Mutable let Bindings
let mutable cell1 = 1

cell1 <- 3

cell1

let sum n m =
    let mutable res =  0
    for i = n to m do
        res <- res + i
    res

sum 2 6


/// Hiding Mutable Data

let generateStamp =
    let mutable count = 0
    (fun () -> count <- count + 1; count)

generateStamp()

/// Working with Arrays

let arr = [|1.0; 1.0; 1.0|]

arr.[1]

arr.[1] <- 3.0

arr

let newArr = Array.append arr [|2.0;2.0|]

let coppiedArr = Array.copy newArr

coppiedArr.[0] <- 3.0

newArr = coppiedArr

Array.iter (fun x -> printfn "%A" x) newArr


/// Generating and Slicing Arrays
let interestingArr = [|for i in 0 .. 5 -> (i, i *i)|]

interestingArr.[1..3]
interestingArr.[..3]
interestingArr.[3..]
interestingArr.[3..] <- [|(1,1);(1,1);(1,1)|]
interestingArr

let arr2 = interestingArr

arr2.[0] <- (1,1)

arr2

interestingArr  // Changed because arr2 and interestingArr reference is the same


/// Introducing the Imperative .NET Collections

/// Using Resizable Arrays

let names = new ResizeArray<string>()

for name in ["Claire"; "Sophie"; "Jane"] do
    names.Add(name)

names.Count

for name in names do
    printfn "%s" name

names.[0]

let squares = new ResizeArray<int>(seq {for i in 0..100 -> i * i})

for x in squares do
    printfn "Square: %d" x

/// Using Dictionaries

open System.Collections.Generic

let capitals = new Dictionary<string, string>(HashIdentity.Structural)

capitals.["USA"] <- "Washington D.C."
capitals.["Bangladesh"] <- "Dhaka"
capitals.ContainsKey("USA")

capitals.["Bangladesh"]

for kvp in capitals do
    printfn "%s has capital %s" kvp.Key kvp.Value

/// Using Dictionary's TryGetValue
open System.Collections.Generic

let lookupName nm (dict: Dictionary<string, string>) =
    let mutable res = ""
    let foundIt = dict.TryGetValue(nm, &res)
    if foundIt then res
    else failwithf "Didn't find %s" nm

lookupName "USA" capitals

lookupName "Australia" capitals

lookupName "Dhaka" capitals

let mutable res = ""
capitals.TryGetValue("Australia", &res)  // false
res  // null
capitals.TryGetValue("USA", &res)  // true
res  // "Washington D.C."
capitals.TryGetValue("Bangladesh", &res)  //true
res  // "Dhaka"

capitals.TryGetValue("Australia")
capitals.TryGetValue("USA")

/// Using Dictionaries with Compound Keys

open System.Collections.Generic

open FSharp.Collections

let sparseMap = new Dictionary<(int* int), float>()

sparseMap.[(0,2)] <- 4.0

sparseMap.[(1021,1847)] <- 9.0

sparseMap.Keys

sparseMap


/// Exceptions and Controlling Them

let req = System.Net.WebRequest.Create("not a URL")

if false then 3 else failwith "hit the wall"

if (System.DateTime.Now > failwith "not yet decided") then
    printfn "you've run out of time!"


/// Catching Exceptions
open Microsoft.FSharp.Core

try
    raise (System.InvalidOperationException ("it's just not my day"))
with
    :? System.InvalidOperationException -> printfn "caught!"

open System.IO

exception BlockedURL of string

let http (url : string) =
    try
        if url = "http://www.kaos.org"
        then raise (BlockedURL(url))
        let req = System.Net.WebRequest.Create(url)
        let resp = req.GetResponse()
        let stream = resp.GetResponseStream()
        let reader = new StreamReader(stream)
        let html = reader.ReadToEnd()
        html
    with
        | :? System.UriFormatException -> ""
        | :? System.Net.WebException -> ""
        | BlockedURL url -> sprintf "blocked! url = '%s'" url

http "invalid URL"
http "http://www.kaos.org"
http "http://www.facebook.com"

try 
    raise (new System.InvalidOperationException ("invalid operation"))
with
    err -> printfn "oops, msg = '%s'" err.Message

/// Using try ... finally

let httpViaTryFinally (url: string) =
    let req = System.Net.WebRequest.Create(url)
    let resp = req.GetResponse()
    try
        let stream = resp.GetResponseStream()
        let reader = new StreamReader(stream)
        let html = reader.ReadToEnd()
        html
    finally
        resp.Close()


/// Defining New Exception Types


let http2 url =
    if url = "http://www.kaos.org"
    then raise (BlockedURL(url))
    else http url

/// Having an Effect: Basic I/O
open System.IO

let tmpFile = Path.Combine(__SOURCE_DIRECTORY__, "temp.txt")
File.WriteAllLines(tmpFile, [|"This is a test file."; "It is easy to read."|])

let tmpFileText = File.ReadAllLines tmpFile

for line in tmpFileText do
    printfn "%A" line

seq { for line in File.ReadLines tmpFile do
          let words = line.Split[|' '|]
          if words.Length > 3 && words.[2] = "easy" then
              yield line}

let outp = File.CreateText "playlist.txt"

outp.WriteLine "Enchanted"
outp.WriteLine "Put your records on"
outp.Close()

let inp = File.OpenText("playlist.txt")

inp.ReadLine()

inp.ReadLine()

inp.Close()

/// USing System.Console

System.Console.WriteLine "Hello World"

let consoleString = System.Console.ReadLine()

consoleString



/// Combining Functional and Imperative Efficient Precomputation and Caching

/// Precomputation and Partial Application

let isWord (words : string list) =
    let wordTable = Set.ofList words
    fun w -> wordTable.Contains(w)

let isCapital = isWord ["London"; "Paris"; "Warsaw"; "Tokyo"]

isCapital "Paris"

isCapital "Darien"

// Version of isWord with HashSet
open System.Collections.Generic

let isWord2 (words : string list) =
    let wordTable = HashSet<string>(words)
    fun word -> wordTable.Contains word


/// Precomputation and Objects


open System


type NameLookupService =
    abstract Contains : string -> bool
    abstract StartsWithL : string -> bool

let buildSimpleNameLookup (words : string list) =
    let wordTable = HashSet<_>(words)
    {new NameLookupService with
        member t.Contains w = wordTable.Contains w
        member t.StartsWithL w = w.[0] = 'L'}

let capitalLookup2 = buildSimpleNameLookup ["London"; "Paris"; "Warsaw"; "Tokyo"]

capitalLookup2.Contains "Paris"
capitalLookup2.StartsWithL "London"
capitalLookup2.StartsWithL "Paris"

/// Memoizing Computations

let fibFast =
    let t = new System.Collections.Generic.Dictionary<int, int>()
    let rec fibCached n =
        match n with
        | n when t.ContainsKey n -> printfn "cached value"; t.[n]
        | n when n <= 2 -> printfn "0, 1, or 2"; 1
        | _ -> 
            printfn "greater than 2"
            let res = fibCached (n - 1) + fibCached (n - 2)
            t.Add (n, res)
            res
    fun n -> fibCached n


fibFast 0
fibFast 1
fibFast 2
fibFast 3
fibFast 4
fibFast 5
fibFast 5
fibFast 10
fibFast 11



let time f =
    let sw = System.Diagnostics.Stopwatch.StartNew()
    let res = f()
    let finish = sw.Stop()
    (res, sw.Elapsed.TotalMilliseconds |> sprintf "%f ms")

time(fun () -> fibFast 30)
time(fun () -> fibFast 30)

/// A generic memoization function

open System.Collections.Generic

let memoize (f : 'T -> 'U) =
    let t = new Dictionary<'T, 'U>(HashIdentity.Structural)
    fun n ->
        if t.ContainsKey n then t.[n]
        else
            let res = f n
            t.Add (n, res)
            res

let rec fibFast2 =
    memoize (fun n -> 
                 if n <=2 then 1
                 else fibFast (n-1) + fibFast (n-2))

time (fun () -> fibFast2 30)
time (fun () -> fibFast2 30)


/// A generic memoization service

open System.Collections.Generic

type Table<'T, 'U> =
    abstract Item : 'T -> 'U with get
    abstract Discard : unit -> unit

let memoizeAndPermitDiscard f =
    let lookasideTable = new Dictionary<_, _>(HashIdentity.Structural)
    { new Table<'T, 'U> with
        member t.Item
            with get(n) =
                if lookasideTable.ContainsKey(n) then
                    lookasideTable.[n]
                else
                    let res = f n
                    lookasideTable.Add(n, res)
                    res
        member t.Discard() =
            lookasideTable.Clear()}

#nowarn "40"
let rec fibFast3 =
    memoizeAndPermitDiscard (fun n ->
        printfn "computing fibFast %d" n
        if n<=2 then 1 else fibFast3.[n - 1] + fibFast3.[n - 2])

fibFast3.[3]
fibFast3.[3]
fibFast3.[30]

fibFast3.[30]

fibFast3.Discard()

fibFast3

fibFast3.[30]
fibFast3


/// Lazy Values

let sixty = lazy (30 + 30)

sixty.Force() * 2

let f x = lazy (x * x)

let fx = f 10

fx.Force()

let sixtyWithSideEffect = lazy (printfn "Hello World"; 30 + 30)

sixtyWithSideEffect.Force()

sixtyWithSideEffect.Force()

/// Mutable Reference Cells

let refCell = ref 1

printfn "%A" (refCell.Value, refCell.contents)

refCell := 3

printfn "%A" (refCell.Value, refCell.contents)

refCell.contents <- 5

printfn "%A" (refCell.Value, refCell.contents)

/// Combining Functional and IMperative: Functional Programming with Side Effects

/// Consider Replacing Mutable Locals and Loops with Recursion

let factorizeImperative n =
    let mutable factor1 = 1
    let mutable factor2 = n
    let mutable i = 2
    let mutable fin = false
    while (i < n && not fin) do
        if (n % i = 0) then
            factor1 <- i
            factor2 <- n/i
            fin <- true
        i <- i + 1
    if (factor1 = 1) then (n, None)
    else (n, Some (factor1, factor2))

let factorizeRecursive n =
    let rec find i =
        if i >=n then (n, None)
        elif (n % i = 0) then (n, Some(i, n / i))
        else find (i + 1)
    find 2

let getPrime x =
    match x with
    | (n, None) -> Some n
    | _ -> None

let primesToN n =
    [1..n]
        |> List.map factorizeRecursive
        |> List.map getPrime
        |> List.filter (fun x -> x = None = false && x = Some 1 = false)

primesToN 100

/// Separating Mutable Data Structures

open System.Collections.Generic

let divideIntoEquivalenceClasses keyf seq =
    // The dictionary to hold the equivalence classes
    let dict = new Dictionary<'key, ResizeArray<'T>>()
    // Build the groupings
    seq |> Seq.iter (fun v ->
        let key = keyf v
        let ok, prev = dict.TryGetValue(key)
        if ok then prev.Add(v)
        else let prev = new ResizeArray<'T>()
             dict.[key] <- prev
             prev.Add(v))
    // Return the sequence-sequences. Don't reveal the
    // internal collections: just reveal them as sequences
    dict |> Seq.map (fun group -> group.Key, Seq.readonly group.Value)

divideIntoEquivalenceClasses (fun n -> n % 4) [0..10]

divideIntoEquivalenceClasses (fun (n : string) -> n.Contains("A"))
                             ["A"; "ABC"; "DEF"; "HEY"; "HEYA"; "HEY"]


/// Avoid COmbining Imperative Programming and Laziness

open System.IO

let line1, line2 =
    let reader = new StreamReader(File.OpenRead("temp.txt"))
    let firstLine = reader.ReadLine()
    let secondLine = reader.ReadLine()
    reader.Close()
    firstLine, secondLine

let linesOfFile =
    seq { use reader = new StreamReader(File.OpenRead(__SOURCE_DIRECTORY__ + "/temp.txt"))
          while not reader.EndOfStream do
              yield reader.ReadLine() }

linesOfFile




