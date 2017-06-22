(** 
### Calc Test
*)

(*** hide ***)
//#r @"C:\Users\user\Documents\CODE\F#\FSharp\Structural2\Structural2\bin\Debug\Structural2.dll"
#r @"C:\Users\darien.shannon\Documents\Code\F#\FSharp\Structural2\Structural2\bin\Debug\Structural2.dll"
open Structural.SteelDesign

(*** define: Section ***)
let mySection = Plate.create
                    (Inch(3.0), Inch(3.0),
                     SteelMaterial.create (Ksi(50.0), Ksi(60.0), Ksi(29000.0)),
                     None)
(*** include: Section ***)
(*** include-value: mySection ***)

(*** define: test1 ***)
let test1 = Section.area mySection
(*** include: test1 ***)
(*** include-value: test1 ***)

(*** define: test2***)
let test2 = Section.xBar mySection
(*** include: test2 ***)
(*** include-value: test2 ***)

(*** define: test2 ***)
let test3 = Section.yBar mySection
(*** include: test2 ***)
(*** include-value: test2 ***)

(*** define: test3 ***)
let test4 = Section.allowableTension mySection
(*** include: test3 ***)
(*** include-value: test3 ***)
