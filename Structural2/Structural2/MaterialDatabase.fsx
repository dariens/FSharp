//#r @"C:\Users\user\Documents\CODE\F#\FSharp\Structural2\Structural2\bin\Debug\Structural2.dll"
#r @"C:\Users\darien.shannon\Documents\Code\F#\FSharp\Structural2\Structural2\bin\Debug\Structural2.dll"
open Structural.SteelDesign

module ASTM =
    
    let A36 = SteelMaterial.create (50.0<ksi>, 60.0<ksi>, 29000.0<ksi>)
