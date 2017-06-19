(** 
### Calc Test
*)

(*** hide ***)
//#r @"C:\Users\user\Documents\CODE\F#\FSharp\Structural2\Structural2\bin\Debug\Structural2.dll"
#r @"C:\Users\darien.shannon\Documents\Code\F#\FSharp\Structural2\Structural2\bin\Debug\Structural2.dll"
open Structural.SteelDesign

(*** define: Section ***)
let mySection = Plate.create
                    (3.0<inch>, 3.0<inch>,
                     SteelMaterial.create (50.0<ksi>, 60.0<ksi>, 29000.0<ksi>),
                     None)
(*** include: Section ***)
(*** include-value: mySection ***)

(*** define: allowableTension ***)
let allowableTension = Section.allowableTension mySection

(*** include-value: allowableTension ***)
