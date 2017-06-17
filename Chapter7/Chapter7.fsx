/// Encapsulating and Organizing Your Code

/// Hiding Things

/// Hiding Things with Local Definitions

let generateTicket =
    let mutable count = 0
    (fun () -> count <- count + 1; count)

type IPeekPoke =
    abstract member Peek : unit -> int
    abstract member Poke : int -> unit

let makeCounter initialState = 
    let mutable state = initialState
    {new IPeekPoke with
        member x.Poke n = state <- state + n
        member x.Peek() = state}

type TicketGenerator() =
    // Note: let bindings in a type definition are implicitly private to the object
    // being constructed. Members are implicily public.
    let mutable count = 0

    member x.Next() =
        count <- count + 1
        count

    member x.Reset () =
        count <- 0

type IStatistic<'T, 'U> =
    abstract Record : 'T -> unit
    abstract Value : 'U

let makeAverager(toFloat : 'T -> float) =

    let mutable count = 0
    let mutable total = 0.0

    { new IStatistic<'T, float> with
        
        member __.Record(x) =
            count <- count + 1
            total <- total + toFloat x

        member __.Value =
            total / float count }


/// Hiding THings with Accessibility Annotations

open System

module public VisitorCredentials =

    /// The internal table of permitted visitors and the
    /// days they are allowed to visit.
    let private visitorTable =
        dict [("Anna", set [DayOfWeek.Tuesday; DayOfWeek.Wednesday]);
              ("Carolyn", set [DayOfWeek.Friday])]

    /// This is the function to check if a person is a permitted visitor.
    /// Note: this is public and can be used by external code.
    let public checkVisitor(person) =
        visitorTable.ContainsKey(person) &&
        visitorTable.[person].Contains(DateTime.Today.DayOfWeek)

    /// This is the function to return all known permitted visitors.
    /// Note: this is internal and can be used only by code in the assembly.
    let internal allKnownVisitors() =
        visitorTable.Keys


type Vector2D =
    {DX : float; DY : float}

module Vector2DOps =
    let length v = sqrt (v.DX * v.DX + v.DY * v.DY)
    let scale k v = {DX = k * v.DX; DY = k * v.DY}
    let shiftX x v = {v with DX = v.DX + x}
    let shiftY y v = {v with DY = v.DY + y}
    let zero = {DX = 0.0; DY = 0.0}
    let constX dx = {DX = dx; DY = 0.0}
    let constY dy = {DX = 0.0; DY = dy}





