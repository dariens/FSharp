namespace Structural.SteelDesign

open System
open UnitsNet.Units

[<Measure>] type inch
[<Measure>] type lbf
[<Measure>] type kip
[<Measure>] type psi = lbf/inch^2
[<Measure>] type ksi = kip/inch^2

[<StructuredFormatDisplay("{AsString}")>]
type Inch =
    | Inch of float

    override this.ToString() =
        match this with
        | Inch inch -> String.Format("{0:0.0####} <inch>", inch)
    
    member this.AsString = this.ToString()

    member this.inch =
        match this with
        | Inch value -> box value :?> float<inch> 

    static member fromFloatInch (inch : float<inch>) =
        Inch (box inch :?> float)




[<StructuredFormatDisplay("{AsString}")>]
type Ksi =
    | Ksi of float

    override this.ToString() =
        match this with
        | Ksi ksi -> String.Format("{0:0.0####} <ksi>", ksi)
    
    member this.AsString = this.ToString()

    member this.ksi =
        match this with
        | Ksi value -> box value :?> float<ksi>

type ISection = interface end

[<AutoOpen>]
module Materials =

    type SteelMaterial =
        {
        Fy : Ksi
        Fu : Ksi
        E : Ksi
        }   
        
        static member create (Fy, Fu, E) =
            {Fy = Fy; Fu = Fu; E = E}

    

type Rotation =
    | Ninety
    | OneEighty
    | TwoSeventy
        
type Mirror =
    | Vertical
    | Horizontal

type Transformation =
    | Rotate of Rotation
    | Mirror of Mirror


