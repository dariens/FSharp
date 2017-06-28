namespace NMBS_Tools.CustomerReports


module CustomerReports =
    open Deedle
    open System
    open NMBS_Tools.FrameExtensions
    open Microsoft.Office.Interop.Excel
    open System.Runtime.InteropServices

    let createDsmReport startDate endDate dsm =
        //let startDate = System.DateTime(2017,1,1)
        //let endDate = System.DateTime.Now
        //#r "../packages/Deedle.1.2.5/lib/net40/Deedle.dll"
        //Environment.CurrentDirectory <- @"U:\CODE\F#\Expert-FSharp-4.0\WeeklyWorkbookAnalysis\WeeklyWorkbookAnalysis\bin\Debug"

        
        let quotedVsSold = 
            match dsm with
            | "" -> Frame.ReadCsv(@"Data\Quoted Vs Sold.csv")
            | dsm -> Frame.ReadCsv(@"Data\Quoted Vs Sold.csv")
                     |> Frame.whereRowValuesInColEqual "Quote DSM" [dsm]

        let jobsSold = Frame.ReadCsv(@"Data\Jobs Sold.csv")
        let jobsQuoted = Frame.ReadCsv(@"Data\Jobs Quoted.csv")
    
        let filteredJobsQuoted = 
            jobsQuoted
            |> Frame.whereRowValuesInColAreInDateRange "Date Quoted" startDate endDate
            |> Frame.getSpecificColumns ["Job Number"; "Total Tons"]
            |> Frame.renameColumn "Total Tons" "Quoted Tons (From 'Jobs Quoted')"
             
        let mergedQuotedVsSoldWithJobsQuoted = 
            quotedVsSold
            |> Frame.merge_On filteredJobsQuoted "Job Number" 0.0
            |> Frame.getSpecificColumns ["Job Number"; "Customer"; "J.Tons(Base)";"Quoted Tons (From 'Jobs Quoted')"]
            |> Frame.renameColumn "J.Tons(Base)" "Quoted Tons (From 'Quoted Vs. Sold')"
            |> Frame.aggregateRowsBy 
                (seq ["Customer"])
                (seq ["Quoted Tons (From 'Jobs Quoted')";"Quoted Tons (From 'Quoted Vs. Sold')"])
                Stats.sum

        let soldByCustomer = 
            jobsSold
            |> Frame.whereRowValuesInColAreInDateRange "J. PO Date" startDate endDate
            |> Frame.aggregateRowsBy (seq ["Customer"]) (seq ["Total Tons"]) Stats.sum
            |> Frame.renameColumn "Total Tons" "Sold Tons"

     
        let soldJobNumbers = jobsSold.GetColumn<obj>("Job Number").ValuesAll
        
        let jobsQuotedAndSold = 
            quotedVsSold
            |> Frame.whereRowValuesInColDontEqual "Job Number" soldJobNumbers
            |> Frame.getSpecificColumns ["Customer"; "J.Tons(Base)"]
            |> Frame.renameColumn "J.Tons(Base)" "Quoted Tons That We Sold (From 'Quoted Vs. Sold')"
            |> Frame.aggregateRowsBy
                (seq ["Customer"])
                (quotedVsSold.Columns.Keys)
                Stats.sum

        let filteredJobsQuotedAndSold = 
             jobsQuoted
             |> Frame.whereRowValuesInColAreInDateRange "Date Quoted" startDate endDate
             |> Frame.whereRowValuesInColEqual "Job Number" soldJobNumbers
             |> Frame.renameColumn "Total Tons" "Quoted Tons That We Sold (From 'Jobs Quoted')"
             |> Frame.getSpecificColumns ["Job Number"; "Quoted Tons That We Sold (From 'Jobs Quoted')"]
             
         
        let mergedQuotedVsSoldWithJobsQuoted3 =
            quotedVsSold
            |> Frame.merge_On filteredJobsQuotedAndSold "Job Number" 0.0
            |> Frame.getSpecificColumns ["Job Number"; "Customer"; "J.Tons(Base)";"Quoted Tons That We Sold (From 'Jobs Quoted')"]
            |> Frame.aggregateRowsBy
                (seq ["Customer"])
                (seq ["Quoted Tons That We Sold (From 'Jobs Quoted')"])
                Stats.sum


        let customerAnalysis =
            let newFrame =
                mergedQuotedVsSoldWithJobsQuoted
                |> Frame.merge_On soldByCustomer "Customer" 0.0
                |> Frame.merge_On jobsQuotedAndSold "Customer" 0.0
                |> Frame.merge_On mergedQuotedVsSoldWithJobsQuoted3 "Customer" 0.0

            newFrame.AddColumn("Quoted Tons That We Sold To Others (From 'Jobs Quoted')",
                seq [ for v in newFrame.RowKeys do
                        let quoted = newFrame.Rows.[v].["Quoted Tons That We Sold (From 'Jobs Quoted')"]
                        let sold = newFrame.Rows.[v].["Sold Tons"]
                        yield (quoted :?> float) - (sold :?> float)])

            newFrame.AddColumn("Quoted Tons That We Sold To Others (From 'Quoted Vs. Sold')",
                seq [ for v in newFrame.RowKeys do
                        let quoted = newFrame.Rows.[v].["Quoted Tons That We Sold (From 'Quoted Vs. Sold')"]
                        let sold = newFrame.Rows.[v].["Sold Tons"]
                        yield (quoted :?> float) - (sold :?> float)])

            newFrame.AddColumn("Sold Percentage (From 'Jobs Quoted')",
                seq [ for v in newFrame.RowKeys do
                        let quoted = newFrame.Rows.[v].["Quoted Tons (From 'Jobs Quoted')"]
                        let sold = newFrame.Rows.[v].["Sold Tons"]
                        if (quoted :?> float) <> 0.0 then
                            yield (sold :?> float) / (quoted :?> float)
                        else
                            yield 0.0])

            newFrame.Columns.[["Customer";
                               "Sold Tons";
                               "Quoted Tons (From 'Jobs Quoted')";
                               //"Quoted Tons That We Sold To Others (From 'Jobs Quoted')";
                               //"Quoted Tons That We Sold (From 'Jobs Quoted')";
                               "Quoted Tons (From 'Quoted Vs. Sold')";
                               "Sold Percentage (From 'Jobs Quoted')"]]
                               //"Quoted Tons That We Sold To Others (From 'Quoted Vs. Sold')";
                               //"Quoted Tons That We Sold (From 'Quoted Vs. Sold')"]]
        System.IO.Directory.CreateDirectory(@"Output\Temp") |> ignore
        customerAnalysis.SaveCsv( @"Output\Temp\Customer Analysis_" + dsm + ".csv")

        printfn "%A" ("Finished Report For " + dsm)

    let createDsmReports startDate endDate dsms =
        for dsm in dsms do
            createDsmReport startDate endDate dsm
        printfn "%A" "All Finshed!"


    let createCustomerAnalysis () =
        //let startDate = System.DateTime(2017,1,1)
        //let endDate = System.DateTime.Now
        //#r "../packages/Deedle.1.2.5/lib/net40/Deedle.dll"
        //System.Environment.CurrentDirectory <- @"C:\Users\darien.shannon\Documents\Code\F#\Expert-FSharp-4.0\WeeklyWorkbookAnalysis\WeeklyWorkbookAnalysis\bin\Debug"
        //open Deedle
        //open System
        
        let quotedVsSold = Frame.ReadCsv(@"Data\Quoted Vs Sold.csv")
        let jobsSold = Frame.ReadCsv(@"Data\Jobs Sold.csv")
        let jobsQuoted = Frame.ReadCsv(@"Data\Jobs Quoted.csv")

        printfn "Step 1 of 6 Complete"

        let calander = new System.Globalization.GregorianCalendar()

        let jobsSoldInfo =
            jobsSold
            |> Frame.addCol "Sold Month"
                (jobsSold.GetColumn("J. PO Date")
                    |> Series.mapValues (fun value -> 
                                            System.Globalization.GregorianCalendar().GetMonth(
                                             System.DateTime.Parse(value))))
            |> Frame.addCol "Sold Year"
                (jobsSold.GetColumn("J. PO Date")
                |> Series.mapValues (fun value -> System.DateTime.Parse(value).Year))
            |> Frame.getSpecificColumns ["Job Number"; "Total Tons"; "Sold Month"; "Sold Year"]
            |> Frame.renameColumn "Total Tons" "Sold Tons"
            |> Frame.map (fun r c (v: string) -> v.Trim())

        printfn "Step 2 of 6 Complete"

        let jobsQuotedInfo =
            jobsQuoted
            |> Frame.addCol "Quoted Month"
                    (jobsQuoted.GetColumn("Date Quoted")
                     |> Series.mapValues (fun value -> 
                                             System.Globalization.GregorianCalendar().GetMonth(
                                              System.DateTime.Parse(value))))
            |> Frame.addCol "Quoted Year"
                    (jobsQuoted.GetColumn("Date Quoted")
                    |> Series.mapValues (fun value -> System.DateTime.Parse(value).Year))
            |> Frame.getSpecificColumns ["Job Number"; "Total Tons"]
            |> Frame.renameColumn "Total Tons" "Quoted Tons"
            |> Frame.map (fun r c (v: string) -> v.Trim())

        printfn "Step 3 of 6 Complete"  
           
        let quotedVsSoldInfo =
            quotedVsSold
            |> Frame.getSpecificColumns ["Job Number"; "Customer"; "Quote DSM"; "Sold"; "J.Tons Sold"; "Job Last Changed"]
            |> Frame.addCol "QvS Month"
                    (quotedVsSold.GetColumn("Job Last Changed")
                        |> Series.mapValues (fun value -> 
                                                System.Globalization.GregorianCalendar().GetMonth(
                                                  System.DateTime.Parse(value))))
            |> Frame.addCol "QvS Year"
                    (quotedVsSold.GetColumn("Job Last Changed")
                    |> Series.mapValues (fun value -> System.DateTime.Parse(value).Year))

            |> Frame.removeCols ["Job Last Changed"]
            |> Frame.map (fun r c (v: string) -> v.Trim())
        
        printfn "Step 4 of 6 Complete"
           
        let mergedFrame =
            let frame =
                quotedVsSoldInfo
                |> Frame.merge_On jobsSoldInfo "Job Number" "blank"     /// test here
                |> Frame.merge_On jobsQuotedInfo "Job Number" "blank"   /// test here

            let actualMonth =
                let qvsMonthCol = frame.GetColumn<obj>("QvS Month")
                let soldMonthCol = frame.GetColumn<obj>("Sold Month")
                [for key in qvsMonthCol.Keys do
                   if soldMonthCol.[key] = box "blank" then
                       yield qvsMonthCol.[key]
                   else
                       yield soldMonthCol.[key]]
            frame.AddColumn("Month", actualMonth)
            let actualYear =
                let qvsYearCol = frame.GetColumn<obj>("QvS Year")
                let soldYearCol = frame.GetColumn<obj>("Sold Year")
                [for key in qvsYearCol.Keys do
                   if soldYearCol.[key] = box "blank" then
                       yield qvsYearCol.[key]
                   else
                       yield soldYearCol.[key]]
            frame.AddColumn("Year", actualYear)
            let actualSoldTons =
                let jSoldColumn = frame.GetColumn<bool>("Sold")
                let soldTonsColumn = frame.GetColumn<obj>("Sold Tons") |> Series.fillMissingWith 0.0
                [ for key in jSoldColumn.Keys do
                    if jSoldColumn.[key] = false || soldTonsColumn.[key] = (box "blank") then
                        yield (box null)
                    else
                        yield soldTonsColumn.[key]]
            frame.ReplaceColumn("Sold Tons", actualSoldTons)
            let actualQuotedTons =
                let quotedTonsColumn = frame.GetColumn<obj>("Quoted Tons")
                [ for key in quotedTonsColumn.Keys do
                    let v = quotedTonsColumn.[key]
                    if v = (box 0) || v = (box "blank") || v = (box 0M) then
                        yield (box null)
                    else
                        yield quotedTonsColumn.[key]]
            frame.ReplaceColumn("Quoted Tons", actualQuotedTons)
            frame.Columns.[["Job Number"; "Customer"; "Quote DSM"; "Sold Tons"; "Quoted Tons"; "Month"; "Year"; "Q. Tons"]]
        
            
        printfn "Step 5 of 6 Complete"
        
        mergedFrame.SaveCsv(@"Output\Customer Analysis.csv")

        let outputPath = System.IO.Path.GetFullPath(@"Output\")
        let resourcePath = System.IO.Path.GetFullPath(@"Resources\")

        let app = new ApplicationClass(Visible = false)
        let csv = app.Workbooks.Open(outputPath + "Customer Analysis.csv")
        let csvSheet = (csv.Worksheets.[1] :?> Worksheet)
        let custAnaly = app.Workbooks.Open(resourcePath+ "Customer Analysis - Blank.xlsx")
        let custAnalySheet = (custAnaly.Worksheets.[1] :?> Worksheet)
        let csvRange = csvSheet.UsedRange
        custAnalySheet.Range("A2", "G" + string (csvRange.Rows.Count + 1)).Value2 <- csvSheet.UsedRange.Value2
        custAnalySheet.Range("A2:G2").Delete() |> ignore
        let tableSheet = (custAnaly.Worksheets.[2] :?> Worksheet)
        let table = (tableSheet.PivotTables("pivTableCustomerAnalysis") :?> PivotTable)
        table.RefreshTable() |> ignore

        app.DisplayAlerts <- false
        custAnaly.SaveAs(outputPath + "Customer Analysis.xlsx")
        
        printfn "Step 6 of 6 Complete" 
                    
        Marshal.ReleaseComObject(csvSheet) |> ignore
        Marshal.ReleaseComObject(custAnalySheet) |> ignore
        Marshal.ReleaseComObject(tableSheet)  |> ignore
        csv.Close()
        Marshal.ReleaseComObject(csv)  |> ignore
        custAnaly.Close()
        Marshal.ReleaseComObject(custAnaly) |> ignore          
        app.Quit()
        Marshal.ReleaseComObject(app) |> ignore
        System.GC.Collect()
        System.IO.File.Delete(outputPath + "Customer Analysis.csv")
        printfn "%s" "All Finished!"