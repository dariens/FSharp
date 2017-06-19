namespace Structural.SteelDesign

open System

[<Measure>] type inch
[<Measure>] type lbf
[<Measure>] type kip
[<Measure>] type psi = lbf/inch^2
[<Measure>] type ksi = kip/inch^2

type ISection = interface end

[<AutoOpen>]
module Materials =

    type SteelMaterial =
        {
        Fy : float<ksi>
        Fu : float<ksi>
        E : float<ksi>
        }   
        
        static member create (Fy, Fu, E) = {Fy = Fy; Fu = Fu; E = E}

open Materials


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
        Length : float<inch>
        Thickness : float<inch>
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

    type ISection with
        member this.Material =
            match this with
            | :? Plate as pl -> pl.Material
            | :? SingleAngle as sa -> sa.Material
            | :? DoubleAngle as da -> da.Material
            | :? CF_SingleAngle as cfsa -> cfsa.Material
            | :? CF_DoubleAngle as cfda -> cfda.Material
            | _ -> failwith "Type not supported"
        member this.Transformations =
            match this with
            | :? Plate as pl -> pl.Transformations
            | :? SingleAngle as sa -> sa.Transformations
            | :? DoubleAngle as da -> da.Transformations
            | :? CF_SingleAngle as cfsa -> cfsa.Transformations
            | :? CF_DoubleAngle as cfda -> cfda.Transformations
            | _ -> failwith "Type not supported"

[<AutoOpen>]
module SectionOps =
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
        | _ -> failwith "Type not supported."

    let xBar (shape : ISection) =
        match shape with
        | :? Plate as pl -> pl.Thickness/2.0
        | :? SingleAngle as sa ->
            (sa.HorizontalLeg * sa.Thickness * sa.HorizontalLeg/2.0 +
                ((sa.VerticalLeg - sa.Thickness) * sa.Thickness * sa.Thickness/2.0))
                / (area sa)
        | :? DoubleAngle -> 0.0<inch>
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
        | _ -> failwith "Type not supported."

    type ISection with
        member this.Description = description this
        member this.Area = area this
        member this.XBar = xBar this
        member this.YBar = yBar this

    let allowableTension (section : ISection) : float<kip> =
        section.Area * section.Material.Fy

    let apply f (section: ISection) = f section



module Test =
    open Sections
    open SectionOps

    let mySection = SingleAngle.create (3.0<inch>, 3.0<inch>, 0.25<inch>,
                                        SteelMaterial.create (50.0<ksi>, 60.0<ksi>, 29000.0<ksi>),
                                        None)

    let myArea = mySection.Area
    let myArea2 = area mySection
    let myMaterialFy = mySection.Material.Fy
    let allowableTension = allowableTension mySection

    let apply f (section: ISection) = f section

    mySection |> apply (fun section -> printfn "Section Area = %f" section.Area)

    let newSection = {mySection with Material = {mySection.Material with Fy = 36.0<ksi>; Fu = 50.0<ksi>}}

    let myPlate = Plate.create (10.0<inch>, 1.0<inch>,
                                SteelMaterial.create (50.0<ksi>, 60.0<ksi>, 29000.0<ksi>),
                                None)



