#load @"Structural.fsx"
open Structural

type ASTM =
    static member ASTM1 = SteelMaterial {fy = 50.0<ksi>; fu = 60.0<ksi>; E = 29000.0<ksi>}
    static member ASTM2 = SteelMaterial {fy = 33.0<ksi>; fu = 60.0<ksi>; E = 29000.0<ksi>}