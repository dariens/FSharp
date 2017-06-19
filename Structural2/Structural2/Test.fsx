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

let myNewSection = {mySection with Material = {mySection.Material with Fy = 36.0<ksi>; Fu = 50.0<ksi>}}

let newAllowableTension = Section.allowableTension myNewSection

allowableTension/newAllowableTension = 50.0/36.0


type ILength = interface end

module Length = 

    type Inch(value) =
        let value = value
        member this.Value
            with get() = value

        member this.Feet = Foot(this.Value/12.0)

        interface ILength 

        static member (/) (inch1 : Inch, inch2 : Inch) =
            let inline floatDiv (v1 : float, v2: float) = v1/v2
            Inch (floatDiv (inch1.Value,inch2.Value))

        override this.ToString() = System.String.Format("{0:0.0####} <in>", this.Value)

    and Foot(value) = 
        let value = value
        member this.Value
            with get() = value

        member this.Inch = Inch(this.Value * 12.0)

        interface ILength

        static member (/) (foot1 : Foot, foot2 : Foot) =
            let inline floatDiv (v1 : float, v2: float) = v1/v2
            Inch (floatDiv (foot1.Value,foot2.Value))

        override this.ToString() = System.String.Format("{0:0.0####} <ft>", this.Value)

    type ILength with









