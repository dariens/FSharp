let divide ifZero ifSuccess top bottom =
    if (bottom = 0)
    then ifZero()
    else ifSuccess (top/bottom)

let isEven ifOdd ifEven aNumber =
    if (aNumber % 2 = 0)
    then aNumber |> ifEven
    else aNumber |> ifOdd

let ifZero() = None
let ifSuccess value = Some value

divide ifZero ifSuccess 10 1

type DbResult<'a> = 
    | Success of 'a
    | Error of string

type CustomerId =  CustomerId of string
type OrderId =  OrderId of int
type ProductId =  ProductId of string



let getCustomerId name =
    if (name = "") 
    then Error "getCustomerId failed"
    else Success (CustomerId "Cust42")

let getLastOrderForCustomer (CustomerId custId) =
    if (custId = "") 
    then Error "getLastOrderForCustomer failed"
    else Success (OrderId 123)

let getLastProductForOrder (OrderId orderId) =
    if (orderId  = 0) 
    then Error "getLastProductForOrder failed"
    else Success (ProductId "Product456")

type DbResultBuilder() =

    member this.Bind(m, f) = 
        match m with
        | Error e -> Error e
        | Success a -> 
            printfn "\tSuccessful: %A" a
            f a

    member this.Return(x) = 
        Success x

let dbresult = new DbResultBuilder()

let product' = 
    dbresult {
        let! custId = getCustomerId "Alice"
        let! orderId = getLastOrderForCustomer (CustomerId "")
        let! productId = getLastProductForOrder orderId 
        printfn "Product is %A" productId
        return productId
        }
printfn "%A" product'




type MaybeBuilder() =
    member this.Bind(m, f) = Option.bind f m
    member this.Return(x) = 
        printfn "Wrapping a raw value into an option"
        Some x
    member this.ReturnFrom(m) = 
        printfn "Returning an option directly"
        m

let maybe = new MaybeBuilder()

maybe {return! (Some 2)}

let divideBy bottom top = 
    if bottom = 0
    then None
    else Some (top/bottom)

let divide2 (bottom1, top1) bottom2 =
    maybe
        {
        let! x = top1 |> divideBy bottom1
        return! x |> divideBy bottom2      
        }

divide2 (3, 12) 2


type StringIntBuilder() =
    
    member this.Bind(m, f) =
        let b, i = System.Int32.TryParse(m)
        match b, i with
        | false,_ -> "error"
        | true, i -> f i

    member this.Return(x) = 
        sprintf "%i" x

let stringint = new StringIntBuilder()

let good =
    stringint {
        let! i = "42"
        let! j = "x"
        return i + j
        }

let g1 = "x"
let g2 = stringint {
            let! i = g1
            return i
            }



type ListWorkflowBuilder() =

    member this.Bind(list, f) =
        list |> List.collect f

    member this.Return(x) =
        [x]

    member this.For(list, f) =
        this.Bind(list, f)

let listWorkflow = new ListWorkflowBuilder()

let added =
    listWorkflow {
        for i in [1;2;3] do
        for j in [10;11;12] do
        return i + j}

type IdentityBuilder() =
    member this.Bind(m, f) = f m
    member this.Return(x) = x
    member this.ReturnFrom(x) = x

let identity = new IdentityBuilder()

let result = identity {
    let! x = 1
    let! y = 2
    return x + y}

type TraceBuilder() =
    member this.Bind(m, f) =
        match m with
        | None ->
            printfn "Binding with None. Exiting."
        | Some a ->
            printfn "Binding with Some(%A). Continuing." a
        Option.bind f m

    member this.Return(x) =
        printfn "Returning a unwrapped %A as an option." x
        Some x

    member this.ReturnFrom(m) =
        printfn "Returning an option (%A) directly" m
        m

    member this.Zero() =
        printfn "Zero"
        None

    member this.Yield(x) =
        printfn "Yield an unwrapped %A as an option" x
        Some x

    member this.YieldFrom(m) =
        printfn "Yield an option (%A) directly" m
        m

    member this.Combine (a, b) =
        match a, b with
        | Some a', Some b' ->
            printfn "combining %A and %A" a' b'
            Some (a' + b')
        | Some a', None ->
            printfn "combining %A with None" a'
            Some (a')
        | None, Some b' ->
            printfn "combining None with %A" b'
            Some (b')
        | None, None ->
            printfn "combining None with None"
            None

    member this.Delay(f) =
        printfn "Delay"
        f()

let trace = new TraceBuilder()

trace {
    return 1
    } |> printfn "Result 1: %A"

trace {
    return! Some 2
    } |> printfn "Result 2: %A"

trace {
    let! x = Some 1
    let! y = Some 2
    return x + y
    } |> printfn "Result 3: %A"

trace {
    let! x = None
    let! y = Some 1
    return x + y
    } |> printfn "Result 4: %A"


trace {
    do! None
    do! Some (printfn "...another expression that returns unit")
    let! x = Some (1)
    return x
    } |> printfn "Result from do: %A"

trace {
    printfn "hello world"
    } |> printfn "Result for simple expression %A"


trace {
    if false then return 1
    } |> printfn "Result for if without else: %A"

trace {
    let! x = Some 1
    let! y = Some 2
    yield x + y
    } |> printfn "Result for yield: %A"


trace {
    yield! Some 1
    } |> printfn "Result for yield!: %A"

type ListBuilder() =
    member this.Bind(m, f) =
        printfn "Binding %A" m
        m |> List.collect f

    member this.Zero() =
        printfn "Zero"
        []

    member this.Yield(x) =
        printfn "Yield an unwrapped %A as a list" x
        [x]

    member this.YieldFrom(m) =
        printfn "Yield a list (%A) directly" m
        m

    member this.For(m, f) =
        printfn "For %A" m
        this.Bind(m, f)

    member this.Combine (a, b) =
        printfn "combining %A and %A" a b
        List.concat [a; b]

    member this.Delay(f) =
        printfn "Delay"
        f()

let listbuilder = new ListBuilder()

listbuilder {
    let! x = [1..3]
    let! y = [10;20;30]
    yield x * y
    } |> printfn "Result: %A"

listbuilder {
    for x in [1..3] do
    for y in [10;20;30] do
    yield x * y
    } |> printfn "Result: %A"



trace{
    yield 1
    let! x = None
    yield x
    yield 3
    } |> printfn "Result for yield then yield: %A"

trace {
    yield 1
    return 2
    return 3
    } |> printfn "Result for yield then return: %A"



listbuilder {
    yield 1
    } |> printfn "Result for yield: %A"

listbuilder {
    yield 1
    yield 2
    } |> printfn "Result for yield then yield: %A"

listbuilder {
    yield 1
    yield! [2;3]
    } |> printfn "Result for yield then yield list: %A"

listbuilder {
    for i in ["red"; "blue"] do
        yield i
        for j in ["hat"; "tie"] do
            yield! [i + " " + j; "-"]
            } |> printfn "Result for for..in..do : %A"



