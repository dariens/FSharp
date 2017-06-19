namespace Structural.SteelDesign

open System
open UnitsNet.Units

[<Measure>] type inch
[<Measure>] type lbf
[<Measure>] type kip
[<Measure>] type psi = lbf/inch^2
[<Measure>] type ksi = kip/inch^2

type ISection = interface end

[<AutoOpen>]
module Materials =

    [<StructuredFormatDisplay("{AsString}")>]
    type SteelMaterial =
        {
        Fy : float<ksi>
        Fu : float<ksi>
        E : float<ksi>
        }   
        
        static member create (Fy, Fu, E) = {Fy = Fy; Fu = Fu; E = E}

        override this.ToString() =
            System.String.Format("\r\n{{\r\nFy = {0:0.0####} <ksi>\r\nFu = {1:0.0####} <ksi>\r\nE = {2:0.0####} <ksi>\r\n}}", this.Fy, this.Fu, this.E)
        member this.AsString = this.ToString()
     


module test =
    let myMaterial = (SteelMaterial.create (50.0<ksi>, 60.0<ksi>, 29000.0<ksi>))


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

    [<StructuredFormatDisplay("{AsString}")>]
    type Plate =
        {
        Length : float<inch>
        Thickness : float<inch>
        Material : SteelMaterial
        Transformations : Transformation list option
        }
        interface ISection

        override this.ToString() =
            let transformations = 
                match this.Transformations with
                | Some value -> box value
                | None -> box "null"
            let material =
                let lines = this.Material.AsString.Split([|"\r\n"|], StringSplitOptions.None )
                [for line in lines do
                    yield "          " + line]
                |> List.fold (fun r s -> r + s + "\r\n") ("")
                   
            System.String.Format("Plate =\r\n{{\r\nLength = {0:0.0####} <inch>\r\nThickness = {1:0.0####} <inch>\r\nMaterial = {2:0.0####}\r\nTransformations = {3}\r\n}}",
                this.Length, this.Thickness, material, transformations)
        member this.AsString = this.ToString()

        static member create (length, t, material, transformations) =
            {
            Length = length
            Thickness = t
            Material = material
            Transformations = transformations
            }

    let myPlate = (Plate.create (10.0<inch>, 1.0<inch>, SteelMaterial.create (50.0<ksi>, 60.0<ksi>, 29000.0<ksi>), Some [Rotate Ninety; Mirror Vertical]))

    type SingleAngle =
        {
        VerticalLeg : float<inch>
        HorizontalLeg : float<inch>
        Thickness : float<inch>
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
        VerticalLeg : float<inch>
        HorizontalLeg : float<inch>
        Thickness : float<inch>
        Gap : float<inch>
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
        VerticalLeg : float<inch>
        HorizontalLeg : float<inch>
        Thickness : float<inch>
        Radius : float<inch>
        Material : SteelMaterial
        Transformations : Transformation list option
        }
        interface ISection

        member cfsa.Blank = 
            {
            Length =
                ((cfsa.HorizontalLeg - cfsa.Radius - cfsa.Thickness) +
                    (cfsa.VerticalLeg - cfsa.Radius - cfsa.Thickness) +
                    (2.0 * Math.PI * (cfsa.Radius + cfsa.Thickness/2.0) * 0.25))
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

        static member createEqualLegFromBlank ((blank: Plate), radius, material, transformations) =
            let radius' = radius + blank.Thickness/2.0
            let curveLength = 2.0 * Math.PI * radius' * 0.25
            let legLength = (blank.Length - curveLength) / 2.0
            let singleAngle = 
                {
                VerticalLeg = legLength
                HorizontalLeg = legLength
                Thickness = blank.Thickness
                Radius = radius
                Material = material
                Transformations = transformations
                }
            singleAngle

        static member createUnequalLegFromBlankAndHLeg ((blank: Plate), radius, hLeg, material, transformations) =
            let radius' = radius + blank.Thickness/2.0
            let curveLength = 2.0 * Math.PI * radius' * 0.25
            let vLeg = blank.Length - curveLength - hLeg
            let singleAngle =
                {
                VerticalLeg = vLeg
                HorizontalLeg = hLeg
                Thickness = blank.Thickness
                Radius = radius
                Material = material
                Transformations = transformations
                }
            singleAngle

        static member createUnequalLegFromBlankAndVLeg ((blank: Plate), radius, vLeg, material, transformations) =
            let radius' = radius + blank.Thickness/2.0
            let curveLength = 2.0 * Math.PI * radius' * 0.25
            let hLeg = blank.Length - curveLength - vLeg
            let singleAngle =
                {
                VerticalLeg = vLeg
                HorizontalLeg = hLeg
                Thickness = blank.Thickness
                Radius = radius
                Material = material
                Transformations = transformations
                }
            singleAngle


    type CF_DoubleAngle =
        {
        VerticalLeg : float<inch>
        HorizontalLeg : float<inch>
        Thickness : float<inch>
        Radius : float<inch>
        Gap : float<inch>
        Material : SteelMaterial
        Transformations : Transformation list option
        }
        interface ISection

        member cfda.Blank : Plate = 
            {
            Length =
                ((cfda.HorizontalLeg - cfda.Radius - cfda.Thickness) +
                    (cfda.VerticalLeg - cfda.Radius - cfda.Thickness) +
                    (2.0 * Math.PI * (cfda.Radius + cfda.Thickness/2.0) * 0.25))
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
  
        static member createEqualLegFromBlank ((blank: Plate), radius, gap, material, transformations) =
            let radius' = radius + blank.Thickness/2.0
            let curveLength = 2.0 * Math.PI * radius' * 0.25
            let legLength = (blank.Length - curveLength) / 2.0
            let singleAngle = 
                {
                VerticalLeg = legLength
                HorizontalLeg = legLength
                Thickness = blank.Thickness
                Radius = radius
                Gap = gap
                Material = material
                Transformations = transformations
                }
            singleAngle

        static member createUnequalLegFromBlankAndHLeg ((blank: Plate), radius, hLeg, gap, material, transformations) =
            let radius' = radius + blank.Thickness/2.0
            let curveLength = 2.0 * Math.PI * radius' * 0.25
            let vLeg = blank.Length - curveLength - hLeg
            let doubleAngle =
                {
                VerticalLeg = vLeg
                HorizontalLeg = hLeg
                Thickness = blank.Thickness
                Radius = radius
                Gap = gap
                Material = material
                Transformations = transformations
                }
            doubleAngle

        static member createUnequalLegFromBlankAndVLeg ((blank: Plate), radius, vLeg, gap, material, transformations) =
            let radius' = radius + blank.Thickness/2.0
            let curveLength = 2.0 * Math.PI * radius' * 0.25
            let hLeg = blank.Length - curveLength - vLeg
            let doubleAngle =
                {
                VerticalLeg = vLeg
                HorizontalLeg = hLeg
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

[<RequireQualifiedAccess>]
module Section =
    open Sections

    let description (shape : ISection) =
        match shape with
        | :? Plate as pl -> String.Format("PL{0}x{1}", pl.Length, pl.Thickness)
        | :? SingleAngle as sa ->
            String.Format("L{0}x{1}x{2}", sa.VerticalLeg, sa.HorizontalLeg, sa.Thickness)
        | :? DoubleAngle as da -> 
            String.Format("2L{0}x{1}x{2}x{3}", da.VerticalLeg,
                da.HorizontalLeg, da.Thickness, da.Gap)
        | :? CF_SingleAngle as cfsa ->
            String.Format("CFL{0}x{1}x{2}r{3}",
                cfsa.VerticalLeg, cfsa.HorizontalLeg,
                cfsa.Thickness, cfsa.Radius)
        | :? CF_DoubleAngle as cfda ->
            String.Format("CF2L{0}x{1}x{2}x{3}r{4}",
                cfda.VerticalLeg, cfda.HorizontalLeg,
                cfda.Thickness, cfda.Gap, cfda.Radius)
        | :? GenericSection as gs -> gs.GenericProperties.Description
        | _ -> failwith "Type not supported."

    let rec area (shape : ISection) =
        match shape with
        | :? Plate as pl -> pl.Length * pl.Thickness
        | :? SingleAngle as sa ->
            (sa.HorizontalLeg + sa.VerticalLeg - sa.Thickness) * sa.Thickness
        | :? DoubleAngle as da ->
            2.0 * (da.HorizontalLeg + da.VerticalLeg - da.Thickness) * da.Thickness
        | :? CF_SingleAngle as cfsa -> area (cfsa.Blank)
        | :? CF_DoubleAngle as cfda -> 2.0 * area (cfda.Blank)
        | :? GenericSection as gs -> gs.GenericProperties.Area               
        | _ -> failwith "Type not supported."

    let xBar (shape : ISection) =
        match shape with
        | :? Plate as pl -> pl.Thickness/2.0
        | :? SingleAngle as sa ->
            (sa.HorizontalLeg * sa.Thickness * sa.HorizontalLeg/2.0 +
                ((sa.VerticalLeg - sa.Thickness) * sa.Thickness * sa.Thickness/2.0))
                / (area sa)
        | :? DoubleAngle -> 0.0<inch>
        | :? GenericSection as gs -> gs.GenericProperties.XBar
        | _ -> failwith "Type not supported."
        
    let yBar (shape : ISection) =
        match shape with
        | :? Plate as pl -> pl.Length/2.0
        | :? SingleAngle as sa ->
            ((sa.HorizontalLeg - sa.Thickness) * sa.Thickness * sa.Thickness/2.0 +
                (sa.VerticalLeg * sa.Thickness * sa.VerticalLeg/2.0))
                / (area sa)
        | :? DoubleAngle as da ->
            ((da.HorizontalLeg - da.Thickness) * da.Thickness * da.Thickness/2.0 +
                (da.VerticalLeg * da.Thickness * da.VerticalLeg/2.0))
                / ((area da) / 2.0)
        | :? GenericSection as gs -> gs.GenericProperties.YBar
        | _ -> failwith "Type not supported."

    let apply f (section: ISection) = f section

    let allowableTension (section : ISection) : float<kip> =
        (area section) * section.Material.Fy



[<AutoOpen>]
module ISectionExtensions =
    type ISection with
        member this.Description = Section.description this
        member this.Area = Section.area this
        member this.XBar = Section.xBar this
        member this.YBar = Section.yBar this






