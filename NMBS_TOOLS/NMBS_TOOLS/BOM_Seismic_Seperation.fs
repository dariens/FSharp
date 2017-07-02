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

    let private nullableToOption<'T> value =
            match (box value) with
            | null  -> None
            | value when value = (box "") -> None
            | _ -> Some ((box value) :?> 'T)
    
    let getAllLoadNotes () =
        let getAllInfo (reportPath : string ) (getInfoFunction : Workbook -> 'TOutput) =
            let tempExcelApp = new Microsoft.Office.Interop.Excel.ApplicationClass(Visible = false)
            try 
                tempExcelApp.DisplayAlerts <- false
                let tempReportPath = System.IO.Path.GetTempFileName()      
                File.Delete(tempReportPath)
                File.Copy(reportPath, tempReportPath)
                let workbook = tempExcelApp.Workbooks.Open(tempReportPath)
                let info = getInfoFunction workbook
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
            
        // 'getInfoFunction'
        let getAllLoadNotesAsArray (bom: Workbook) =
            let arrayList =
                seq [for sheet in bom.Worksheets do
                        let sheet = (sheet :?> Worksheet)
                        if sheet.Name.Contains("L (") then
                            yield sheet.Range("A14","M55").Value2 :?> obj [,]]    
            Array2D.joinMany (Array2D.joinByRows) arrayList

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
                            loadNumber <- string a2D.[currentIndex, 0]
                        yield {LoadNumber = loadNumber; Load = getLoadFromArraySlice a2D.[currentIndex, *]}]
            loadNotes

        let getAllLoadsFromBomAsArray bomPath = getAllInfo bomPath getAllLoadNotesAsArray

        let getAllLoadNotesFromBom bomPath = getLoadNotesFromArray (getAllLoadsFromBomAsArray bomPath)

        let bomPath = @"C:\Users\darien.shannon\Desktop\4317-0092 Joist BOMs-For_Import_06-28-17.xlsx"

        let getAllLoadNotes () =
            let timer = new System.Diagnostics.Stopwatch()
            timer.Start()
            let loads = getAllLoadNotesFromBom bomPath
            timer.Stop()
            printfn "Processed BOM in %f seconds" (float timer.ElapsedMilliseconds/1000.0)
            loads

        getAllLoadNotes()







        





                       



    



    

    


