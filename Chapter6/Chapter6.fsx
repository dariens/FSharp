///Chapter 6 : Programming with Objects

///Getting Stated with Objects and Members

/// Two-dimensional vectors
type Vector2DOld =
    {DX : float; DY : float}
    /// Get the length of the vector
    member v.Length = sqrt(v.DX * v.DX + v.DY * v.DY)
    /// Return a vector scaled by the given factor
    member v.Scale k = {DX = k*v.DX; DY = k * v.DY}
    /// Return a vector shifted by the given delta in the X coordinate
    member v.ShiftX x = { v with DX = v.DX + x }
    /// Return a vector shifted by the given delta in the Y Coordinate
    member v.ShiftY y = { v with DY = v.DY + y }
    /// Return a vector shifted by the given distance in both coordinates
    member v.ShiftXY (x, y) = {DX = v.DX + x; DY = v.DY + y}
    /// Get the zero vector
    static member Zero = {DX = 0.0; DY = 0.0}
    /// Return a constant vector along the x axis
    static member ConstX dx = {DX = dx; DY = 0.0}
    /// Return a constant vector along the  y axis
    static member ConstY dy = { DX = 0.0; DY = dy}
    member v.LengthWithSideEffect =
        printfn "Computing!"
        sqrt(v.DX * v.DX + v.DY * v.DY)

let v = {DX = 3.0; DY = 4.0}

//v.Length


//v.Scale(2.0).Length

//Vector2DOld.ConstX(3.0)

let newVector = {DX = 4.0; DY = 4.0}

//newVector.LengthWithSideEffect

/// A type of binary tree, generic in the type of values carried at nodes
type Tree<'T> =
    | Node of 'T * Tree<'T> * Tree<'T>
    | Tip
    member t.Size =
        match t with
        | Node(_, l, r) -> 1 + l.Size + r.Size
        | Tip -> 0

///Using Classes

/// A vector 2D type as a class
type Vector2D(dx : float, dy : float) =
    let len = sqrt(dx * dx + dy * dy)
    static let zero = Vector2D(0.0, 0.0)
    static let oneX = Vector2D(1.0, 0.0)
    static let oneY = Vector2D(0.0, 1.0)
    member v.DX = dx
    member v.DY = dy
    member v.Length = len
    member v.Scale(k) = Vector2D(k * dx, k * dy)
    member v.ShiftX(x) = Vector2D(dx = dx + x, dy = dy)
    member v.ShiftY(y) = Vector2D(dx = dx, dy = dy + y)
    static member Zero = zero
    static member OneX = oneX
    static member OneY = oneY

let v2= Vector2D(3.0, 4.0)
let v3 = Vector2D.OneX

//v2.Length
//v2.Scale(2.0).Length

/// Vectors whose length is checked to be close to length one.
type UnitVector2D(dx,dy) =
    let tolerance = 0.000001

    let length = sqrt (dx * dx + dy * dy)
    do if abs (length - 1.0) >= tolerance then failwith "not a unit vector"

    member v.DX = dx
    member v.DY = dy
    new() = UnitVector2D (1.0, 0.0)
    


/// Adding Further Object Notation to Your Types

open System.Collections.Generic

type SparseVector(items : seq<int * float>) =
    let elems = new SortedDictionary<_,_>()
    do items |> Seq.iter (fun (k, v) -> elems.Add(k,v))

    ///This defines an indexer property
    member t.Item
        with get(idx) =
            if elems.ContainsKey(idx) then elems.[idx]
            else 0.0

let v5 = SparseVector [|(3, 547.0); (4, 550.0)|]

//v5.[3]

///Adding Overloaded Operators

type Vector2DWithOperators(dx : float, dy : float) =
    member x.DX = dx
    member x.DY = dy

    static member (+) (v1: Vector2DWithOperators, v2: Vector2DWithOperators) =
        Vector2DWithOperators(v1.DX + v2.DX, v1.DY + v2.DY)

    static member (-) (v1: Vector2DWithOperators, v2: Vector2DWithOperators) =
        Vector2DWithOperators(v1.DX - v2.DX, v1.DY - v2.DY)

    member v.Print () = printfn "DX = %f; DY = %f" v.DX v.DY

