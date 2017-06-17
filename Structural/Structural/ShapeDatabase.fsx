
//#r @"C:\Users\user\Documents\CODE\F#\Expert F# 4.0\Structural\Structural\bin\Debug\Structural.dll"
#r @"C:\Users\darien.shannon\Documents\Code\F#\Expert-FSharp-4.0\Structural\Structural\bin\Debug\Structural.dll"
open Structural
open Structural.Shapes


module AISC =
    type SingleAngles =
        static member ``L2x2x1/4`` = SingleAngle.create 2.0<inch> 2.0<inch> 0.25<inch>
        static member ``L3x3x1/4`` = SingleAngle.create 3.0<inch> 3.0<inch> 0.25<inch>

    type DoubleAngles =
        static member ``2L2x2x1/4`` = DoubleAngle.create 2.0<inch> 2.0<inch> 0.25<inch> 1.0<inch>
        static member ``2L3x3x1/4`` = DoubleAngle.create 3.0<inch> 3.0<inch> 0.25<inch> 1.0<inch>

module NMBS =

    type CC_SingleAngles =
        static member ``A44A`` = CF_SingleAngle.create 1.5<inch> 1.5<inch> 0.25<inch> 0.325<inch>

    type CC_DoubleAngles =
        static member ``A44A`` = CF_DoubleAngle.create 1.5<inch> 1.5<inch> 0.25<inch> 0.325<inch> 1.0<inch>

