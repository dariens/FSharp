namespace NMBS_Tools.TCWidths

module CreateReport =
    #if INTERACTIVE
    #r "../packages/Deedle.1.2.5/lib/net40/Deedle.dll"
    #r "Microsoft.Office.Interop.Excel.dll"
    System.Environment.CurrentDirectory <- @"C:\Users\darien.shannon\Documents\Code\F#\FSharp\NMBS_TOOLS\NMBS_TOOLS\bin\Debug"
    #endif

    open Deedle
    open System
    open NMBS_Tools.FrameExtensions
    open Microsoft.Office.Interop.Excel
    open System.Runtime.InteropServices
    open System.IO
    

    let xlsToCsv (files: string list) =
        let app = new ApplicationClass(Visible = false)
        app.DisplayAlerts = false |> ignore
        try
            for file in files do
                let fileName = file.Substring(0, file.LastIndexOf("."))
                if System.IO.File.Exists(file) then               
                    let xls = app.Workbooks.Open(fileName)
                    xls.SaveAs(fileName + ".csv",Microsoft.Office.Interop.Excel.XlFileFormat.xlCSV)
                    xls.Close(false)
                    Marshal.ReleaseComObject(xls) |> ignore
                    printfn "Converted %s to csv" (System.IO.Path.GetFileName file)
        finally
            app.Quit()
            Marshal.ReleaseComObject(app) |> ignore

    type TCAnalysis =
        {
        JobNumber : string
        A42A28_Percent : float
        A44A_Percent : float
        A46A28_Percent : float
        A48A29_Percent : float
        A50A28_Percent : float
        ``3028_Percent`` : float
        ``3031_Percent`` : float
        A42A28_WoodLength : float
        A44A_WoodLength : float
        A46A28_WoodLength : float
        A48A29_WoodLength : float
        A50A28_WoodLength : float
        ``3028_WoodLength`` : float
        ``3031_WoodLength`` : float
        }

    let getTC (chords : string) =
           chords.Split([|"/"|], StringSplitOptions.RemoveEmptyEntries).[0]
    
    let getValues tc (df : Frame<_,_>) =
        let rowOption = OptionalValue.asOption (df.TryGetRow(tc)) : Series<string, obj> option
        match rowOption with
        | Some row -> Convert.ToDouble(row.["Wood Length"]), Convert.ToDouble(row.["% TC"])
        | None -> 0.0, 0.0

    //let df = Frame.ReadCsv(@"C:\Users\darien.shannon\Documents\Code\F#\FSharp\NMBS_TOOLS\NMBS_TOOLS\bin\Debug\Data\Joist Details\Joist Details - 4317-0057.csv")

    let tcAnalysis jobNumber df =
        let df =
            let joistsOnly =
                df |> Frame.whereRowValuesInColMeetReq "Description" 
                        (fun value -> value.ToString().Contains("G") = false)
                   |> Frame.indexRowsOrdinally
            let joistCount =
                    (joistsOnly |> Stats.sum).Get "Quantity"

            let woodLength =
                let quantity = (joistsOnly.GetColumn<double> "Quantity").Values |> List.ofSeq
                let length = (joistsOnly.GetColumn<double> "Base Length").Values |> List.ofSeq
                let tcxl = (joistsOnly.GetColumn<double> "TCXL").Values |> List.ofSeq
                let tcxr = (joistsOnly.GetColumn<double> "TCXR").Values |> List.ofSeq
                let woodLengths =
                    seq [ for i = 0 to quantity.Length - 1 do
                            yield quantity.[i] * (length.[i] + tcxl.[i] + tcxr.[i])]
                woodLengths |> Series.ofValues

            let summedByTC =
                joistsOnly
                |> Frame.addCol "Wood Length" woodLength
                |> Frame.colCalcedFromCol "Chords" (fun chords -> getTC chords) "TC"
                |> Frame.aggregateRowsBy (seq ["TC"]) (seq ["Quantity"; "Wood Length"]) Stats.sum

            summedByTC
            |> Frame.colCalcedFromCol "Quantity" (fun quantity -> quantity / joistCount) "% TC"
            |> Frame.whereRowValuesInColEqual "TC" ["A42A28"; "A44A"; "A46A28"; "A48A29"; "A48B28"; "3028"; "3031"]
            |> Frame.indexRows "TC"  

        {
        JobNumber = jobNumber        
        A42A28_Percent = snd (getValues "A42A28" df) + snd (getValues "A42A29" df)
        A44A_Percent = snd (getValues "A44A" df) + snd (getValues "A44A29" df)
        A46A28_Percent = snd (getValues "A46A28" df) + snd (getValues "A46A29" df)
        A48A29_Percent = (snd (getValues "A48A29" df)) + (snd (getValues "A48B28" df))
        A50A28_Percent = (snd (getValues "A50A29" df)) + (snd (getValues "A50B28" df))
        ``3028_Percent`` = snd (getValues "3028" df)
        ``3031_Percent`` = snd (getValues "3031" df)
        A42A28_WoodLength = fst (getValues "A42A28" df) + fst (getValues "A42A29" df)
        A44A_WoodLength = fst (getValues "A44A" df) + fst (getValues "A44A29" df)
        A46A28_WoodLength = fst (getValues "A46A28" df) + fst (getValues "A46A29" df)
        A48A29_WoodLength = (fst (getValues "A48A29" df)) + (fst (getValues "A48B28" df))
        A50A28_WoodLength = (fst (getValues "A50A29" df)) + (fst (getValues "A50B28" df))
        ``3028_WoodLength`` = fst (getValues "3028" df) 
        ``3031_WoodLength`` = fst (getValues "3031" df)
        }

    let getTCAnalysis (files : string list) =
        xlsToCsv files
        [for file in files do
            let jobNumber = file.Split([|"Joist Details - "; ".xls"|], StringSplitOptions.RemoveEmptyEntries).[1]
            let newFileName = file.Replace(".xls", ".csv")
            let df = Frame.ReadCsv(newFileName)
            File.Delete(newFileName)
            yield (df |> tcAnalysis jobNumber)
            printfn "Proceessed %s" (System.IO.Path.GetFileName file)]

    let dataPath = System.IO.Path.GetFullPath(@"Data\")

    let joistDetailPaths =
        let joistDetailsPath = dataPath + "Joist Details"
        let joistDetailsDirectory = new DirectoryInfo(joistDetailsPath)
        let files = joistDetailsDirectory.GetFiles();
        [for file in files do yield file.FullName]
        |> List.filter (fun s -> s.Contains(".xls"))
        

    let TCAnalysis() = 
        let frame = (getTCAnalysis joistDetailPaths) |> Frame.ofRecords
        let outputPath = System.IO.Path.GetFullPath(@"Output\")
        frame.SaveCsv(outputPath + "TCAnalysis.csv")




    





    

        
        



