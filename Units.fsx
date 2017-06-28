open System.Diagnostics





type IDimension =
    abstract member Dimension : int

[<RequireQualifiedAccess>]
module Dim =

    type ``1``() =
        static member create = ``1``()
        interface IDimension with
            member this.Dimension = 1

    type ``2``() =
        static member create = ``2``()
        interface IDimension with
            member this.Dimension = 2

    type ``3``() =
        static member create = ``3``()
        interface IDimension with
            member this.Dimension = 3

    type ``4``() =
        static member create = ``4``()
        interface IDimension with
            member this.Dimension = 4

    let dimFromInt (int : int) =
        match int with
        | 1 -> (``1``()) :> IDimension
        | 2 -> (``2``()) :> IDimension
        | 3 -> (``3``()) :> IDimension
        | 4 -> (``4``()) :> IDimension
        | _ -> failwith "Dimension not supported."


    


    


let inline getValue (input : ^T) : float =
    (^T : ( member Value : float) (input))

let inline getPower (input : ^T) : int =
    (^T : ( member Power : int) (input))

let inline getBaseUnit (input : ^T)  =
    (^T : ( static member baseUnit : System.Type) ())

let inline getScale (input : ^T) =
    (^T : (static member scale : float) ())

let inline getUnit input = (getValue input), (getPower input), (getScale input), (getBaseUnit input)
             
let scaleList = []

let inline addScaleToList (input : ^T) =
    let float = (^T : (static member scale : float) ())
    float :: scaleList

let inline addToMeasureTable (input : ^T) =
    let measureTable = (^T : (member MeasureTable : System.Collections.Generic.Dictionary<float, int>) (input))
    measureTable.Add(getValue input, getPower input)


let inline toBaseUnit< ^T, ^TBase when ^T : (static member toBaseUnit : ^T -> ^TBase)> (input : ^T) =
        (^T : (static member toBaseUnit : ^T -> ^TBase) (input))

let inline fromBaseUnit< ^T, ^TBase when ^T : (static member fromBaseUnit : ^TBase -> ^T)> (input : ^TBase) =
    (^T : (static member fromBaseUnit : ^TBase -> ^T) (input))

let inline convert< ^TInitial, ^TFinal, ^TBase when
                        ^TInitial : (static member toBaseUnit : ^TInitial -> ^TBase) and
                        ^TInitial : (static member fromBaseUnit :^TBase -> ^TInitial) and
                        ^TFinal : (static member fromBaseUnit :^TBase -> ^TFinal)> (initial : ^TInitial) =
                            let initialAsBase = (^TInitial : (static member toBaseUnit : ^TInitial -> ^TBase) initial)
                            let final = (^TFinal : (static member fromBaseUnit : ^TBase -> ^TFinal) initialAsBase)
                            final