[<AutoOpen>]
module Sections =
    open Materials

    type Plate =
        {
        Length : Inch
        Thickness : Inch
        Material : SteelMaterial
        Transformations : Transformation list option
        }
        interface ISection

        static member create (length, t, material, transformations) =
            {
            Length = length
            Thickness = t
            Material = material
            Transformations = transformations
            }

    let myPlate = (Plate.create (Inch(10.0), Inch(1.0), SteelMaterial.create (Ksi(50.0), Ksi(60.0), Ksi(29000.0)), Some [Rotate Ninety; Mirror Vertical]))

    type SingleAngle =
        {
        VerticalLeg : Inch
        HorizontalLeg : Inch
        Thickness : Inch
        Material : SteelMaterial
        Transformations : Transformation list option
        }
        interface ISection   

        static member create (vLeg, hLeg, t, material, transformations) =
            {
            VerticalLeg = vLeg
            HorizontalLeg = hLeg
            Thickness = t
            Material = material
            Transformations = transformations
            }

    type DoubleAngle =
        {
        VerticalLeg : Inch
        HorizontalLeg : Inch
        Thickness : Inch
        Gap : Inch
        Material : SteelMaterial
        Transformations : Transformation list option
        }
        interface ISection

        static member create (vLeg, hLeg, t, gap, material, transformations) =
            {
            VerticalLeg = vLeg
            HorizontalLeg = hLeg
            Thickness = t
            Gap = gap
            Material = material
            Transformations = transformations
            }


    type CF_SingleAngle =
        {
        VerticalLeg : Inch
        HorizontalLeg : Inch
        Thickness : Inch
        Radius : Inch
        Material : SteelMaterial
        Transformations : Transformation list option
        }
        interface ISection

        member cfsa.Blank = 
            {
            Length =
                Inch.fromFloatInch
                    ((cfsa.HorizontalLeg.inch - cfsa.Radius.inch - cfsa.Thickness.inch) +
                     (cfsa.VerticalLeg.inch - cfsa.Radius.inch - cfsa.Thickness.inch) +
                     (2.0 * Math.PI * (cfsa.Radius.inch + cfsa.Thickness.inch/2.0) * 0.25))
            Thickness = cfsa.Thickness
            Material = cfsa.Material
            Transformations = cfsa.Transformations
            }

        static member create (vLeg, hLeg, t, r, material, transformations) =
            {
            VerticalLeg = vLeg
            HorizontalLeg = hLeg
            Thickness = t
            Radius = r
            Material = material
            Transformations = transformations
            }

        static member createEqualLegFromBlank ((blank: Plate), radius : Inch, material, transformations) =
            let radius' = radius.inch + blank.Thickness.inch/2.0
            let curveLength = 2.0 * Math.PI * radius' * 0.25
            let legLength = (blank.Length.inch - curveLength) / 2.0
            let singleAngle = 
                {
                VerticalLeg = Inch.fromFloatInch legLength
                HorizontalLeg = Inch.fromFloatInch legLength
                Thickness = blank.Thickness
                Radius = radius
                Material = material
                Transformations = transformations
                }
            singleAngle

        static member createUnequalLegFromBlankAndHLeg ((blank: Plate), radius : Inch, hLeg : Inch, material, transformations) =
            let radius' = radius.inch + blank.Thickness.inch/2.0
            let curveLength = 2.0 * Math.PI * radius' * 0.25
            let vLeg = blank.Length.inch - curveLength - hLeg.inch
            let singleAngle =
                {
                VerticalLeg = Inch.fromFloatInch vLeg
                HorizontalLeg = hLeg
                Thickness = blank.Thickness
                Radius = radius
                Material = material
                Transformations = transformations
                }
            singleAngle

        static member createUnequalLegFromBlankAndVLeg ((blank: Plate), radius : Inch, vLeg : Inch, material, transformations) =
            let radius' = radius.inch + blank.Thickness.inch/2.0
            let curveLength = 2.0 * Math.PI * radius' * 0.25
            let hLeg = blank.Length.inch - curveLength - vLeg.inch
            let singleAngle =
                {
                VerticalLeg = vLeg
                HorizontalLeg = Inch.fromFloatInch hLeg
                Thickness = blank.Thickness
                Radius = radius
                Material = material
                Transformations = transformations
                }
            singleAngle


    type CF_DoubleAngle =
        {
        VerticalLeg : Inch
        HorizontalLeg : Inch
        Thickness : Inch
        Radius : Inch
        Gap : Inch
        Material : SteelMaterial
        Transformations : Transformation list option
        }
        interface ISection

        member cfda.Blank : Plate = 
            {
            Length =
                Inch.fromFloatInch
                 ((cfda.HorizontalLeg.inch - cfda.Radius.inch - cfda.Thickness.inch) +
                    (cfda.VerticalLeg.inch - cfda.Radius.inch - cfda.Thickness.inch) +
                    (2.0 * Math.PI * (cfda.Radius.inch + cfda.Thickness.inch/2.0) * 0.25))
            Thickness = cfda.Thickness
            Material = cfda.Material
            Transformations = cfda.Transformations
            }

        static member  create (vLeg, hLeg, t, r, gap, material, transformations) =
            {
            VerticalLeg = vLeg
            HorizontalLeg = hLeg
            Thickness = t
            Radius = r
            Gap = gap
            Material = material
            Transformations = transformations
            }
  
        static member createEqualLegFromBlank ((blank: Plate), radius : Inch, gap : Inch, material, transformations) =
            let radius' = radius.inch + blank.Thickness.inch/2.0
            let curveLength = 2.0 * Math.PI * radius' * 0.25
            let legLength = (blank.Length.inch - curveLength) / 2.0
            let singleAngle = 
                {
                VerticalLeg = Inch.fromFloatInch legLength
                HorizontalLeg = Inch.fromFloatInch legLength
                Thickness = blank.Thickness
                Radius = radius
                Gap = gap
                Material = material
                Transformations = transformations
                }
            singleAngle

        static member createUnequalLegFromBlankAndHLeg ((blank: Plate), radius : Inch, hLeg : Inch, gap : Inch, material, transformations) =
            let radius' = radius.inch + blank.Thickness.inch/2.0
            let curveLength = 2.0 * Math.PI * radius' * 0.25
            let vLeg = blank.Length.inch - curveLength - hLeg.inch
            let doubleAngle =
                {
                VerticalLeg = Inch.fromFloatInch vLeg
                HorizontalLeg = hLeg
                Thickness = blank.Thickness
                Radius = radius
                Gap = gap
                Material = material
                Transformations = transformations
                }
            doubleAngle

        static member createUnequalLegFromBlankAndVLeg ((blank: Plate), radius : Inch, vLeg : Inch, gap : Inch, material, transformations) =
            let radius' = radius.inch + blank.Thickness.inch/2.0
            let curveLength = 2.0 * Math.PI * radius' * 0.25
            let hLeg = blank.Length.inch - curveLength - vLeg.inch
            let doubleAngle =
                {
                VerticalLeg = vLeg
                HorizontalLeg = Inch.fromFloatInch hLeg
                Thickness = blank.Thickness
                Radius = radius
                Gap = gap
                Material = material
                Transformations = transformations
                }
            doubleAngle

    type GenericProperties =
        {
        Description : string
        Area : float<inch^2>
        XBar: float<inch>
        YBar: float<inch>
        }

    type GenericSection =
        {
        GenericProperties : GenericProperties
        Material : SteelMaterial
        Transformations : Transformation list option
        }
        interface ISection

        static member create (description, area, xBar, yBar, material, transformations) =         
            {
            GenericProperties =
                {
                Description = description
                Area = area
                XBar = xBar
                YBar = yBar
                }
            Material = material
            Transformations = transformations
            }

    type ISection with
        member this.Material =
            match this with
            | :? Plate as pl -> pl.Material
            | :? SingleAngle as sa -> sa.Material
            | :? DoubleAngle as da -> da.Material
            | :? CF_SingleAngle as cfsa -> cfsa.Material
            | :? CF_DoubleAngle as cfda -> cfda.Material
            | :? GenericSection as gs -> gs.Material
            | _ -> failwith "Type not supported"
        member this.Transformations =
            match this with
            | :? Plate as pl -> pl.Transformations
            | :? SingleAngle as sa -> sa.Transformations
            | :? DoubleAngle as da -> da.Transformations
            | :? CF_SingleAngle as cfsa -> cfsa.Transformations
            | :? CF_DoubleAngle as cfda -> cfda.Transformations
            | :? GenericSection as gs -> gs.Transformations
            | _ -> failwith "Type not supported"


