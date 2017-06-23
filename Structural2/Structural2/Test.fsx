


//#r @"C:\Users\user\Documents\CODE\F#\FSharp\Structural2\Structural2\bin\Debug\Structural2.dll"
#r @"C:\Users\darien.shannon\Documents\Code\F#\FSharp\Structural2\Structural2\bin\Debug\Structural2.dll"
open Structural.SteelDesign


//#load @"C:\Users\user\Documents\CODE\F#\FSharp\Structural2\Structural2\SectionDatabase.fsx" 
#load @"C:\Users\darien.shannon\Documents\Code\F#\FSharp\Structural2\Structural2\SectionDatabase.fsx"
open SectionDatabase.AISC


let mySection = SingleAngles.A36.``L2x2x1/4``


mySection.Description
mySection.HorizontalLeg.GetType()
mySection.Area
mySection.XBar

let allowableTension = Section.allowableTension SingleAngles.A36.``L2x2x1/4``

let myNewSection = {mySection with Material = {mySection.Material with Fy = Ksi(36.0); Fu = Ksi(50.0)}}

let newAllowableTension = Section.allowableTension myNewSection

allowableTension.Result/newAllowableTension.Result = 50.0/36.0

let myValue = mySection.HorizontalLeg.inch


let inline mult< ^TBase, ^TFirst, ^TSecond when 
                 ^TBase : (static member (*) : ^TBase * ^TBase -> ^TBase) and
                 ^TFirst : (static member ToBase : ^TFirst -> ^TBase) and
                 ^TFirst : (static member FromBase : ^TBase -> ^TFirst) and
                 ^TSecond : (static member ToBase : ^TSecond -> ^TBase)>
                   (num1: ^TFirst) (num2: ^TSecond) : ^TFirst  =
                     let num1base = (^TFirst : (static member ToBase : ^TFirst -> ^TBase) (num1))
                     let num2base = (^TSecond : (static member ToBase : ^TSecond -> ^TBase) (num2))
                     let baseMult = (^TBase : (static member (*) : ^TBase * ^TBase -> ^TBase) (num1base, num2base))
                     (^TFirst : (static member FromBase : ^TBase -> ^TFirst) (baseMult))

type ILength = interface end

module MyLengths =

    type Inch(value : float, power : int) =
        member this.Value = value
        member this.Power = power

        static member value (inch : Inch) = inch.Value
        static member conversion = 1.0
        static member baseUnit = Inch

        static member ToBase (inch : Inch) = Inch(inch.Value * 1.0, inch.Power)
        static member FromBase (inch : Inch) = Inch(inch.Value, inch.Power) 
        static member (*) (inch1:Inch, inch2:Inch) = Inch(inch1.Value*inch2.Value, inch1.Power + inch2.Power)

    type Foot(value : float, power : int) =
        member this.Value = value
        member this.Power = power

        static member ToBase (foot : Foot) = Inch(foot.Value * (12.0 ** (float foot.Power)), foot.Power)
        static member FromBase (inch : Inch) = Foot(inch.Value / (12.0 ** (float inch.Power)), inch.Power)
        static member (*) (foot1: Foot, foot2: Foot) = Foot(foot1.Value * foot2.Value, foot1.Power + foot1.Power)

    type Yard(value: float, power: int) =
        member this.Value = value
        member this.Power = power

        static member ToBase (yard : Yard) = Inch(yard.Value * (36.0 ** (float yard.Power)), yard.Power)
        static member FromBase (inch : Inch) = Yard(inch.Value / (36.0 ** (float inch.Power)), inch.Power)

    let myInch = Inch(5.0, 1)
    let myFoot = Foot(5.0, 1)
    let myYard = Yard(5.0, 1)

    mult myYard myFoot













        
        