let v6 = new Vector2DWithOperators(3.0, 4.0)
//v6.Print ()
let v7 = v6 + v6
//v7.Print ()

let (++) x y = List.append x [y]

let list = [1;2;3]

//list ++ 4

/// Using Named and Optional Arguments

open System.Drawing

type LabelInfo(?text : string, ?font : Font) =
    let text = defaultArg text ""  
    let font = match font with
               | None -> new Font(FontFamily.GenericSansSerif, 12.0f)
               | Some v -> v
    member x.Text = text
    member x.Font = font

    static member Create(?text, ?font) = new LabelInfo(?text = text, ?font = font)

//LabelInfo (text = "Hello World")
//LabelInfo("Goodbye Lenin")
//LabelInfo.Create()
//LabelInfo(font = new Font(FontFamily.GenericMonospace, 36.0f),
//          text = "Imagine")


/// Adding Method Overloading

/// Interval(lo,hi) represents the range of numbers from lo to hi,
/// but not including either lo or hi.

type Interval(lo, hi) =
    member r.Lo = lo
    member r.Hi = hi
    member r.IsEmpty = hi <= lo
    member r.Contains v = lo < v && v < hi

    static member Empty = Interval(0.0, 0.0)

    /// Return the smallest interval that covers both the intervals
    static member Span (r1 : Interval, r2 : Interval) =
        if r1.IsEmpty then r2 else
        if r2.IsEmpty then r1 else
        Interval (min r1.Lo r2.Lo, max r1.Hi r2.Hi)

    /// Return the smallest interval that covers all the intervals
    static member Span (ranges : seq<Interval>) =
        Seq.fold (fun r1 r2 -> Interval.Span(r1, r2)) Interval.Empty ranges

let interval1 = Interval(3.0, 6.0)
let interval2 = Interval(5.0, 9.0)
let interval3 = Interval (12.0, 14.0)
let intervalSeq = seq [interval1;interval2;interval3]
//Interval.Span(interval1,interval2)
//Interval.Span(intervalSeq)

type Vector =
    { DX : float; DY : float}
    member v.Length = sqrt (v.DX * v.DX + v.DY * v.DY)

type Point =
    { X : float; Y : float}

    static member (-) (p1 : Point, p2 : Point) =
        { DX = p1.X - p2.X; DY = p1.Y - p2.Y }

    static member (-) (p : Point, v : Vector) = 
        { X = p.X - v.DX; Y = p.Y - v.DY }

/// Defining Object Types with Mutable State

type MutableVector2D(dx : float, dy : float) =
    let mutable currDX = dx
    let mutable currDY = dy

    member vec.DX with get() = currDX and set v = currDX <- v
    member vec.DY with get() = currDY and set v = currDY <- v

    member vec.Length
        with get () = sqrt (currDX * currDX + currDY * currDY)
        and set len =
            let theta = vec.Angle
            currDX <- cos theta * len
            currDY <- sin theta * len

    member vec.Angle
        with get () = atan2 currDY currDX
        and set theta =
            let len = vec.Length
            currDX <- cos theta * len
            currDY <- sin theta * len

let myVector = MutableVector2D(3.0, 4.0)

//(myVector.DX, myVector.DY)

//(myVector.Length, myVector.Angle)

//myVector.Angle <- System.Math.PI / 6.0

//(myVector.DX, myVector.DY, myVector.Length)


open System.Collections.Generic
type IntegerMatrix(rows : int, cols : int) =
    let elems = Array2D.create rows cols None : int option [,]
    /// This defines an indexer property with getter and setter
    member t.Item
        with get (idx1, idx2) = elems.[idx1, idx2]
        and set (idx1, idx2) v = elems.[idx1, idx2] <- v

let myMatrix = IntegerMatrix(10, 10)

//myMatrix.[5, 5] <- (Some 10)

//myMatrix.[5,5]
//myMatrix.[9,9]

/// Using Optional Property Settings

