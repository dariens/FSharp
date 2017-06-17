seq {0 .. 2}
seq {-100.0 .. 100.0}
let mySeq = seq {1..1000000}
mySeq |> Seq.map (fun v -> v * 2)

/// Iterating a Sequence

let range = seq {0 .. 2 .. 6}

for i in range do printfn "i = %d" i

/// Transforming Sequences with Functions

range |> Seq.map (fun i -> (i, i * i))

/// Using Lazy Sequences from External Sources

open System
open System.IO

let rec allFiles dir =
    Seq.append
        (dir |> Directory.GetFiles)
        (dir |> Directory.GetDirectories |> Seq.map allFiles |> Seq.concat)

allFiles Environment.SystemDirectory


/// Using Sequence Expressions

let squares = seq { for i in 0 .. 10 -> (i, i * i) }

seq {for (i, iSquared) in squares -> (i, iSquared, i * iSquared)}

/// Enriching Sequence Expressions with Additional Logic

let checkerboardCoordinates n =
    seq {for row in 1 .. n do
            for col in 1 .. n do
                let sum = row + col
                if sum%2 = 0 then
                    yield (row, col)}

checkerboardCoordinates 3

let fileInfo dir =
    seq { for file in Directory.GetFiles dir do
              let creationTime = File.GetCreationTime file
              let lastAccessTime = File.GetLastAccessTime file
              yield (file, creationTime, lastAccessTime)}

fileInfo Environment.SystemDirectory

let rec allFilesInfo dir =
    seq {for file in Directory.GetFiles dir do
            yield file
         for subdir in Directory.GetDirectories dir do
             yield! allFiles subdir}

allFilesInfo Environment.SystemDirectory
         
/// More on Working with Sequences

// A table of people in our startup
let people =
    [("Amber", 28, "Design")
     ("Wendy", 35, "Events")
     ("Antonia", 40, "Sales")
     ("Petra", 31, "Design")
     ("Carlos", 34, "Marketing")]

// Extract information from the table of people

let namesOfPeopleStartingWithA =
    people
      |> Seq.filter (fun (name, _age, _dept) -> name.StartsWith "A")
      |> Seq.toList

let namesOfDesigners =
    people
    |> Seq.filter (fun (_name, _age, dept) -> dept = "Design")
    |> Seq.map (fun (name, _age, _dept) -> name)
    |> Seq.toList

/// Using Other Sequence Operators: Truncate and Sort

/// A random-number generator
let rand = System.Random();

/// An infinite sequence of numbers
let randomNumbers = seq {while true do yield rand.Next(100000)}

/// The first 10 random numbers, sorted

let firstTenRandomNumbers =
    randomNumbers
       |> Seq.truncate 10
       |> Seq.sort
       |> Seq.toList

/// the first 3000 even numbers, sorted
let firstThreeThousaandEvenNumberWithSquares =
    randomNumbers
        |> Seq.filter (fun i -> i % 2 = 0)
        |> Seq.truncate 3000
        |> Seq.sort 
        |> Seq.map (fun i -> i, i*i)
        |> Seq.toList

/// The first 10 random numbers, sorted by last digit
let firstTenRandomNumbersSortedByLastDigit =
    randomNumbers
        |> Seq.truncate 10
        |> Seq.sortBy (fun x -> x % 10)
        |> Seq.toList

/// Selecting Multiple Elements from Sequences

// Take the first 10 numbers and build a triangle
let triangleNumbers =
    [1 .. 3]
        |> Seq.collect (fun i -> [ 1 .. i] 
                                    |> List.map (fun i -> i * i))
        |> Seq.toList

let gameBoard =
    [ for i in 0 .. 7 do
        for j in 0 .. 7 do
            yield (i, j, rand.Next(10)) ]

let evenPositions =
    gameBoard
        |> Seq.choose(fun (i, j, v) ->
                             if v % 2 = 0 then Some (i, j)
                             else None)
        |> Seq.toList

/// Finding Elements and Indexes in Sequences
let firstElementScoringZero =
    gameBoard |> Seq.tryFind (fun (i, j, v) -> v = 0)

let firstPositionScoringZero =
    gameBoard |> Seq.tryPick (fun (i, j, v) -> 
                                if v = 0 then Some(i,j)
                                else None)

/// Grouping and Indexing Sequences

open System.Collections.Generic
let positionsGroupedByGameValue =
    gameBoard
      |> Seq.groupBy (fun (i, j, v) -> v)
      |> Seq.sortBy (fun (k, v) -> k)
      |> Seq.toList

let positionsIndexedByGameValue =
    gameBoard
        |> Seq.groupBy (fun (i, j, v) -> v)
        |> Seq.sortBy (fun (k, v) -> k)
        |> Seq.map (fun (k, v) -> (k, Seq.toList v))
        |> dict
    
