/// Chapter 5: Understanding Types in Functional Programming

/// Exploring Some Simple Type Definitions

/// Defining Type Abbreviations

type index = int
type flags = int64
type results = string * System.TimeSpan * int * int

/// Defining Record Types
open System

type Person =
    { Name : string
      DateOfBirth : DateTime}

//{Name = "Bill"; DateOfBirth = DateTime(1962, 09, 02)}

type PageStats =
    { Site : string
      Time : System.TimeSpan
      Length : int
      NumWords : int
      NumHRefs : int}

#load "../Chapter3/Chapter3.fsx"
open Chapter3

let stats site =
    let url = "http://" + site
    let html, t = time (fun () -> http url)
    let words = html |> getWords
    let hrefs = words |> Array.filter (fun s -> s = "href")
    { Site = site
      Time = t
      Length = html.Length
      NumWords = words.Length
      NumHRefs = hrefs.Length}

//stats "www.google.com"


/// Handling Non-Unique Record Field Names

type Company =
    { Name : string
      Address : string}

type Dot = {X : int; Y : int}
type Point = {X : float; Y : float}


let coords1 (p:Point) = (p.X, p.Y)

let coords2 (d:Dot) = (d.X, d.Y)

let dist p = sqrt (p.X * p.X + p.Y * p.Y)  // inferes Point since it is the last defined type that matches


type Animal =
    { Name : string
      DateOfBirth :DateTime}

// Without explicit type annotation this would be inferred as type Animal 
// Since Animal was the last definition that fits.

let anna = ({Name = "Anna"; DateOfBirth = new System.DateTime(1968, 07, 23)} : Person)


/// Cloning Records

type Point3D = {X: float; Y: float; Z: float}

let p1 = {X = 3.0; Y = 4.0; Z = 5.0}
let p2 = {p1 with Y = 0.0; Z = 0.0}

/// Defining Discriminated Unions

type Route = int
type Make = string
type Model = string
type Transport =
    | Car of Make * Model
    | Bicycle
    | Bus of Route

let ian = Car("BMW", "360")
let don = [Bicycle; Bus 8]
let peter = [Car("Ford", "Fiesta"); Bicycle; Bus 10]

let averageSpeed (tr : Transport) =
    match tr with
    | Car ("BMW", _) -> 60
    | Car _ -> 65
    | Bicycle -> 16
    | Bus 2 | Bus 3 -> 32
    | Bus _ -> 24

type Proposition =
    | True
    | And of Proposition * Proposition
    | Or of Proposition * Proposition
    | Not of Proposition

let rec eval (p : Proposition) =
    match p with
    | True -> true
    | And (p1, p2) -> eval p1 && eval p2
    | Or  (p1, p2) -> eval p1 || eval p2
    | Not (p1) -> not (eval p1)

//eval True
//eval (And (True, Not True))
//eval (Or (True, Not True))

type Tree<'T> =
    | Tree of 'T * Tree<'T> * Tree<'T>
    | Tip of 'T

let rec sizeOfTree tree =
    match tree with
    | Tree(_, l, r) -> 1 + sizeOfTree l + sizeOfTree r
    | Tip _ -> 1

let smallTree = Tree("1", Tree("2", Tip "a", Tip "b"), Tip "c")

//sizeOfTree smallTree

/// Using Discriminated Unions as Records

type Point3D2 = Vector3D of float * float * float

let origin = Vector3D(0., 0., 0.)
let unitX = Vector3D(1., 0., 0.)
let unitY = Vector3D(0., 1., 0.)
let unitZ = Vector3D(0., 0., 1.)

let length (Vector3D(dx, dy, dz)) = sqrt (dx * dx + dy * dy + dz * dz)

/// Defining Multiple Types Simultaneously
type Node =
    { Name : string
      Links : Link list}
and Link =
    | Drangling
    | Link of Node

/// Understanding Generics


type StringMap<'T> = Map<string, 'T>

type Projections<'T, 'U> = ('T -> 'U) * ('U -> 'T)

