// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r @"C:\Users\user\Documents\CODE\F#\FSharp\Structural2\Structural2\bin\Debug\Structural2.dll"
open Structural.SteelDesign

#load @"C:\Users\user\Documents\CODE\F#\FSharp\Structural2\Structural2\MaterialDatabase.fsx"
open MaterialDatabase

// Define your library scripting code here

module AISC =

    let A36 = ASTM.A36

    module SingleAngles =
        
        module A36 =
            
            let ``L2x2x1/4`` = SingleAngle.create (2.0<inch>, 2.0<inch>, 0.25<inch>, A36, None)
            let ``L2x2x5/16`` = SingleAngle.create (2.0<inch>, 2.0<inch>, 5.0<inch>/16.0, A36, None)
            let ``L2x2x3/8`` = SingleAngle.create (2.0<inch>, 2.0<inch>, 3.0<inch>/8.0, A36, None)

    module DoubleAngles =

        module A36 =

            let ``2L2x2x1/4x1`` = DoubleAngle.create (2.0<inch>, 2.0<inch>, 0.25<inch>, 1.0<inch>, A36, None)
            let ``2L2x2x5/16x1`` = DoubleAngle.create (2.0<inch>, 2.0<inch>, 5.0<inch>/16.0, 1.0<inch>, A36, None)
            let ``2L2x2x3/8x1`` = DoubleAngle.create (2.0<inch>, 2.0<inch>, 3.0<inch>/8.0, 1.0<inch>, A36, None)