let worstPostions = positionsIndexedByGameValue.[0]
let bestPostions = positionsIndexedByGameValue.[9]

/// Folding Sequences
List.fold (fun acc x -> List.append acc [x*x]) [] [4;5;6]

(0, [4;5;6]) ||> List.fold (fun acc x -> acc + x)

(0.0, [4.0;5.0;6.0]) ||> Seq.fold (fun acc x -> acc + x)

(0, [4;5;6]) ||> Seq.fold (+)

([4;5;6;3;5], System.Int32.MaxValue)
    ||> List.foldBack min

([(6, "three"); (5, "five")], System.Int32.MaxValue)
    ||> List.foldBack (fst >> min)

([(6, "Six"); (5, "Five")]) |> Seq.sumBy (fun tup -> fst tup)

/// Cleaning Up in Sequence Expresiions
open System.IO

let firstTwoLines file =
    seq { use s = File.OpenText(file)
          yield s.ReadLine()
          yield s.ReadLine() }

File.WriteAllLines("test1.txt", [|"First line"; "Second Line"|])

let twoLines () = firstTwoLines "test1.txt"

twoLines() |> Seq.iter (printfn "line = '%s'")

/// Expressing Operations Using Sequence Expressions

let triangleNumbers2 =
    [for i in 1 .. 10 do
        for j in 1 .. i do
            yield (i, j)]

let evenPostitions =
    [for (i,j,v) in gameBoard do
        if v % 2 = 0 then
            yield (i, j)]

/// Structure beyond Sequences: Domain Modeling

open System.Xml
open System.Drawing

type Scene = 
    | Ellipse of RectangleF
    | Rect of RectangleF
    | Composite of Scene list

    static member Circle(center:PointF, radius) =
        Ellipse(RectangleF(center.X-radius,center.Y-radius,
                           radius*2.0f, radius*2.0f))
    
    static member Square(left,top,side) =
        Rect(RectangleF(left,top,side,side))

// flatten to sequence of Ellipse and Rectangle nodes

let rec flatten scene =
    seq {match scene with
           | Composite scenes -> for x in scenes do yield! flatten x
           | Ellipse _ | Rect _ -> yield scene }

/// flatten, but accumulate result
let rec flattenAux scene acc =
    match scene with
    | Composite scenes -> List.foldBack flattenAux scenes acc
    | Ellipse _
    | Rect _ -> scene :: acc

let flatten2 scene = flattenAux scene [] |> Seq.ofList

/// do eager traversal using ResizeArray as accumultor

let flatten3 scene =
    let acc = new ResizeArray<_>()
    let rec flattenAux s =
        match s with
        | Composite scenes -> scenes |> List.iter flattenAux
        | Ellipse _ | Rect _ -> acc.Add s
    flattenAux scene
    Seq.readonly acc

/// Transforming Domain Models

/// mapping transformation rewriteing all leaf ellipses to rectangles

let rec rectanglesOnly scene =
   match scene with
   | Composite scenes -> Composite(scenes |> List.map rectanglesOnly)
   | Ellipse rect | Rect rect -> Rect rect

/// function that applies one functionto each leaf rectangle

let rec mapRects f scene =
    match scene with
    | Composite scenes -> Composite (scenes |> List.map (mapRects f))
    | Ellipse rect -> Ellipse (f rect)
    | Rect rect -> Rect (f rect)

let adjustAspectRatio scene =
    scene |> mapRects (fun r -> RectangleF.Inflate(r, 1.1f, 1.0f/ 1.1f))
    
/// Using On-Demand Computation with Domain Models

let xml = @"
<Composite>
    <File file = 'spots.xml'/>
    <File file = 'dots.xml'/>
</Composite>"

type Scene2 =
    | Ellipse of RectangleF
    | Rect of RectangleF
    | Composite of Scene list
    | Delay of Lazy<Scene>


/// Caching Properties in Domain Models
(*
type SceneWithCachedBoundingBox =
    | EllipseInfo of RectangleF
    | RectInfo of RectangleF
    | CompositeInfo of CompositeInfo
and CompositeInfo =
    { Scenes: SceneWithCachedBoundingBox list
      mutable BoundingBoxCache : RectangleF option }

    member x.BoundingBox =
        match x with
        | EllipseInfo rect | RectInfo rect -> rect
        | CompositeInfo info ->
            match info.cache with
            | Some v -> v
            | None ->
                let bbox =
                    info.Scenes
                    |> List.map (fun s -> s.BoundingBox)
                    |> List.reduce (fun r1 r2 -> RectangleF.Union(r1, r2))
                info.cache <- Some bbox
                bbox
*)


/// Memoizing Construction of Domain Model Nodes