let inline toBase< ^TInitial, ^TBase when
                        ^TInitial : (static member scale : ^TBase) and
                        ^TInitial : (member Power : float) and
                        ^TInitial : (member Value : float) and
                        ^TBase : (member Value : float) and
                        ^TBase : (static member create : float * float -> ^TBase)> (input : ^TInitial) =
                            let currentValue = (^TInitial : (member Value : float) input)                           
                            let power = (^TInitial : (member Power : float) input)
                            let scale = getValue (^TInitial : (static member scale : 'TBase) ())
                            let scaledValue = currentValue * (scale ** power)
                            (^TBase : (static member create : float * float -> ^TBase) (scaledValue, power))

let inline fromBase< ^TBase, ^TFinal when
                        ^TFinal : (static member scale : ^TBase) and
                        ^TFinal : (member Value : float) and
                        ^TFinal : (static member create : float * float -> ^TFinal) and
                        ^TBase : (member Value : float) and
                        ^TBase : (member Power : float)> (input : ^TBase) =
                            let currentValue = (^TBase : (member Value : float) input)                           
                            let power = (^TBase : (member Power : float) input)
                            let scale = getValue (^TFinal : (static member scale : 'TBase) ())
                            let scaledValue = currentValue / (scale ** power)
                            (^TFinal : (static member create : float * float -> ^TFinal) (scaledValue, power))

let inline convert2< ^TInitial, ^TFinal, ^TBase when
                       ^TInitial : (static member scale : ^TBase) and
                       ^TInitial : (member Power : float) and
                       ^TInitial : (member Value : float) and
                       ^TFinal : (static member scale : ^TBase) and
                       ^TFinal : (static member create : float * float -> ^TFinal) and
                       ^TFinal : (member Value : float) and
                       ^TBase : (static member create : float * float -> ^TBase) and
                       ^TBase : ( member Value : float) and
                       ^TBase : ( member Power : float)> (input : ^TInitial) =
                         let inputAsBase = toBase input
                         let output = fromBase inputAsBase : ^TFinal
                         output


let inline convert3< ^TInitial, ^TFinal, ^TBase when
                       ^TInitial : (static member scale : ^TBase) and
                       ^TInitial : (member Power : float) and
                       ^TInitial : (member Value : float) and
                       ^TFinal : (static member scale : ^TBase) and
                       ^TFinal : (static member create : float * float -> ^TFinal) and
                       ^TFinal : (member Value : float) and
                       ^TBase : (member Value : float) > (input : ^TInitial) =
                         let initialValue = (^TInitial : (member Value : float) input)
                         let power = (^TInitial : (member Power : float) input)
                         let initialScale = (^TBase : (member Value : float) (^TInitial : (static member scale : 'TBase) ()))
                         let finalScale = (^TBase : (member Value : float) (^TFinal : (static member scale : 'TBase) ()))
                         let relativeScale = initialScale/finalScale
                         let scaledValue = initialValue * (relativeScale ** power)
                         let output = (^TFinal : (static member create : float * float -> ^TFinal) (scaledValue, power))
                         output

let inline unitAdd< ^T when
                    ^T : (member Value: float) and
                    ^T : (member Power: float) and
                    ^T : (static member create: float*float -> ^T)>
                    (unit1 : ^T, unit2 : ^T) =
                    let value1 = (^T : (member Value: float) unit1)
                    let power1 = (^T : (member Power: float) unit1)
                    let value2 = (^T : (member Value: float) unit2)
                    let power2 = (^T : (member Power: float) unit2)
                    let power =
                        match (power1, power2) with
                        | power1, power2 when power1 = power2 -> power1
                        | _ -> failwith "Cannot add units with different dimensions"
                    (^T : (static member create: float*float -> ^T) (value1 + value2, power))

                            


type Inch(value : float, power : float) =
    member this.Value = value
    member this.Power = power
    static member scale = Inch(1.0)
    static member toBaseUnit (inch : Inch) = Inch(inch.Value, inch.Power)
    static member fromBaseUnit (inch : Inch) = Inch(inch.Value, inch.Power)
    static member create (value:float, power:float) = Inch (value, power)  
    new(value:float) = Inch(value, 1.0)

type Foot(value : float, power : float) =
    member this.Value = value
    member this.Power = power
    member inline this.Convert<'TFinal when 'TFinal : (static member toBaseUnit : 'TFinal -> Inch)>() = convert<Foot, 'TFinal, Inch> this
    static member scale = Inch(12.0)
    static member create (value : float, power : float)  = Foot(value, power)
    static member toBaseUnit (foot : Foot) = Inch(foot.Value * (12.0 ** foot.Power), foot.Power)
    static member fromBaseUnit (inch : Inch) = Foot(inch.Value / (12.0 ** inch.Power), inch.Power)
    new(value:float) = Foot(value,1.0)

type Yard(value : float, power : float) =
    member this.Value = value
    member this.Power = power
    static member scale = Inch(36.0)
    static member create (value:float, power:float) = Yard (value, power)
    static member toBaseUnit (yard : Yard) = Inch(yard.Value * (36.0 ** yard.Power), yard.Power)
    static member fromBaseUnit (inch : Inch) = Yard(inch.Value / (36.0 ** inch.Power), inch.Power)


type Inch2(value : float, power : float) =
    member this.Value = value
    member this.Power = power
    static member scale = Inch2(1.0)
    static member create (value:float, power:float) = Inch2 (value, power)
    static member add (first : Inch2, second : Inch2) = first.Value + second.Value
    new(value:float) = Inch2(value, 1.0)

type Inch3< ^T when ^T :> IDimension
                and ^T : (static member create : ^T)>(value : float) =
    member inline this.Value = value
    member inline this.Power = 
        let defaultOfT = (^T : (static member create : ^T) ())
        (defaultOfT :> IDimension).Dimension
    static member inline (+) (first : Inch3<'T>, second:Inch3<'T>) = Inch3<'T>(first.Value + second.Value)


    



let myInch3 = Inch3<Dim.``1``>(3.0)
myInch3.Power
myInch3.Value

let testAgain = myInch3 + myInch3




let myNewInch = Inch3<Dim.``2``>(3.0)
myNewInch.Power



type Foot2(value : float, power : float) =
    member this.Value = value
    member this.Power = power
    member inline this.Convert<'TFinal>() = convert3<Foot2, 'TFinal, Inch> this
    static member scale = Inch(12.0)
    static member create (value : float, power : float)  = Foot2(value, power)
    
    new(value:float) = Foot2(value,1.0)

type Yard2(value : float, power : float) =
    member this.Value = value
    member this.Power = power
    static member scale = Inch(36.0)
    member inline this.Convert<'TFinal>() = convert3<Yard2, 'TFinal, Inch> this
    static member create (value:float, power:float) = Yard2 (value, power)

type Mile(value : float, power : float) =
    member this.Value = value
    member this.Power = power
    static member scale = Inch(63360.0)
    member inline this.Convert<'TFinal>() = convert3<Mile, 'TFinal, Inch> this
    static member create (value:float, power:float) = Mile (value, power)

let myInch = Inch(3.0, 1.0)

let myFoot = Foot(4.0, 1.0)

let myYard = Yard(5.0, 1.0)

toBaseUnit myFoot


let test = (convert<Foot,Yard,_> myFoot).Value

let test2 = myFoot.Convert<Inch>()

let yardToBase = toBase myYard

let yardFromBase = fromBase<Inch, Yard> yardToBase

myYard = yardFromBase



myInch.GetType()

let test5 = convert2<Inch, Yard, _> myInch


let myInch2 = Inch2(1.0, 2.0)
let myFoot2 = Foot2(1.0, 2.0)
let myYard2 = Yard2(1.0, 2.0)

myFoot2.Convert<Inch>()
myFoot2.Convert<Foot>()
myFoot2.Convert<Yard>()

myYard2.Convert<Inch>()
myYard2.Convert<Foot>()
myYard2.Convert<Yard>()

let myMile = Mile(1.0, 1.0)

myMile.Convert<Inch>()
myMile.Convert<Foot>()
myMile.Convert<Yard>()
myMile.Convert<Mile>()

let inch_5 = Inch2(5.0)
let inch_10 = Inch2(10.0)

let test6 = unitAdd (inch_5,inch_10)

[<Measure>] type inch

let inch_5_2 = 5.0<inch>
let inch_10_2 = 10.0<inch>

let unitAdd2 (unit1:float<inch>, unit2:float<inch>) = unit1 + unit2

let customInches = 
    [for i in 1..10000000 do
         yield (inch_5, inch_10)]

let inches =
    [for i in 1..10000000 do
         yield (inch_5_2, inch_10_2)]

let testTime list f =
    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    list |> List.map f |> ignore
    stopWatch.Stop()
    stopWatch.Elapsed.TotalMilliseconds


let fullTest =
    [for i in 1..10 do
        yield testTime customInches Inch2.add / testTime customInches unitAdd]
    |> Seq.average

let myInch = Inch2(5.0)
let myInch2 = Inch2(5.0, 2.0)

unitAdd (myInch, myInch2)

