//#r @"C:\Users\user\Documents\CODE\F#\Expert F# 4.0\Structural\Structural\bin\Debug\Structural.dll"
#r @"C:\Users\darien.shannon\Documents\Code\F#\Expert-FSharp-4.0\Structural\Structural\bin\Debug\Structural.dll"
open Structural
open Structural.Shapes
open FSharp.Reflection

//#load @"C:\Users\user\Documents\CODE\F#\Expert F# 4.0\Structural\Structural\Materials.fsx"
//#load @"C:\Users\user\Documents\CODE\F#\Expert F# 4.0\Structural\Structural\ShapesDatabase.fsx"

//#load @"C:\Users\darien.shannon\Documents\Code\F#\Expert-FSharp-4.0\Structural\Structural\MaterialDatabase.fsx"
//#load @"C:\Users\darien.shannon\Documents\Code\F#\Expert-FSharp-4.0\Structural\Structural\ShapeDatabase.fsx"
#load @"C:\Users\darien.shannon\Documents\Code\F#\Expert-FSharp-4.0\Structural\Structural\SectionDatabase.fsx"

open SectionDatabase.Sections

let mySection = NMBS.SingleAngles.A44A

printfn "Section :%s" mySection.Shape.Description
printfn "Fu : %f" mySection.Material.Fu
printfn "Properties: %A" mySection.Shape.DesignProperties


let myShapes =
    [for leg in [1.0<inch> .. 0.25<inch> .. 6.0<inch>] do
        for thickness in [0.125<inch> .. 0.125<inch> .. 0.375<inch>] do
          let angle = SingleAngle.create leg leg thickness
          let material = SteelMaterial.create 50.0<ksi> 60.0<ksi> 29000.0<ksi>
          let transformations = None
          yield Section.create angle material transformations]

myShapes |> List.iter (fun shape -> printfn "Description = %s" shape.Shape.Description)

let (|MeetsArea|_|) reqArea (section : Section<_,_>) =
    if section.GetType() = typeof<Section<CF_SingleAngle.T, _>> then
        let section = (box section) :?> Section<CF_SingleAngle.T, IMaterial>
        if section.Shape.DesignProperties.Area > reqArea then Some MeetsArea
        else None
    else
        None




let Test =
    match mySection with
    | MeetsArea 3.0<inch^2> -> true
    | _ -> false


let (|MeetsAreaReq2|_|) reqArea (section : Section<_,_>) =
    match box section with
    | :? Section<Plate.T, _> as this ->
        let area = this.Shape.DesignProperties.Area
        if area >= reqArea then Some MeetsAreaReq2 else None
    | :? Section<SingleAngle.T, _> as this ->
        let area = this.Shape.DesignProperties.Area
        if area >= reqArea then Some MeetsAreaReq2 else None
    | :? Section<DoubleAngle.T, _> as this ->
        let area = this.Shape.DesignProperties.Area
        if area >= reqArea then Some MeetsAreaReq2 else None
    | :? Section<CF_SingleAngle.T, _> as this ->
        let area = this.Shape.DesignProperties.Area
        if area >= reqArea then Some MeetsAreaReq2 else None
    | :? Section<CF_DoubleAngle.T, _> as this ->
        let area = this.Shape.DesignProperties.Area
        if area >= reqArea then Some MeetsAreaReq2 else None
    | _ -> failwith "Case not handled"






