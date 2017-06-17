

//#r @"C:\Users\user\Documents\CODE\F#\Expert F# 4.0\Structural\Structural\bin\Debug\Structural.dll"
#r @"C:\Users\darien.shannon\Documents\Code\F#\Expert-FSharp-4.0\Structural\Structural\bin\Debug\Structural.dll"
open Structural

module NMBS =

    type ColdForm =
        static member Standard = Materials.SteelMaterial.create 50.0<ksi> 60.0<ksi> 29000.0<ksi>
