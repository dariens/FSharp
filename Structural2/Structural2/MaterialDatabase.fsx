//#r @"C:\Users\user\Documents\CODE\F#\FSharp\Structural2\Structural2\bin\Debug\Structural2.dll"
#r @"C:\Users\darien.shannon\Documents\Code\F#\FSharp\Structural2\Structural2\bin\Debug\Structural2.dll"
open Structural.SteelDesign

module ASTM =
    
    let A36 = SteelMaterial.create (Ksi(50.0), Ksi(60.0), Ksi(29000.0))
