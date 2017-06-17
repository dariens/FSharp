#load @"Structural.fsx"
open Structural

module AISC =
    type SingleAngles =
        static member ``L2x2x1/4`` = SingleAngle (SingleAngle.Create 2.0<inch> 2.0<inch> 0.25<inch> 0.0<inch>)
        static member ``L3x3x1/4`` = SingleAngle (SingleAngle.Create 3.0<inch> 3.0<inch> 0.25<inch> 0.0<inch>)

    type DoubleAngles =
        static member ``2L2x2x1/4`` = DoubleAngle (DoubleAngle.Create 2.0<inch> 2.0<inch> 0.25<inch> 0.0<inch> 1.0<inch>)
