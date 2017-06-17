open System
open System.Collections.Generic

[<Measure>] type inch
[<Measure>] type lbf
[<Measure>] type kip 
[<Measure>] type psi = lbf/inch^2
[<Measure>] type ksi = kip/inch^2


type SingleAngle = {verticalLeg : float<inch>;
                    horizontalLeg : float<inch>;
                    thickness : float<inch>}

                    static member Create hLeg vLeg t r = {verticalLeg = vLeg;
                                                          horizontalLeg = hLeg;
                                                          thickness = t}

type DoubleAngleOrientation =
    | LLBB
    | SLBB

type DoubleAngle = {verticalLeg : float<inch>;
                    horizontalLeg : float<inch>;
                    thickness : float<inch>;
                    gap : float<inch>}

                    
                    static member Create vLeg hLeg t r gap = {verticalLeg = vLeg;
                                                              horizontalLeg = hLeg;
                                                              thickness = t;
                                                              gap = gap}
                                                              
type StructuralShape =
    | SingleAngle of SingleAngle
    | DoubleAngle of DoubleAngle

    member ss.Area =
        match ss with
        | SingleAngle sa -> (sa.horizontalLeg + sa.verticalLeg - sa.thickness) * sa.thickness
        | DoubleAngle da -> 2.0 * (da.horizontalLeg + da.verticalLeg - da.thickness) * da.thickness

    member ss.x_bar =
        match ss with
        | SingleAngle sa as singleAngle ->
                            (sa.horizontalLeg * sa.thickness * sa.horizontalLeg/2.0 +
                             ((sa.verticalLeg - sa.thickness) * sa.thickness * sa.thickness/2.0)) / singleAngle.Area
        | DoubleAngle _ -> 0.0<inch>

    member ss.y_bar =
        match ss with
        | SingleAngle sa as singleAngle ->
              ((sa.horizontalLeg - sa.thickness) * sa.thickness * sa.thickness/2.0 +
               (sa.verticalLeg * sa.thickness * sa.verticalLeg/2.0)) / singleAngle.Area
        | DoubleAngle da as doubleAngle ->
              ((da.horizontalLeg - da.thickness) * da.thickness * da.thickness/2.0 +
               (da.verticalLeg * da.thickness * da.verticalLeg/2.0)) / (doubleAngle.Area / 2.0)
                             

    member ss.Description =
        match ss with
        | SingleAngle sa -> String.Format("L{0}x{1}x{2}",
                                          sa.verticalLeg, sa.horizontalLeg, sa.thickness)
        | DoubleAngle da -> String.Format("2L{0}x{1}x{2}x{3}",
                                          da.verticalLeg, da.horizontalLeg, da.thickness, da.gap)


type SteelMaterial = {fy : float<kip/inch^2>
                      fu : float<kip/inch^2>
                      E : float<kip/inch^2>}

type Material =
    | SteelMaterial of SteelMaterial

type Calculation<'TVal> = {Description : string;
                           Value : 'TVal;
                           Units : string
                           Reference : string}

                           override t.ToString() =
                               sprintf "%s: %A <%s> (%s)" t.Description t.Value t.Units t.Reference

type Section = {shape : StructuralShape
                material : Material}

                member t.AllowableTension =
                    match t.material with
                    | SteelMaterial sm -> {Description = "Allowable Tension"
                                           Value = (t.shape.Area * sm.fy) / 1.67;
                                           Units = "Kip"
                                           Reference = "AISC"}