open System.Drawing
open System.Windows.Forms
type LabelInfoWithPropertySetting() =
    //let mutable text = "" // the default
    //let mutable font = new Font(FontFamily.GenericSansSerif, 12.0f)
    //member x.Text with get() = text and set v = text <- v
    // member x.Font with get() = font and set v = font <- v

    /// shorthand version with 'Auto-Properties'
    member val Name = "label"
    member val Text = "" with get, set
    member val Font = new Font(FontFamily.GenericSansSerif, 12.0f) with get, set

let labelInfo = LabelInfoWithPropertySetting(Text = "Hello World")

//let form = new Form(Visible = true, TopMost = true, Text = "Welcome to F#")

/// Getting Started with Object Interface Types

open System.Drawing

/// Defining New Object Interface Types
type IShape =
    abstract Contains : PointF -> bool
    abstract BoundingBox : RectangleF
    abstract ShapeInfo : string


/// Implementing Object Interface Types Using Object Expressions

// { new IShape with ... }
// General Form:
(*
{ new Type optional-arguments with
      member-definition
  optional-extra-interface-definitions }
*)

let circle (center : PointF, radius : float32) =
     { new IShape with

         member x.Contains(p : PointF) =
             let dx = float32 (p.X - center.X)
             let dy = float32 (p.Y - center.Y)
             sqrt(dx * dx + dy * dy) <= radius
             
         member x.BoundingBox =
             RectangleF(
                 center.X - radius, center.Y - radius,
                 2.0f * radius + 1.0f, 2.0f * radius + 1.0f)
         member x.ShapeInfo =
             sprintf "Center: (%f,%f), Radius: %f" center.X center.Y radius 
                 }

let square (center : PointF, side : float32) =
    { new IShape with

        member x.Contains(p:PointF) =
            let dx = p.X - center.X
            let dy = p.Y - center.Y
            abs(dx) < side / 2.0f && abs(dy) < side / 2.0f

        member x.BoundingBox =
            RectangleF(center.X - side, center.Y - side, side * 2.0f, side * 2.0f)
            
        member x.ShapeInfo =
            sprintf "Center: (%f, %f), Side: %f" center.X center.Y side}

let bigCircle = circle(PointF(0.0f, 0.0f), 100.0f)
//bigCircle.BoundingBox
//bigCircle.Contains(PointF(70.0f,70.0f))
//bigCircle.Contains(PointF(71.0f,71.0f))

let smallSquare = square(PointF(1.0f, 1.0f), 1.0f)
//smallSquare.BoundingBox
//smallSquare.Contains(PointF(0.0f, 0.0f))

/// IMplementing Object Interface Types Using Concrete Types

type Area =
        | Area of float32

type MutableCircle(center: PointF, radius: float32) =
    
    member val Center = center with get, set
    member val Radius = radius with get, set

    member c.Perimeter = 2.0 * System.Math.PI * float c.Radius

    static member private ZeroOrigin = new PointF(0.0f, 0.0f)

    interface IShape with

        member c.Contains(p : PointF) =
            let dx = p.X - c.Center.X
            let dy = float32 (p.Y - c.Center.Y)
            sqrt(dx * dx + dy * dy) <= float32 c.Radius

        member c.BoundingBox =
            RectangleF(
                c.Center.X - c.Radius, c.Center.Y - c.Radius,
                2.0f * c.Radius + 1.0f, 2.0f * c.Radius + 1.0f)

        member c.ShapeInfo =
            sprintf "Center: (%f,%f), Radius: %f" center.X center.Y radius 

    new(a : float32,center : PointF) =      
        let radius = sqrt((float32 a)/(float32 System.Math.PI))
        MutableCircle(center, radius)     

    new(area : Area) =
        match area with
        | Area a ->
            let radius = sqrt((float32 a)/(float32 System.Math.PI))
            MutableCircle(MutableCircle.ZeroOrigin, radius)

    new(radius : float32) =
        MutableCircle(MutableCircle.ZeroOrigin, radius)

    new() =
        MutableCircle(MutableCircle.ZeroOrigin, 10.0f)


let circle2 = MutableCircle()
//circle2.Radius
//(circle2 :> IShape).BoundingBox

