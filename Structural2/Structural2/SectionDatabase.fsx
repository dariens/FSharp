// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

//#r @"C:\Users\user\Documents\CODE\F#\FSharp\Structural2\Structural2\bin\Debug\Structural2.dll"
#r @"C:\Users\darien.shannon\Documents\Code\F#\FSharp\Structural2\Structural2\bin\Debug\Structural2.dll"
open Structural.SteelDesign

//#load @"C:\Users\user\Documents\CODE\F#\FSharp\Structural2\Structural2\MaterialDatabase.fsx"
#load @"C:\Users\darien.shannon\Documents\Code\F#\FSharp\Structural2\Structural2\MaterialDatabase.fsx"
open MaterialDatabase

// Define your library scripting code here

module AISC =

    let A36 = ASTM.A36

    module SingleAngles =
        
        module A36 =
            
            let ``L2x2x1/4`` = SingleAngle.create (Inch(2.0), Inch(2.0), Inch(0.25), A36, None)
            let ``L2x2x5/16`` = SingleAngle.create (Inch(2.0), Inch(2.0), Inch(5.0/16.0), A36, None)
            let ``L2x2x3/8`` = SingleAngle.create (Inch(2.0), Inch(2.0), Inch(3.0/8.0), A36, None)

    module DoubleAngles =

        module A36 =

            let ``2L2x2x1/4x1`` = DoubleAngle.create (Inch(2.0), Inch(2.0), Inch(0.25), Inch(1.0), A36, None)
            let ``2L2x2x5/16x1`` = DoubleAngle.create (Inch(2.0), Inch(2.0), Inch(5.0/16.0), Inch(1.0), A36, None)
            let ``2L2x2x3/8x1`` = DoubleAngle.create (Inch(2.0), Inch(2.0), Inch(3.0/8.0), Inch(1.0), A36, None)

    