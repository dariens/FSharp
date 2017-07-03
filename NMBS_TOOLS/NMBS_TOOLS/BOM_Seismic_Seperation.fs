namespace NMBS_Tools.BOM_Seismic_Seperation

module Seperator =
    #if INTERACTIVE
    //#r "../packages/Deedle.1.2.5/lib/net40/Deedle.dll"
    #r "Microsoft.Office.Interop.Excel.dll"
    //System.Environment.CurrentDirectory <- @"C:\Users\darien.shannon\Documents\Code\F#\FSharp\NMBS_TOOLS\NMBS_TOOLS\bin\Debug"
    #endif

    open System
    open System.IO
    open Microsoft.Office.Interop.Excel
    open System.Runtime.InteropServices
    open NMBS_Tools.ArrayExtensions


    type Load =
        {
        Type : string;
        Category : string
        Position : string
        Load1Value : float
        Load1DistanceFt : float option
        Load1DistanceIn : float option
        Load2Value : float option
        Load2DistanceFt : float option
        Load2DistanceIn : float option
        Ref : string option
        LoadCase : string option
        }

    type LoadNote =
        {
        LoadNumber : string
        Load : Load
        }

    type Joist =
        {
        Mark : string
        JoistSize : string
        LoadNotes : string list option
        Loads : LoadNote list option
        }




    module CleanBomInfo =

        let nullableToOption<'T> value =
            match (box value) with
            | null  -> None
            | value when value = (box "") -> None
            | _ -> Some ((box value) :?> 'T)


        module CleanLoads =

            let getLoadFromArraySlice (a : obj []) =
                {
                Type = string a.[1]
                Category = string a.[2]
                Position = string a.[3]
                Load1Value =  (box a.[5]) :?> float
                Load1DistanceFt = nullableToOption<float> a.[6]
                Load1DistanceIn = nullableToOption<float> a.[7]
                Load2Value = nullableToOption<float> a.[8]
                Load2DistanceFt = nullableToOption<float> a.[9]
                Load2DistanceIn = nullableToOption<float> a.[10]
                Ref = nullableToOption<string> a.[11]
                LoadCase = nullableToOption<string> a.[12]
                }

            let getLoadNotesFromArray (a2D : obj[,]) =
                let mutable startIndex = Array2D.base1 a2D 
                let endIndex = (a2D |> Array2D.length1) - match startIndex with
                                                            | 0 -> 1
                                                            | _ -> 0
                let loadNotes = 
                    let mutable loadNumber = ""
                    [for currentIndex = startIndex to endIndex do
                        if a2D.[currentIndex, 1] <> null && a2D.[currentIndex,1] <> (box "") then
                            if a2D.[currentIndex, 0] <> null && a2D.[currentIndex, 0] <> (box "") then
                                loadNumber <- (string a2D.[currentIndex, 0]).Trim()
                            yield {LoadNumber = loadNumber; Load = getLoadFromArraySlice a2D.[currentIndex, *]}]
                loadNotes

        module CleanJoists =

            let getLoadNotes (note : string) =
                let loadNoteStart = note.IndexOf("(")
                let loadNotes = note.Substring(loadNoteStart)
                let loadNotes = loadNotes.Split([|"("; ","; ")"|], StringSplitOptions.RemoveEmptyEntries)
                let loadNotes = loadNotes |> List.ofArray
                loadNotes |> List.map (fun (s : string) -> s.Trim())

            let getJoistsFromArray (a2D : obj [,]) =
                let mutable startIndex = Array2D.base1 a2D 
                let endIndex = (a2D |> Array2D.length1) - match startIndex with
                                                            | 0 -> 1
                                                            | _ -> 0
                let joists =
                    [for currentIndex = startIndex to endIndex do
                        if a2D.[currentIndex, 0] <> null && a2D.[currentIndex, 0] <> (box "") then
                            let loadNotes =
                                let notes = nullableToOption<string> a2D.[currentIndex, 26]
                                match notes with
                                | Some notes -> Some (getLoadNotes notes)
                                | None -> None
                            yield
                                {
                                Mark = string a2D.[currentIndex, 0]
                                JoistSize = string a2D.[currentIndex, 2]
                                LoadNotes = loadNotes 
                                Loads = None
                                }]
                joists

    
    let getAllInfo (reportPath : string ) (getInfoFunctions : (Workbook -> 'TOutput) list) =
        let tempExcelApp = new Microsoft.Office.Interop.Excel.ApplicationClass(Visible = false)
        //let bom = tempExcelApp.Workbooks.Open(bomPath)
        try 
            tempExcelApp.DisplayAlerts <- false
            let tempReportPath = System.IO.Path.GetTempFileName()      
            File.Delete(tempReportPath)
            File.Copy(reportPath, tempReportPath)
            let workbook = tempExcelApp.Workbooks.Open(tempReportPath)
            let info =
                [for getInfoFunction in getInfoFunctions do
                        yield getInfoFunction workbook]
            workbook.Close(false)
            Marshal.ReleaseComObject(workbook) |> ignore
            System.GC.Collect() |> ignore
            printfn "Finished processing %s." reportPath 
            printfn "Finished processing all files."
            info
        finally
            tempExcelApp.Quit()
            Marshal.ReleaseComObject(tempExcelApp) |> ignore
            System.GC.Collect() |> ignore
            
    type BomReturnTypes =
            | Joists of Joist list option
            | Girders of (obj [,] * obj [,]) option
            | Loads of LoadNote list option

            member this.getJoists =
                match this with
                | Joists joists -> joists
                | _ -> failwith "This is not joists"
            member this.getGirders =
                match this with
                | Girders girders -> girders
                | _ -> failwith "This is not girders"
            member this.getLoads =
                match this with
                | Loads loads -> loads
                | _ -> failwith "This is not loads"



    let getLoads (bom: Workbook) =
        let workSheetNames = [for sheet in bom.Worksheets -> (sheet :?> Worksheet).Name]
        let filteredList = workSheetNames |> List.filter (fun name -> name.Contains("L ("))
        if (List.isEmpty filteredList) then
            Loads None
        else
            let arrayList =
                seq [for sheet in bom.Worksheets do
                        let sheet = (sheet :?> Worksheet)
                        if sheet.Name.Contains("L (") then
                            yield sheet.Range("A14","M55").Value2 :?> obj [,]]   
            let loadsAsArray = Array2D.joinMany (Array2D.joinByRows) arrayList
            let loads = CleanBomInfo.CleanLoads.getLoadNotesFromArray loadsAsArray
            Loads (Some loads)

    let getJoists (bom : Workbook) =
        let workSheetNames = [for sheet in bom.Worksheets -> (sheet :?> Worksheet).Name]
        let filteredList = workSheetNames |> List.filter (fun name -> name.Contains("J ("))
        if (List.isEmpty filteredList) then
            Joists None
        else
            let arrayList =
                seq [for sheet in bom.Worksheets do
                        let sheet = (sheet :?> Worksheet)
                        if sheet.Name.Contains("J (") then
                            if (sheet.Range("A21").Value2 :?> string) = "MARK" then
                                yield sheet.Range("A23","AA40").Value2 :?> obj [,]
                            else
                                yield sheet.Range("A16", "AA45").Value2 :?> obj [,]]

            let joistsAsArray = Array2D.joinMany (Array2D.joinByRows) arrayList
            let joists = CleanBomInfo.CleanJoists.getJoistsFromArray joistsAsArray
            Joists (Some joists)

    let getGirders (bom : Workbook) =
        let workSheetNames = [for sheet in bom.Worksheets -> (sheet :?> Worksheet).Name]
        let filteredList = workSheetNames |> List.filter (fun name -> name.Contains("G ("))
        if (List.isEmpty filteredList) then
            Girders None
        else
            let arrayList1 =
                seq [for sheet in bom.Worksheets do
                        let sheet = (sheet :?> Worksheet)
                        if sheet.Name.Contains("G (") then
                            if (sheet.Range("A26").Value2 :?> string) = "MARK" then
                                yield sheet.Range("A28","AA45").Value2 :?> obj [,]
                            else
                                yield sheet.Range("A14", "AA45").Value2 :?> obj [,]]
            let arrayList2 =
                seq [for sheet in bom.Worksheets do
                        let sheet = (sheet :?> Worksheet)
                        if sheet.Name.Contains("G (") then
                            yield sheet.Range("AB14","BG45").Value2 :?> obj [,]]
                
            Girders (Some ((Array2D.joinMany (Array2D.joinByRows) arrayList1,
                            Array2D.joinMany (Array2D.joinByRows) arrayList2)))

    let getInfoFunctions = [getJoists; getGirders; getLoads]

    type BomInfo = 
        {
        Joists : Joist list option
        Girders : (obj [,] * obj [,]) option
        Loads : LoadNote list option
        }

    let getAllBomInfo bomPath =
        let [joists; girders; loads] = getAllInfo bomPath getInfoFunctions /// warning is OK since this will always return a list of three itmes
        {
        Joists = joists.getJoists
        Girders = girders.getGirders
        Loads = loads.getLoads
        }

    let bomPath = @"C:\Users\darien.shannon\Desktop\4317-0092 Joist BOMs-For_Import_06-28-17.xlsx"

    let allBomInfo = getAllBomInfo bomPath

    let joists = allBomInfo.Joists
















        





                       



    



    

    


