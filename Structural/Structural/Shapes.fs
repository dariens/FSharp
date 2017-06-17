namespace Structural

open System

[<Measure>] type inch
[<Measure>] type lbf
[<Measure>] type kip
[<Measure>] type psi = lbf/inch^2
[<Measure>] type ksi = kip/inch^2

type IShape = interface end
module Plate =

    type T =
        {
        Length : float<inch>
        Thickness : float<inch>
        }
        interface IShape

    let create length t=
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

type Shape =
    | Plate of Plate.T
    | SingleAngle of SingleAngle.T
    | DoubleAngle of DoubleAngle.T
    | CF_SingleAngle of CF_SingleAngle.T
    | CF_DoubleAngle of CF_DoubleAngle.T

type DesignProperties =
    {
    Area : float<inch ^2>
    XBar : float<inch>
    YBar : float<inch>
    }

module ShapeOps =

    let description shape =
        match shape with
        | Plate pl -> String.Format("PL{0}x{1}", pl.Length, pl.Thickness)
        | SingleAngle sa ->
            String.Format("L{0}x{1}x{2}", sa.VerticalLeg, sa.HorizontalLeg, sa.Thickness)
        | DoubleAngle da -> 
            String.Format("2L{0}x{1}x{2}x{3}", da.VerticalLeg,
                da.HorizontalLeg, da.Thickness, da.Gap)
        | CF_SingleAngle cfsa ->
            String.Format("CFL{0}x{1}x{2}r{3}",
                cfsa.VerticalLeg, cfsa.HorizontalLeg,
                cfsa.Thickness, cfsa.Radius)
        | CF_DoubleAngle cfda ->
            String.Format("CF2L{0}x{1}x{2}x{3}r{4}",
                cfda.VerticalLeg, cfda.HorizontalLeg,
                cfda.Thickness, cfda.Gap, cfda.Radius)

    let depth shape =
        match shape with
        | Plate pl -> pl.Length
        | SingleAngle sa -> sa.VerticalLeg
        | DoubleAngle da -> da.VerticalLeg
        | CF_SingleAngle cfsa -> cfsa.VerticalLeg
        | CF_DoubleAngle cfda -> cfda.VerticalLeg

    let width shape =
        match shape with
        | Plate pl -> pl.Thickness
        | SingleAngle sa -> sa.HorizontalLeg
        | DoubleAngle da -> da.HorizontalLeg
        | CF_SingleAngle cfsa -> cfsa.HorizontalLeg
        | CF_DoubleAngle cfda -> cfda.HorizontalLeg

    let rec area shape =
        match shape with
        | Plate pl -> pl.Length * pl.Thickness
        | SingleAngle sa ->
            (sa.HorizontalLeg + sa.VerticalLeg - sa.Thickness) * sa.Thickness
        | DoubleAngle da ->
            2.0 * (da.HorizontalLeg + da.VerticalLeg - da.Thickness) * da.Thickness
        | CF_SingleAngle cfsa -> area (Plate cfsa.Blank)
        | CF_DoubleAngle cfda -> 2.0 * (area (Plate cfda.Blank))

    let xBar shape =
        match shape with
        | Plate pl -> pl.Thickness/2.0
        | SingleAngle sa ->
            (sa.HorizontalLeg * sa.Thickness * sa.HorizontalLeg/2.0 +
             ((sa.VerticalLeg - sa.Thickness) * sa.Thickness * sa.Thickness/2.0))
             / (area (SingleAngle sa))
        | DoubleAngle da -> 0.0<inch>
        | CF_SingleAngle cfsa -> -1.0<inch>
        | CF_DoubleAngle cfda -> -1.0<inch>

    let yBar shape =
        match shape with
        | Plate pl -> pl.Length/2.0
        | SingleAngle sa ->
            ((sa.HorizontalLeg - sa.Thickness) * sa.Thickness * sa.Thickness/2.0 +
             (sa.VerticalLeg * sa.Thickness * sa.VerticalLeg/2.0))
             / (area (SingleAngle sa))
        | DoubleAngle da ->
            ((da.HorizontalLeg - da.Thickness) * da.Thickness * da.Thickness/2.0 +
             (da.VerticalLeg * da.Thickness * da.VerticalLeg/2.0))
             / ((area (DoubleAngle da)) / 2.0)
        | CF_SingleAngle cfsa -> -1.0<inch>
        | CF_DoubleAngle cfda -> -1.0<inch>


    let designProperties shape =
        {
        Area = area shape
        XBar = xBar shape
        YBar = yBar shape
        }

    type Shape with
        member s.DesignProperties = designProperties s
        member s.Descrition = s.Descrition


[<AutoOpen>]
module Shapes =
    open ShapeOps

    type Plate.T with
        member pl.Description = description (Plate pl)
        member pl.DesignProperties = designProperties (Plate pl)

    type SingleAngle.T with
        member sa.Description = description (SingleAngle sa)
        member sa.DesignProperties = designProperties (SingleAngle sa)

    type DoubleAngle.T with
        member da.Description = description (DoubleAngle da)
        member da.DesignProperties = designProperties (DoubleAngle da)

    type CF_SingleAngle.T with
        member cfsa.Description = description (CF_SingleAngle cfsa)
        member cfsa.DesignProperties = designProperties (CF_SingleAngle cfsa)

    type CF_DoubleAngle.T with
        member cfda.Description = description (CF_DoubleAngle cfda)
        member cfda.DesignProperties = designProperties (CF_DoubleAngle cfda)






    


        



    




    
    



    
    

    