let r1 = square(new PointF(1.0f, 1.0f), 2.0f) 
let c1 = new MutableCircle() :> IShape
let c2 = new MutableCircle(new PointF(3.0f, 3.0f), radius = 4.0f) :> IShape
let c3 = new MutableCircle(Area(4.0f)) :> IShape
let c4 = new MutableCircle(radius = 4.0f) :> IShape

let printShapeInfo (c:IShape) = printfn "%s" c.ShapeInfo

//[r1;c1;c2;c3;c4] |> List.map printShapeInfo

///Using Common Object Interface Types from the .Net Libraries

type IEnumerator<'T> =
    abstract Current : 'T
    abstract MoveNext : unit -> bool

type IEnumerable<'T> =
    abstract GetEnumerator : unit -> IEnumerator<'T>

/// Understanding Hierarchies of Object Interface Types

type ICollection<'T> =
    inherit IEnumerable<'T>
    abstract Count : int
    abstract isReadOnly : bool
    abstract Add : 'T -> unit
    abstract Clear : unit -> unit
    abstract Contains : 'T -> bool
    abstract CopyTo : 'T [] * int -> unit
    abstract Remove : 'T -> unit

/// More Techniques for Implementing Objects

/// Combining Object Expressions and Function Parameters

/// An object interface type that consumes characters and strings
type ITextOutputSink =
   /// When implemented, writes one Unicode character to the sink
   abstract WriteChar : char -> unit

   /// When implemennted, writes one Unicode string to the sink
   abstract WriteString : string -> unit

/// Returns an object that implements ITextOutputSink by using writeCharFunction
let simpleOutputSink writeCharFunction =
    { new ITextOutputSink with
        member x.WriteChar(c) = writeCharFunction c
        member x.WriteString(s) = s |> String.iter x.WriteChar}

let stringBuilderOutputSink(buf : System.Text.StringBuilder ) =
    simpleOutputSink (fun c -> buf.Append(c) |> ignore)

/// Implementation
open System.Text

let buf = new StringBuilder()
let c = stringBuilderOutputSink(buf)

//buf.ToString()
//["Incy"; " "; "Wincy"; " "; "Spider"] |> List.iter c.WriteString
//buf.ToString()

//[';';' '; 'D';'o';'n';'e'] |> List.iter c.WriteChar
//buf.ToString()

/// A type that fully implements the ITextOutputSink object interface
type CountingOutputSink(writeCharFunction : char -> unit) =
    let mutable count = 0
    interface ITextOutputSink with
        member x.WriteChar(c) = count <- count + 1; writeCharFunction(c)
        member x.WriteString(s) = s |> String.iter (x :> ITextOutputSink).WriteChar
    member x.Count = count

/// Defining Partially Implement Class Types

[<AbstractClass>]
type TextOutputSink() =
    abstract WriteChar : char -> unit
    abstract WriteString : string -> unit
    default x.WriteString s = s |> String.iter x.WriteChar

/// Using Partially Implemented Types via Delegation

/// A type which usees a TextOutputSink internally
type HtmlWriter() =
    let mutable count = 0
    let sink =
        { new TextOutputSink() with
              member x.WriteChar c =
                  count <- count + 1;
                  System.Console.Write c}

    member x.CharCount = count
    member x.OpenTag(tagName) = sink.WriteString(sprintf "<%s>" tagName)
    member x.CloseTag(tagName) = sink.WriteString(sprintf "</%s>" tagName)
    member x.WriteString(s) = sink.WriteString(s)

/// Using Partially IMplemented Types via Implementation Inheritance

/// An implementation of TextOutputSink, counting the number of bytes written
type CountingOutputSinkByInheritance() =
    inherit TextOutputSink()

    let mutable count = 0

    member sink.Count = count

    default sink.WriteChar c =
        count <- count + 1
        System.Console.Write c

