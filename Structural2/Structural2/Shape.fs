namespace Structural.SteelDesign

open System

[<Measure>] type inch
[<Measure>] type lbf
[<Measure>] type kip
[<Measure>] type psi = lbf/inch^2
[<Measure>] type ksi = kip/inch^2

type IShape = interface end

[<AutoOpen>]
module Shapes =

        module Plate =

            type T =
                {
                Length : float<inch>
                Thickness : float<inch>
                }
                interface IShape

            let create length t =
                {
                Length = length
                Thickness = t
                }

        module SingleAngle =
    
            type T =
                {
                VerticalLeg : float<inch>
                HorizontalLeg : float<inch>
                Thickness : float<inch>
                }
                interface IShape   

            let create vLeg hLeg t =
                {
                VerticalLeg = vLeg
                HorizontalLeg = hLeg
                Thickness = t
                }

        module DoubleAngle =
    
            type T =
                {
                VerticalLeg : float<inch>
                HorizontalLeg : float<inch>
                Thickness : float<inch>
                Gap : float<inch>
                }
                interface IShape

            let create vLeg hLeg t gap=
                {
                VerticalLeg = vLeg
                HorizontalLeg = hLeg
                Thickness = t
                Gap = gap
                }


        module CF_SingleAngle =

            type T =
                {
                VerticalLeg : float<inch>
                HorizontalLeg : float<inch>
                Thickness : float<inch>
                Radius : float<inch>
                }
                interface IShape

                member cfsa.Blank : Plate.T = 
                    {
                    Length =
                        ((cfsa.HorizontalLeg - cfsa.Radius - cfsa.Thickness) +
                            (cfsa.VerticalLeg - cfsa.Radius - cfsa.Thickness) +
                            (2.0 * Math.PI * (cfsa.Radius + cfsa.Thickness/2.0) * 0.25))
                    Thickness = cfsa.Thickness
                    }

            let create vLeg hLeg t r =
                {
                VerticalLeg = vLeg
                HorizontalLeg = hLeg
                Thickness = t
                Radius = r
                }

            let createEqualLegFromBlank (blank: Plate.T) radius =
                let radius' = radius + blank.Thickness/2.0
                let curveLength = 2.0 * Math.PI * radius' * 0.25
                let legLength = (blank.Length - curveLength) / 2.0
                let singleAngle = 
                    {
                    VerticalLeg = legLength
                    HorizontalLeg = legLength
                    Thickness = blank.Thickness
                    Radius = radius
                    }
                singleAngle

            let createUnequalLegFromBlankAndHLeg (blank: Plate.T) radius hLeg =
                let radius' = radius + blank.Thickness/2.0
                let curveLength = 2.0 * Math.PI * radius' * 0.25
                let vLeg = blank.Length - curveLength - hLeg
                let singleAngle =
                    {
                    VerticalLeg = vLeg
                    HorizontalLeg = hLeg
                    Thickness = blank.Thickness
                    Radius = radius
                    }
                singleAngle

            let createUnequalLegFromBlankAndVLeg (blank: Plate.T) radius vLeg =
                let radius' = radius + blank.Thickness/2.0
                let curveLength = 2.0 * Math.PI * radius' * 0.25
                let hLeg = blank.Length - curveLength - vLeg
                let singleAngle =
                    {
                    VerticalLeg = vLeg
                    HorizontalLeg = hLeg
                    Thickness = blank.Thickness
                    Radius = radius
                    }
                singleAngle


        module CF_DoubleAngle =

            type T =
                {
                VerticalLeg : float<inch>
                HorizontalLeg : float<inch>
                Thickness : float<inch>
                Radius : float<inch>
                Gap : float<inch>
                }
                interface IShape

                member cfda.Blank : Plate.T = 
                    {
                    Length =
                        ((cfda.HorizontalLeg - cfda.Radius - cfda.Thickness) +
                            (cfda.VerticalLeg - cfda.Radius - cfda.Thickness) +
                            (2.0 * Math.PI * (cfda.Radius + cfda.Thickness/2.0) * 0.25))
                    Thickness = cfda.Thickness
                    }

            let create vLeg hLeg t r gap =
                {
                VerticalLeg = vLeg
                HorizontalLeg = hLeg
                Thickness = t
                Radius = r
                Gap = gap
                }
  
            let createEqualLegFromBlank (blank: Plate.T) radius gap =
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
                    }
                singleAngle

            let createUnequalLegFromBlankAndHLeg (blank: Plate.T) radius hLeg gap=
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
                    }
                doubleAngle

            let createUnequalLegFromBlankAndVLeg (blank: Plate.T) radius vLeg gap=
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
                    }
                doubleAngle