let rec map (f: 'T -> 'U) (l : 'T list) =
    match l with
    | h :: t -> f h :: map f t
    | [] -> []

//[1..3] |> map (fun x -> x * x)

/// Writing Generic Functions

let getFirst (a,b,c) = a

let mapPair f g (x, y) = (f x, g y)


/// Some Important Generic Functions

//compare 3 2
//compare 'a' 'b'

//("abc", "def") < ("abc", "xyz")

//compare [10;30] [10;20]

/// Generic Hashing


//hash 100

//hash "abc"

//hash (100, "abc")

//hash (100.0)

//hash ([1;2;3])

/// Generic Pretty-Printing

//sprintf "result = %A" ([1], [true])

let darien = ({Name = "Darien"; DateOfBirth = new System.DateTime(1991, 01, 14)} : Person)

//sprintf "result = %A" darien


/// Generic Boxing and Unboxing

//box 1
//box "abc"
let stringObj = box "abc"
//(unbox<string> stringObj)
//(unbox stringObj : string)

let testFunc = (fun x -> x * x)

let boxedTestFunc = box testFunc

let unBoxedTestFunc = unbox<int -> int> boxedTestFunc

//unBoxedTestFunc 5

/// Generic Binary Serialization via the .NET Libraries

open System.IO
open System.Runtime.Serialization.Formatters.Binary

let writeValue outputStream x = 
    let formatter = new BinaryFormatter()
    formatter.Serialize(outputStream, box x)

let readValue inputStream =
    let formatter = new BinaryFormatter()
    let res = formatter.Deserialize(inputStream)
    unbox res

let addresses =
    Map.ofList ["Jeff", "123 Main Street, Redmond, WA 98052"
                "Fred", "987 Pine Road, Phila., PA 19116"
                "Mary", "PO Box 112233, Palo Alto, CA 94301"]

let fsOut = new FileStream(__SOURCE_DIRECTORY__ + "/Data.dat", FileMode.Create)
//writeValue fsOut addresses
//fsOut.Close()

let fsIn = new FileStream(__SOURCE_DIRECTORY__ + "/Data.dat", FileMode.Open)
let res : Map<string, string> = readValue fsIn
//fsIn.Close()


/// Using System.Runtime.Serialization.Json
#r "System.Runtime.Serialization.dll"
open System.Runtime.Serialization.Json
open System.Runtime.Serialization
open System.Runtime.Serialization.Formatters.Binary

[<DataContract>]
type Joist =
    {
        [<field : DataMember>]
        Mark : string

        [<field : DataMember>]
        TCSize : string
    }

let joist = {Mark = "J1"; TCSize = "3028"}


//let jsonOut = new FileStream(__SOURCE_DIRECTORY__ + "/Data6.txt", FileMode.Create)
//let jsonSerializer = new DataContractJsonSerializer(typedefof<Joist>)
//jsonSerializer.WriteObject(jsonOut, joist)
//jsonOut.Close()

//let jsonIn = new FileStream(__SOURCE_DIRECTORY__ + "/Data6.txt", FileMode.Open)
//let newJoist = jsonSerializer.ReadObject jsonIn :?> Joist
//jsonIn.Close()

/// Using Newtonsoft.Json.dll

#r "packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
open Newtonsoft.Json

type Joist2 =
    { Mark : string
      TCSize : string}

let joist1 = {Mark = "J1"; TCSize = "3028"}
let joist2 = {Mark = "J2"; TCSize = "4050"}
let joists = [joist1;joist2]

let json = JsonConvert.SerializeObject(joists, Formatting.Indented)
//printfn "%A" json

type testUnion =
    | Car of string
    | Bus of string * int
    | Plane of string

let car = Car("BMW")
let bus = Bus("Bus", 5)
let plane = Plane("Plane")

let vehicles = [car; bus; plane]


let jsonCar = JsonConvert.SerializeObject(vehicles, Formatting.Indented)

let newJsonCar = jsonCar.Replace("BMW", "Mercedes")

let newCar = JsonConvert.DeserializeObject<testUnion list>(newJsonCar)

/// Using JsonConvert.DeserializeXMLNode to convert Json to XML
#r "System.Xml.Linq.dll"
open System.Xml.Linq
open System.Security.Cryptography

// Function to add object to beginning of json
let formatJson firstObject json =
    sprintf "{\"%s\": %s }" firstObject json

let formatedJsonCar = formatJson "TestUnion_List" newJsonCar

let node = JsonConvert.DeserializeXmlNode(formatedJsonCar, "root")
//node.Save(__SOURCE_DIRECTORY__ + "/testXML.xml")

open System.IO


let readLines (filePath:string) =
    use sr = new StreamReader (filePath)
    sr.ReadToEnd()
        

let xml = readLines (__SOURCE_DIRECTORY__ + "/testXML.xml")
//printfn "%A" xml



/// Making Things Generic


/// Generic Algorithms through Explicit Arguments

// Non-generic highest common factor
let rec hcf a b =
    if a = 0 then b
    elif a < b then hcf a (b - a)
    else hcf (a - b) b

// Generic highest common factor

let hcfGenericOld (zero, sub, lessThan) =
    let rec hcf a b =
        if a = zero then b
        elif lessThan a b then hcf a (sub b a)
        else hcf (sub a b) b
    hcf

let hcfIntOld = hcfGenericOld (0, (-), (<))
let hcfInt64Old = hcfGenericOld (0L, (-), (<))
let hcfBigIntOld = hcfGenericOld (0I, (-), (<))

//hcfIntOld 18 12
//hcfBigIntOld 1810287116162232383039576I 1239028178293092830480239032I

/// Generic Algorithms through Function Parameters

// Using concrete record type containing function values
type Numeric<'T> =
    {Zero : 'T
     Subtract : ('T -> 'T -> 'T)
     LessThan : ('T -> 'T -> bool)}

let intOps = {Zero = 0; Subtract = (-); LessThan = (<)}
let bigintOps = {Zero = 0I; Subtract = (-); LessThan = (<)}
let int64Ops = {Zero = 0L; Subtract = (-); LessThan = (<)}

let hcfGeneric (ops : Numeric<'T>) =
    let rec hcf a b =
        if a = ops.Zero then b
        elif ops.LessThan a b then hcf a (ops.Subtract b a)
        else hcf (ops.Subtract a b) b
    hcf

let hcfInt = hcfGeneric intOps
let hcfBigInt = hcfGeneric bigintOps

//hcfInt 18 12

// Interface-type definition

type INumeric<'T> =
    abstract Zero : 'T
    abstract Subtract : 'T * 'T -> 'T
    abstract LessThan : 'T * 'T -> bool

let intOpsInterface=
    {new INumeric<int> with
        member ops.Zero = 0
        member ops.Subtract(x, y) = x - y
        member ops.LessThan(x, y) = x < y}

let hcfGenericInterface (ops : INumeric<'T>) =
    let rec hcf a b =
        if a = ops.Zero then b
        elif ops.LessThan(a, b) then hcf a (ops.Subtract(b, a))
        else hcf (ops.Subtract(a, b)) b
    hcf

/// Generic Algorithms through Inlining

[<System.ObsoleteAttribute>]
let convertToFloat x = float x


let inline convertToFloatInline x = float x

let inline convertToFloatAndAdd x y = convertToFloatInline x + convertToFloatInline y

//convertToFloatAndAdd 1.0 "3"

/// Understanding Subtyping

/// Casting Up Statically

let xobj = (1 :> obj)

let sobj = ("abc" :> obj)

/// Casting Down Dynamically

let boxedObject = box "abc"

let downcastString = (boxedObject :?> string)

/// Performing Type Tests via Pattern Matching

type storage =
    | String of string
    | Int of int
    | Float of float
    | Unknown of string option

let checkObject (x: obj) =
    match x with
    | :? string as s -> String(s)
    | :? int as d   -> Int(d)
    | :? float as f -> Float(f)
    | _         -> Unknown(None)

let storageList = [checkObject "String"; checkObject 1; checkObject 1.0; checkObject '1']

let squareStorage storageValue =
    match storageValue with
    | String x -> None
    | Int x    -> Some (float x * float x)
    | Float x  -> Some (x * x)
    | Unknown x -> None

//storageList |> List.map squareStorage

/// Knowing When Upcasts Are Aplied Automatically

open System
open System.Net
open System.IO
let dispose (c: IDisposable) = c.Dispose()

let obj1 = new WebClient()
let obj2 = new HttpListener()

//dispose obj1
//dispose obj2 

open System
open System.IO
let textReader =
    if DateTime.Today.DayOfWeek = DayOfWeek.Monday
    then Console.In
    else (File.OpenText("input.txt") :> TextReader)

let getTextReader () : TextReader = (File.OpenText("input.txt") :> TextReader)

/// Flexible Types

open System

let disposeMany (cs : seq<#IDisposable>) =
    for c in cs do c.Dispose()

/// Troubleshooting Type-Inference Problems

/// Understanding the Value Restriction



let emptyList = []
let initialLists = ([], [2])
let listOfEmptyLists = [[]; []]
let makeArray () = Array.create 100 []

/// Working around the Value Restriction

// Ensure Generic Functions Have Explicit Arguments

let mapFirst inp = inp |> List.map (fun (x, y)-> x)

let printFstElements inp = inp |> List.map fst |> List.iter (printf "res = %d")


// Add a Unit Argument to Create a Generic Function

let empties () = Array.create 100 []

let intEmpties : int list [] = empties ()
let stringEmpties : int list [] = empties ()

//Add Explicit Type Arguments

let emptyLists<'T> : seq<'T list> = Seq.init 100 (fun _ -> [])

//Seq.length emptyLists

/// Understanding Generic Overloaded Operators

let threeTimes x = (x + x + x)
let sixTimesInt64 (x: int64) = threeTimes x + threeTimes x


