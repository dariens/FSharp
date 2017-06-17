//#r @"C:\Users\user\Documents\CODE\F#\Expert F# 4.0\Structural\Structural\bin\Debug\Structural.dll"
#r @"C:\Users\darien.shannon\Documents\Code\F#\Expert-FSharp-4.0\Structural\Structural\bin\Debug\Structural.dll"
open Structural

#load @"C:\Users\darien.shannon\Documents\Code\F#\Expert-FSharp-4.0\Structural\Structural\MaterialDatabase.fsx"
#load @"C:\Users\darien.shannon\Documents\Code\F#\Expert-FSharp-4.0\Structural\Structural\ShapeDatabase.fsx"

module Sections =

    module NMBS =
        open MaterialDatabase.NMBS
        open ShapeDatabase.NMBS
        
        type SingleAngles =
            static member ``A44A`` = Section.create CC_SingleAngles.A44A ColdForm.Standard None

