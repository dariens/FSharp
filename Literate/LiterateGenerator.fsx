#load @"packages/FSharp.Formatting/FSharp.Formatting.fsx"
open FSharp.Literate
open System.IO

let source = __SOURCE_DIRECTORY__
let template = Path.Combine(source, @"output\resources\template.html")

let fsi = FsiEvaluator()

let scripts = 
   [for file in System.IO.DirectoryInfo(source + "\\scripts").GetFiles() do
       let fileName = file.Name.Split('.').[0]
       yield fileName]
       

let generateHtml script =
    let fsxScript = Path.Combine(source, @"scripts\" + script + ".fsx")
    let output = Path.Combine(source, @"output\" + script + ".html")
    Literate.ProcessScriptFile(fsxScript,template, output , fsiEvaluator = fsi,lineNumbers = false)

let generateAllHtml (scripts: string list) =
    for script in scripts do
        generateHtml script

generateAllHtml scripts