[<AutoOpen>]
module ShapeOps =

        let description (shape : IShape) =
            match shape with
            | :? Plate.T as pl -> String.Format("PL{0}x{1}", pl.Length, pl.Thickness)
            | :? SingleAngle.T as sa ->
                String.Format("L{0}x{1}x{2}", sa.VerticalLeg, sa.HorizontalLeg, sa.Thickness)
            | :? DoubleAngle.T as da -> 
                String.Format("2L{0}x{1}x{2}x{3}", da.VerticalLeg,
                    da.HorizontalLeg, da.Thickness, da.Gap)
            | :? CF_SingleAngle.T as cfsa ->
                String.Format("CFL{0}x{1}x{2}r{3}",
                    cfsa.VerticalLeg, cfsa.HorizontalLeg,
                    cfsa.Thickness, cfsa.Radius)
            | :? CF_DoubleAngle.T as cfda ->
                String.Format("CF2L{0}x{1}x{2}x{3}r{4}",
                    cfda.VerticalLeg, cfda.HorizontalLeg,
                    cfda.Thickness, cfda.Gap, cfda.Radius)
            | _ -> failwith "Type not supported."

        let rec area (shape : IShape) =
            match shape with
            | :? Plate.T as pl -> pl.Length * pl.Thickness
            | :? SingleAngle.T as sa ->
                (sa.HorizontalLeg + sa.VerticalLeg - sa.Thickness) * sa.Thickness
            | :? DoubleAngle.T as da ->
                2.0 * (da.HorizontalLeg + da.VerticalLeg - da.Thickness) * da.Thickness
            | :? CF_SingleAngle.T as cfsa -> area (cfsa.Blank)
            | :? CF_DoubleAngle.T as cfda -> 2.0 * area (cfda.Blank)               
            | _ -> failwith "Type not supported."

        let xBar (shape : IShape) =
            match shape with
            | :? Plate.T as pl -> pl.Thickness/2.0
            | :? SingleAngle.T as sa ->
                (sa.HorizontalLeg * sa.Thickness * sa.HorizontalLeg/2.0 +
                 ((sa.VerticalLeg - sa.Thickness) * sa.Thickness * sa.Thickness/2.0))
                 / (area sa)
            | :? DoubleAngle.T -> 0.0<inch>
            | :? CF_SingleAngle.T -> -1.0<inch>
            | :? CF_DoubleAngle.T -> -1.0<inch>
            | _ -> failwith "Type not supported."
        
        let yBar (shape : IShape) =
            match shape with
            | :? Plate.T as pl -> pl.Length/2.0
            | :? SingleAngle.T as sa ->
                ((sa.HorizontalLeg - sa.Thickness) * sa.Thickness * sa.Thickness/2.0 +
                 (sa.VerticalLeg * sa.Thickness * sa.VerticalLeg/2.0))
                 / (area sa)
            | :? DoubleAngle.T as da ->
                ((da.HorizontalLeg - da.Thickness) * da.Thickness * da.Thickness/2.0 +
                 (da.VerticalLeg * da.Thickness * da.VerticalLeg/2.0))
                 / ((area da) / 2.0)
            | :? CF_SingleAngle.T as cfsa -> -1.0<inch>
            | :? CF_DoubleAngle.T as cfda -> -1.0<inch>
            | _ -> failwith "Type not supported."


        type IShape with
            member this.Description = description this
            member this.Area = area this
            member this.XBar = xBar this
            member this.YBar = yBar this
            
     
[<AutoOpen>]
module Material =

    type T =
        {
        Fy : float<ksi>
        Fu : float<ksi>
        E : float<ksi>
        }
                
    let create Fy Fu E = {Fy = Fy; Fu = Fu; E = E}

[<AutoOpen>]
module Transformations =
        
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
module Section =

    type T<'TShape when 'TShape :> IShape> =
        {
        Shape : 'TShape
        Material : Material.T
        Transformations : Transformation list option
        }

    let create shape material transformations =
        {
        Shape = shape
        Material = material
        Transformations = transformations
        }

    let allowableTension section =
        section.Shape.Area * section.Material.Fy

    let testDesignFunction (holeSize : float<inch^2>) section : float<kip> =
        let shape = section.Shape :> IShape
        match shape with
        | :? Plate.T as this -> (this.Area - holeSize) * section.Material.Fy 
        | :? SingleAngle.T as this -> (this.Area - holeSize) * section.Material.Fy
        | _ -> failwith "Shape not supported."