open System.Text
/// A component to write bytes to an ouput sink
[<AbstractClass>]
type ByteOutputSink() =
    inherit TextOutputSink()

    /// When implemented, writes one byte to the sink
    abstract WriteByte : byte -> unit
    
    /// When implemented, writes multiple bytes to the sink
    abstract WriteBytes : byte[] -> unit

    default sink.WriteChar c = sink.WriteBytes(Encoding.UTF8.GetBytes [|c|])

    override sink.WriteString s = sink.WriteBytes(Encoding.UTF8.GetBytes s)
    
    default sink.WriteBytes b = b |> Array.iter sink.WriteByte

/// Combining Functionl and Objects: Cleaning Up Resources
open System.IO
let myWriteStringToFile1() =
    use outp = File.CreateText("playlist.txt")
    outp.WriteLine("Enchanted")
    outp.WriteLine("Put your records on")

// This is eqivalent ot the following:
let myWriteStringToFile () =
    let outp = File.CreateText("playlist.txt")
    try
        outp.WriteLine("Enchanted")
        outp.WriteLine("Put your records on")
    finally
        (outp :> System.IDisposable).Dispose()

/// Resources and IDisposable

let http (url : string) =
   let req = System.Net.WebRequest.Create url
   use resp = req.GetResponse()
   use stream = resp.GetResponseStream()
   use reader = new System.IO.StreamReader(stream)
   let html = reader.ReadToEnd()
   html

/// Managing Resoruces with More-Complex Liftimes

/// Cleaning Up Internal Objects

// Implementing IDisposable to clean up internal objects

open System.IO

type LineChooser(fileName1, fileName2) =
    let file1 = File.OpenText(fileName1)
    let file2 = File.OpenText(fileName2)
    let rnd = new System.Random()

    let mutable disposed = false

    let cleanup() =
        if not disposed then
            disposed <- true;
            file1.Dispose();
            file2.Dispose();
    
    interface System.IDisposable with
        member x.Dispose() = cleanup()

    member obj.CloseAll() = cleanup()

    member obj.GetLine() =
        if not file1.EndOfStream &&
            (file2.EndOfStream || rnd.Next() % 2 = 0) then file1.ReadLine()
        elif not file2.EndOfStream then file2.ReadLine()
        else raise (new EndOfStreamException())

open System

open System.IO
//File.WriteAllLines("test1.txt", [|"Daisy, Daisy"; "Give me your hand oh do"|])
//File.WriteAllLines("test2.txt", [|"I'm a little teapot"; "Short and stout"|])
let chooser = new LineChooser("test1.txt", "test2.txt")

//chooser.GetLine()
//chooser.GetLine()
//(chooser :> IDisposable).Dispose()
//chooser.GetLine()

/// Cleaning Up Unmangaed Objects

/// Reclaiming unmanaged tickets with IDisposable

open System

type TicketGenerator() =
    let mutable free = []
    let mutable max = 0

    member h.Alloc() =
        match free with
        | [] -> max <- max + 1; max
        | h :: t -> free <- t; h

    member h.Dealloc(n:int) =
        printfn "returning ticket %d" n
        free <- n :: free

let ticketGenerator = new TicketGenerator()

type Customer() =
    let myTicket = ticketGenerator.Alloc()
    let mutable disposed = false
    let cleanup() =
        if not disposed then
            disposed <-true
            ticketGenerator.Dealloc(myTicket)

    member x.Ticket = myTicket

    override x.Finalize() = cleanup()

    interface IDisposable with
        member x.Dispose() = cleanup(); GC.SuppressFinalize(x)

let bill = new Customer()

//bill.Ticket


//(use joe = new Customer() in printfn "joe.Ticket = %d" joe.Ticket)

//(use jane = new Customer() in printfn "jane.Ticket = %d" jane.Ticket)

let darien = new Customer()

//(use darina = new Customer() in printfn "darina.Ticket = %d" darina.Ticket)

//(bill :> IDisposable).Dispose()

//(use darina = new Customer() in printfn "darina.Ticket = %d" darina.Ticket)

let darina = new Customer()
//darina.Ticket

/// Extending Existing Types and Modules

module NumberTheoryExtensions =
    let factorize i =
        let lim = int (sqrt (float i))
        let rec check j =
            if j > lim then None
            elif (i % j) = 0 then Some (i / j, j)
            else check (j + 1)
        check 2

    type System.Int32 with
        member i.IsPrime = (factorize i).IsNone
        member i.TryFactorize() = factorize i

