#r @"C:\Users\user\Documents\CODE\F#\FSharp\Structural2\Structural2\bin\Debug\Structural2.dll"
open Structural.SteelDesign

#load @"C:\Users\user\Documents\CODE\F#\FSharp\Structural2\Structural2\SectionDatabase.fsx" 
open SectionDatabase.AISC


let mySection = SingleAngles.A36.``L2x2x1/4``


mySection.Description
mySection.HorizontalLeg.GetType()
mySection.Area
mySection.XBar

let allowableTension = SectionOps.allowableTension SingleAngles.A36.``L2x2x1/4``

let myNewSection = {mySection with Material = {mySection.Material with Fy = 36.0<ksi>; Fu = 50.0<ksi>}}

let newAllowableTension = SectionOps.allowableTension myNewSection

allowableTension/newAllowableTension = 50.0/36.0