[<StructuredFormatDisplay("{AsString}")>]
    type Calculation<'T> =
        {Result : 'T
         Units : string option
         Reference : string option}

         override this.ToString() =
             let reference =
                 match this.Reference with
                 | Some value -> sprintf " [%s]" value
                 | None -> ""
             let units =
                 match this.Units with
                 | Some value -> sprintf " <%s>" value
                 | None -> ""
             sprintf "%A%s%s" this.Result units reference

         member this.AsString = this.ToString()

         static member create (result, units, reference) : Calculation<'T>=
             {Result = result
              Reference = reference
              Units = units}


[<RequireQualifiedAccess>]
module Section =
    open Sections

    let description (shape : ISection) =
        match shape with
        | :? Plate as pl -> String.Format("PL{0}x{1}", pl.Length.inch, pl.Thickness.inch)
        | :? SingleAngle as sa ->
            String.Format("L{0}x{1}x{2}", sa.VerticalLeg.inch, sa.HorizontalLeg.inch, sa.Thickness.inch)
        | :? DoubleAngle as da -> 
            String.Format("2L{0}x{1}x{2}x{3}", da.VerticalLeg.inch,
                da.HorizontalLeg.inch, da.Thickness.inch, da.Gap.inch)
        | :? CF_SingleAngle as cfsa ->
            String.Format("CFL{0}x{1}x{2}r{3}",
                cfsa.VerticalLeg.inch, cfsa.HorizontalLeg.inch,
                cfsa.Thickness.inch, cfsa.Radius.inch)
        | :? CF_DoubleAngle as cfda ->
            String.Format("CF2L{0}x{1}x{2}x{3}r{4}",
                cfda.VerticalLeg.inch, cfda.HorizontalLeg.inch,
                cfda.Thickness.inch, cfda.Gap.inch, cfda.Radius.inch)
        | :? GenericSection as gs -> gs.GenericProperties.Description
        | _ -> failwith "Type not supported."


    let rec area (shape : ISection) =
        let result =
            match shape with
            | :? Plate as pl -> pl.Length.inch * pl.Thickness.inch
            | :? SingleAngle as sa ->
                (sa.HorizontalLeg.inch + sa.VerticalLeg.inch - sa.Thickness.inch) * sa.Thickness.inch
            | :? DoubleAngle as da ->
                2.0 * (da.HorizontalLeg.inch + da.VerticalLeg.inch - da.Thickness.inch) * da.Thickness.inch
            | :? CF_SingleAngle as cfsa -> (area (cfsa.Blank)).Result
            | :? CF_DoubleAngle as cfda -> 2.0 * (area (cfda.Blank)).Result
            | :? GenericSection as gs -> gs.GenericProperties.Area               
            | _ -> failwith "Type not supported."
        Calculation.create (result, units = (Some "in^2"), reference = None) 
        


    let xBar (shape : ISection) =
        let result = 
            match shape with
            | :? Plate as pl -> pl.Thickness.inch/2.0
            | :? SingleAngle as sa ->
                (sa.HorizontalLeg.inch * sa.Thickness.inch * sa.HorizontalLeg.inch/2.0 +
                    ((sa.VerticalLeg.inch - sa.Thickness.inch) * sa.Thickness.inch * sa.Thickness.inch/2.0))
                    / (area sa).Result
            | :? DoubleAngle -> 0.0<inch>
            | :? GenericSection as gs -> gs.GenericProperties.XBar
            | _ -> failwith "Type not supported."
        Calculation.create (result, (Some "inch"), None)
        
    let yBar (shape : ISection) =
        let result =
            match shape with
            | :? Plate as pl -> pl.Length.inch/2.0
            | :? SingleAngle as sa ->
                ((sa.HorizontalLeg.inch - sa.Thickness.inch) * sa.Thickness.inch * sa.Thickness.inch/2.0 +
                    (sa.VerticalLeg.inch * sa.Thickness.inch * sa.VerticalLeg.inch/2.0))
                    / (area sa).Result
            | :? DoubleAngle as da ->
                ((da.HorizontalLeg.inch - da.Thickness.inch) * da.Thickness.inch * da.Thickness.inch/2.0 +
                    (da.VerticalLeg.inch * da.Thickness.inch * da.VerticalLeg.inch/2.0))
                    / ((area da).Result / 2.0)
            | :? GenericSection as gs -> gs.GenericProperties.YBar
            | _ -> failwith "Type not supported."
        Calculation.create (result, (Some "inch"), None)

    let apply f (section: ISection) = f section

    let allowableTension (section : ISection)=
        let result = (area section).Result * section.Material.Fy.ksi : float<kip>
        Calculation.create (result, (Some "kip"), None)


[<AutoOpen>]
module ISectionExtensions =
    type ISection with
        member this.Description = Section.description this
        member this.Area = Section.area this
        member this.XBar = Section.xBar this
        member this.YBar = Section.yBar this