open NumberTheoryExtensions

//(2 + 1).IsPrime

//(6093704 + 11).TryFactorize()

module CSharpStyleExtensions =
    
    open System.Runtime.CompilerServices

    let factorize i =
        let lim = int (sqrt (float i))
        let rec check j =
            if j > lim then None
            elif (i % j) = 0 then Some (i / j, j)
            else check (j + 1)
        check 2

    [<Extension>]
    type Int32Extensions() =
        [<Extension>]
        static member IsPrime2(i:int) = (factorize i).IsNone

        [<Extension>]
        static member TryFactorize2(i:int) = factorize i

    [<Extension>]
    type ResizeArrayExtensions() =
        [<Extension>]
        static member Product(values:ResizeArray<int>) =
            let mutable total = 1
            for v in values do
                total <- total * v
            total

        [<Extension>]
        static member inline GenericProduct(values:ResizeArray<'T>) =
            let mutable total = LanguagePrimitives.GenericOne<'T>
            for v in values do
                total <- total * v
            total


open CSharpStyleExtensions

//(2 + 1).IsPrime2()

//(6093704 + 11).TryFactorize2()

open System.Collections.Generic

let arr = ResizeArray([1 .. 10])

let arr2 = ResizeArray([ 1L .. 10L ])

//arr.Product()

//arr.GenericProduct()

//arr2.GenericProduct()

module List =
    let rec pairwise l =
        match l with
        | [] | [_] -> []
        | h1 :: ((h2 :: _) as t) -> (h1, h2) :: pairwise t

//List.pairwise [1; 2; 3; 4]

/// Working with F# Objects and .Net Types

type Vector2D2(dx : float, dy : float) =
    class
        let len = sqrt(dx * dx + dy * dy)
        member v.DX = dx
        member v.DY = dy
        member v.Length = len
    end

[<Class>]
type Vector2D3(dx : float, dy : float) =
    let len = sqrt(dx * dx + dy * dy)
    member v.DX = dx
    member v.DY = dy
    member v.Length = len

type IShape2 =
    interface
        abstract Contains : Point -> bool
        abstract BoundingBox : Rectangle
    end

[<Interface>]
type IShape3 =
    abstract Contains : Point -> bool
    abstract BoundingBox : Rectangle

/// Structs

[<Struct>]
type Vector2DStruct(dx : float, dy : float) =
    member v.DX = dx
    member v.DY = dy
    member v.Length = sqrt (dx * dx + dy * dy)

[<Struct>]
type Vector2DStructUsingExplicitVals =
    val dx : float
    val dy : float
    member v.DX = v.dx
    member v.DY = v.dy
    member v.Length = sqrt (v.dx * v.dx + v.dy * v.dy)

/// Delegates

type ControlEventHandler = delegate of int -> bool

///Add a new handler ot the Win32 ctrl + c handling API
open System.Runtime.InteropServices

let ctrlSignal = ref false

[<DllImport("kernel32.dll")>]
extern void SetConsoleCtrlHandler(ControlEventHandler callback, bool add)

let ctrlEventHandler = new ControlEventHandler(fun i -> ctrlSignal := true; true)

//SetConsoleCtrlHandler(ctrlEventHandler, true)


/// Enums

type Vowels =
    | A = 1
    | E = 5
    | I = 9
    | O = 15
    | U = 21

/// Working with null Values

let parents = [("Adam", None); ("Cain", Some("Adam", "Eve"))]

//match System.Environment.GetEnvironmentVariable("PATH") with
//| null -> printf "the enviornment variable PATH is not defined\n"
//| res -> printf "the enviornment variable PATH is set to %s\n" res

let switchOnType (a : obj) =
    match a with
    | null -> printf "null!"
    | :? System.Exception as e -> printf "An exception: %s!" e.Message
    | :? System.Int32 as i -> printf "An integer: %d!" i
    | :? System.DateTime as d -> printf "A date/time: %O!" d
    | _ -> printf "Some other kind of object\n"