type Prop =
    Prop of int

and PropRepr =
    | AndRepr of Prop * Prop
    | OrRepr of Prop * Prop
    | NotRepr of Prop
    | VarRepr of string
    | TrueRepr

open System.Collections.Generic

module PropOps =
    let uniqStamp = ref 0
    type internal PropTable() =
        let fwdTable = new Dictionary<PropRepr, Prop>(HashIdentity.Structural)
        let bwdTable = new Dictionary<int, PropRepr>(HashIdentity.Structural)
        member t.ToUnique repr =
            if fwdTable.ContainsKey repr then fwdTable.[repr]
            else let stamp = incr uniqStamp; !uniqStamp
                 let prop = Prop stamp
                 fwdTable.Add (repr, prop)
                 bwdTable.Add (stamp, repr)
                 prop
        member t.FromUnique (Prop stamp) =
            bwdTable.[stamp]


    let internal table = PropTable ()

    let And (p1, p2) = table.ToUnique (AndRepr (p1, p2))
    let Not p = table.ToUnique (NotRepr p)
    let Or (p1, p2) = table.ToUnique (OrRepr (p1, p2))
    let Var p = table.ToUnique (VarRepr p)
    let True = table.ToUnique TrueRepr
    let False = Not True

    let getRepr p = table.FromUnique p


module test =
    open PropOps

    True

    let prop = And (Var "x", Var "y")

    let repr = getRepr prop

    let prop2 = And (Var "x", Var "y")


/// Actie Patterns: Views for Structured Data

[<Struct>]
type Complex(r: float, i: float) =
    static member Polar(mag, phase) = Complex(mag * cos phase, mag * sin phase)
    member x.Magnitued = sqrt(r * r + i * i)
    member x.Phase = atan2 i r
    member x.RealPart = r
    member x.ImaginaryPart = i

let (|Rect|) (x : Complex) = (x.RealPart, x.ImaginaryPart)
let (|Polar|) (x: Complex) = (x.Magnitued, x.Phase)

let addViaRect a b =
    match a, b with
    | Rect (ar, ai), Rect (br, bi) -> Complex (ar + br, ai + bi)

let mulViaRect a b =
    match a, b with
    | Rect (ar, ai), Rect (br, bi) -> Complex (ar * br - ai * bi, ai * br + bi * ar)

let mulViaPolar a b =
    match a, b with
    | Polar (m, p), Polar (n, q) -> Complex.Polar (m * n, p + q)

fsi.AddPrinter (fun (c: Complex) -> sprintf "%gr + %gi" c.RealPart c.ImaginaryPart)

let c = Complex (3.0, 4.0)

match c with
| Rect (x, y) -> printfn "x = %g, y = %g" x y

match c with
| Polar (x, y) -> printfn "x = %g, y = %g" x y

addViaRect c c

mulViaRect c c

mulViaPolar c c

let (|Named|Array|Ptr|Param|) (typ : System.Type) =
    if typ.IsGenericType
    then Named(typ.GetGenericTypeDefinition(), typ.GetGenericArguments())
    elif typ.IsGenericParameter then Param(typ.GenericParameterPosition)
    elif not typ.HasElementType then Named(typ, [||])
    elif typ.IsArray then Array(typ.GetElementType(), typ.GetArrayRank())
    elif typ.IsByRef then Ptr(true, typ.GetElementType())
    elif typ.IsPointer then Ptr(false, typ.GetElementType())
    else failwith "MSDN says this can't happen"

open System
let rec formatType typ =
    match typ with
    | Named (con, [||]) -> sprintf "%s" con.Name
    | Named (con, args) -> sprintf "%s<%s>" con.Name (formatTypes args)
    | Array (arg, rank) -> sprintf "Array(%d,%s)" rank (formatType arg)
    | Ptr(true, arg) -> sprintf "%s&" (formatType arg)
    | Ptr(false, arg) -> sprintf "%s*" (formatType arg)
    | Param(pos) -> sprintf "!%d" pos
and formatTypes typs =
    String.Join(",", Array.map formatType typs)

let rec freeVarsAcc typ acc =
    match typ with
    | Array (arg, rank) -> freeVarsAcc arg acc
    | Ptr (_, arg) -> freeVarsAcc arg acc
    | Param _ -> (typ :: acc)
    | Named (con, args) -> Array.foldBack freeVarsAcc args acc

let freeVars typ = freeVarsAcc typ []

/// Defining Partial and Parameterized Active atterns

let (|MulThree|_|) inp = if inp % 3 = 0 then Some(inp/3) else None
let (|MulSeven|_|) inp = if inp % 7 = 0 then Some(inp/7) else None

let (|MulN|_|) n inp = if inp % n = 0 then Some(inp/n) else None



















