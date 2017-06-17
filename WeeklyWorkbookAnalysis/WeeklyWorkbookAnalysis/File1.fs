namespace ReportAnalysis


module DsmReportAnalysis =
    //#r "U:/CODE/F#/WeeklyWorkbookAnalysis/packages/XPlot.Plotly.1.4.2/lib/net45/Xplot.Plotly.dll"
    //#r "Microsoft.Office.Interop.Excel.dll"
    open Microsoft.Office.Interop.Excel
    open System.IO
    open System.Runtime.InteropServices
    


    /// Get a FileInfo [] containing the reports in the 'Weekly Sales Report' folder
    
    type FeedbackLine = 
        {Tons : obj
         SellingPrice : obj
         FreightPrice : obj
         Adjustment: obj
         PricePerTonPlant : obj
         PricePerTonDel : obj
         Company: obj
         Miles: obj
         Customer: obj
         Comments: obj}

        static member Null =
            {Tons = null
             SellingPrice = null
             FreightPrice = null
             Adjustment = null
             PricePerTonPlant = null
             PricePerTonDel = null
             Company = null
             Miles = null
             Customer = null
             Comments = null}

        member x.IsNull =
            (x.Company = null &&
             x.Tons = null &&
             x.SellingPrice = null &&
             x.FreightPrice = null &&
             x.Adjustment = null &&
             x.Company = null &&
             x.Miles = null &&
             x.Customer = null &&
             x.Comments = null)
                        

    type JFeedback = 
        {Dsm : string
         WeekStart : System.DateTime
         JobNumber : string
         JobName : string
         FeedbackLines : FeedbackLine list}

    type InfoReturnTypes =
        | JoistFeedback of JFeedback list
    
    let feedbackToExcel (feedbackList : JFeedback list) =
        let app = new ApplicationClass(Visible = false)
        let workbook = app.Workbooks.Add()
        let worksheet = (workbook.Worksheets.[1] :?> Worksheet)
        let mutable row = 2
        for f in feedbackList do
            for line in f.FeedbackLines do
                let r = string row
                worksheet.Range("A" + r).Value2 <- f.JobName
                worksheet.Range("B" + r).Value2 <- f.JobNumber
                worksheet.Range("C" + r).Value2 <- (string line.Company).Trim()
                worksheet.Range("D" + r).Value2 <- line.Tons
                worksheet.Range("E" + r).Value2 <- line.SellingPrice
                worksheet.Range("F" + r).Value2 <- line.PricePerTonPlant
                worksheet.Range("G" + r).Value2 <- line.PricePerTonDel
                worksheet.Range("H" + r).Value2 <- f.Dsm
                worksheet.Range("I" + r).Value2 <- f.WeekStart.ToString("MM/dd/yyyy")
                row <- row + 1

        worksheet.Range("A1").Value2 <- "Job Name"
        worksheet.Range("B1").Value2 <- "Job Number"
        worksheet.Range("C1").Value2 <- "Company"
        worksheet.Range("D1").Value2 <- "Tons"
        worksheet.Range("E1").Value2 <- "Selling Price"
        worksheet.Range("F1").Value2 <- "$/Ton (Plant)"
        worksheet.Range("G1").Value2 <- "$/Ton (Del.)"
        worksheet.Range("H1").Value2 <- "DSM"
        worksheet.Range("I1").Value2 <- "Week Start"
        
        workbook.SaveAs(@"\\nmbsfaln-fs\sales\TOOLS\WEEKLY SALES REPORT\Weekly Workbook Analysis\Report_Temp.xlsx")
        Marshal.ReleaseComObject(worksheet) |> ignore
        workbook.Close()
        Marshal.ReleaseComObject(workbook) |> ignore
        app.Quit()
        Marshal.ReleaseComObject(app) |> ignore
        System.GC.Collect()

    let mutable (allFeedback : JFeedback list) = []
    let combinedFeedback (feedbackList : JFeedback list) =
        for f in feedbackList do
            allFeedback <- f :: allFeedback

    let handleInfoReturnTypes (info : InfoReturnTypes list) =
        let mutable result = []
        match info with
            | [] -> ()
            | JoistFeedback(fbList) :: tail -> combinedFeedback fbList

    


    let getInfoFromAllReports (reports : FileInfo []) (getInfo : (ApplicationClass -> string -> InfoReturnTypes list)) =
        let tempExcelApp = new Microsoft.Office.Interop.Excel.ApplicationClass(Visible = false)
        try
            for f in reports do
                if f.Name.Contains("~$") = false then
                    let reportInfo = getInfo tempExcelApp f.FullName
                    handleInfoReturnTypes reportInfo
        finally
            tempExcelApp.Quit()
            Marshal.ReleaseComObject(tempExcelApp) |> ignore
            System.GC.Collect()



    

        /// Function that pulls info from one of the reports. The 'pullInfoFunction' will determine what info is taken from the report 
    let pullInfoFromReport (pullInfoFunctions : (Workbook -> InfoReturnTypes) list) (tempExcelApp: Microsoft.Office.Interop.Excel.Application) (reportPath : string)  =
        let tempReportPath = System.IO.Path.GetTempFileName()
        File.Delete(tempReportPath)
        File.Copy(reportPath, tempReportPath)
        let workbook = tempExcelApp.Workbooks.Open(tempReportPath)
        let mutable allInfo = []
        for infoFunction in pullInfoFunctions do
            let infoPulledFromWorkbook = infoFunction(workbook)
            allInfo <- infoPulledFromWorkbook :: allInfo
        workbook.Close()
        Marshal.ReleaseComObject(workbook) |> ignore
        System.GC.Collect() |> ignore
        allInfo


    /// Get Joist FB Info
    let getJoistFeedback (worksheet : Worksheet ) =
        let dsm = string (worksheet.Range("G2").Value2)
        let weekStart = string (worksheet.Range("L2").Text)
        let fbRowStart = System.Convert.ToInt32(worksheet.Range("P4").Value2)
        let fbRowEnd = System.Convert.ToInt32(worksheet.Range("Q4").Value2)
        let feedback =
          let mutable row = fbRowStart
          [while row <= fbRowEnd do
              if worksheet.Range("A" + string row).Value2 = null then
                  row <- row + 1
              else
                let jobNumber = string (worksheet.Range("A" + string row).Value2)
                let jobName = string (worksheet.Range("B" + string row).Value2)
                let feedbackList = 
                    let mutable mutableFeedbackList = [] : FeedbackLine list
                    let mutable atNextJob = false
                    [while atNextJob = false && row <= fbRowEnd do
                        let feedbackLine = {Tons = worksheet.Range("C" + string row).Value2
                                            SellingPrice = worksheet.Range("D" + string row).Value2
                                            FreightPrice = worksheet.Range("E" + string row).Value2
                                            Adjustment = worksheet.Range("F" + string row).Value2
                                            PricePerTonPlant = worksheet.Range("G" + string row).Value2
                                            PricePerTonDel = worksheet.Range("H" + string row).Value2
                                            Company = worksheet.Range("I" + string row).Value2
                                            Miles = worksheet.Range("J" + string row).Value2
                                            Customer = worksheet.Range("K" + string row).Value2
                                            Comments = worksheet.Range("L" + string row).Value2}
                        row <- row + 1
                        if worksheet.Range("A" + string row).Value2 <> null then
                            atNextJob <- true 
                        else
                            if feedbackLine.IsNull = false then
                                yield feedbackLine ]

                let joistFeedback = {Dsm = dsm;
                                     JobNumber = jobNumber;
                                     JobName = jobName;
                                     WeekStart = System.DateTime.Parse weekStart;
                                     FeedbackLines = feedbackList}
                yield joistFeedback]
                
        feedback

    let getAllJoistFeedback (workbook : Workbook) =
        let returns =
            [ for s in workbook.Worksheets do
                let s = s :?> Worksheet
                let invalidSheets = ["Current"; "JOIST FEEDBACK"; "DECK FEEDBACK"; "Tables"]
                if (List.contains s.Name invalidSheets) = false then
                    yield getJoistFeedback s ]
        let jfeedbackList = List.concat returns
        JoistFeedback(jfeedbackList)   

    let pullInfoFunctions = [getAllJoistFeedback]

    let getAllInfo = pullInfoFromReport pullInfoFunctions : (ApplicationClass -> string -> InfoReturnTypes list )

    let dsmReports =
        let reportPath = @"\\nmbsfaln-fs\sales\tools\weekly sales report\"
        let reportDirectory = new DirectoryInfo(reportPath)
        let files = reportDirectory.GetFiles();
        files

    let singleDsmReport dsm =
        [| for s in dsmReports do
               if s.Name.Contains(dsm) then
                   yield s|]

    let getInfoFromBobsReport = getInfoFromAllReports (singleDsmReport "Stearns")
    let GetBobsInfo () = getInfoFromBobsReport getAllInfo
    //GetBobsInfo ()

    let getInfoFromAllDsmReports = getInfoFromAllReports dsmReports
    let GetAllInfo () = getInfoFromAllDsmReports getAllInfo
    //GetAllInfo ()

    let SendFeedbackToExcel () = feedbackToExcel allFeedback

module XPlotting =
    open XPlot.Plotly

    let MyPlot () =
        let trace1 =
            Scatter(
                x = [1; 2; 3; 4],
                y = [10; 15; 13; 17])

        let trace2 =
            Scatter(
                x = [2; 3; 4; 5],
                y = [16; 5; 11; 9])

        let plot =
            [trace1; trace2]
            |> Chart.Plot
            |> Chart.WithWidth 700
            |> Chart.WithHeight 500

        plot.Show()


module Deedleing =
    //#r "../packages/Deedle.1.2.5/lib/net40/Deedle.dll"

    open Deedle
    open System
    open System.IO
    open Microsoft.Office.Interop.Excel
    open System.Runtime.InteropServices

    module Frame =
        let whereRowValuesInColMeetReq colIndex req frame =
            Frame.filterRowValues (fun (objSeries : ObjectSeries<'a>) ->
                    (req (objSeries.Get(colIndex)))) frame

        let whereRowValuesInColEqual colIndex acceptableValues frame =
            frame
            |> whereRowValuesInColMeetReq colIndex (fun obj -> Seq.contains obj acceptableValues)
            //Frame.filterRowValues (fun (objSeries : ObjectSeries<'a>) -> 
            //        (Seq.contains (objSeries.Get(colIndex)) acceptableValues)) frame

        let whereRowValuesInColDontEqual colIndex unacceptableValues frame =
            frame
            |> whereRowValuesInColMeetReq colIndex (fun obj -> (Seq.contains obj unacceptableValues) = false)

        let whereRowValuesInColAreInDateRange colIndex startDate endDate frame =
            frame
            |> whereRowValuesInColMeetReq colIndex (fun date ->
                                                        let date = System.DateTime.Parse(string date)
                                                        date >= startDate && date <= endDate)

        let removeDuplicateRows index (frame : Frame<'a, 'b>) =
          let nonDupKeys = frame.GroupRowsBy(index).RowKeys
                           |> Seq.distinctBy (fun (a, b) -> a)
                           |> Seq.map (fun (a, b) -> b)
          frame.Rows.[nonDupKeys]
        
        
        let merge_On (infoFrame : Frame<'c, 'b>) column missingReplacement (initialFrame : Frame<'a,'b>) =
              let frame = initialFrame.Clone()
              let infoFrame = infoFrame.Clone()
                               |> removeDuplicateRows column 
                               |> Frame.indexRowsString column
              let initialSeries = frame.GetColumn(column)
              let infoFrameRows = infoFrame.RowKeys
              for colKey in infoFrame.ColumnKeys do
                  let newSeries =
                      [for v in initialSeries.ValuesAll do
                            if Seq.contains v infoFrameRows then  
                                let key = infoFrame.GetRow(v)
                                yield key.[colKey]
                            else
                                yield box missingReplacement ]
                  frame.AddColumn(colKey, newSeries)
              frame

        let merge_On2 (infoFrame : Frame<'c, 'b>) column missingReplacement (initialFrame : Frame<'a,'b>) =
              let infoFrame = infoFrame
                              |> removeDuplicateRows column
                              |> Frame.indexRows column

              let infoMatched =
                  initialFrame.Rows
                  |> Series.map (fun k row ->
                      infoFrame.Rows.TryGet(row.GetAs(column)).ValueOrDefault)
                  |> Series.fillMissingWith missingReplacement
                  |> Frame.ofRows
              
              initialFrame.Join(infoMatched) |> ignore
              initialFrame

        

        let sumColumnsBy colIndex (frame: Frame<'a, 'b>) =
                let distinctValues = frame.GetColumn(colIndex).Values |> Seq.distinct
                let mutable row = -1
                let seriesList =
                  [for distinctValue in distinctValues do
                    row <- row + 1
                    let sumOfDistinctValue = frame
                                             |> whereRowValuesInColEqual colIndex [distinctValue]
                                             |> Frame.dropCol colIndex
                                             |> Stats.sum
                                             
                    for key in sumOfDistinctValue.Keys do                   
                         yield (box row, key, sumOfDistinctValue.[key])]
                let newFrame = seriesList |> Frame.ofValues
                newFrame.AddColumn(colIndex, distinctValues)
                newFrame
                


        let sumSpecificColumnsBy byColumn (wantedColumns) (frame: Frame<'a, 'b>) =
            let allColumns = byColumn :: wantedColumns
            let newFrame = frame.Columns.[allColumns]
            let summedFrame = newFrame |> sumColumnsBy byColumn
            summedFrame

        let getSpecificColumns (columns: 'b list) (frame: Frame<'a, 'b>) =
            frame.Columns.[columns]

        let renameColumn initialName finalName (frame: Frame<'a, 'b>) =
            frame.RenameColumn(initialName, finalName)
            frame

        let stack (stackFrame: Frame<'a, 'b>) (initialFrame: Frame<'a, 'b>) =
            let initialFrame = initialFrame |> Frame.dropSparseRows
            let stackFrame = stackFrame |> Frame.dropSparseRows
            let cols = initialFrame.Columns.Keys
            let allColumns =
                [for col in cols do
                    let initialValues = initialFrame.GetColumn(col).ValuesAll             
                    let newValues = stackFrame.GetColumn(col).ValuesAll      
                    let allValues = Seq.append initialValues newValues
                    yield (col, Series.ofValues allValues)]
            Frame.ofColumns allColumns

        let replaceVaueInColumnWith column oldValue newValue (frame : Frame<_,_>) =
            let newColumn = frame.GetColumn<'b>(column)
                            |> Series.mapValues (fun v -> match v with
                                                          | oldValue -> box newValue
                                                          | _ -> box v )
            frame.ReplaceColumn(column, newColumn)
            frame

        let removeCols columns (frame : Frame<_,_>) =
            let allColumns = frame.Columns.Keys
            let wantedColumns =
                [for col in allColumns do
                     if not (Seq.contains col columns) then
                         yield col]
            frame.Columns.[wantedColumns]

            






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


    let createEmployeeReport () =
        //#r "../packages/Deedle.1.2.5/lib/net40/Deedle.dll"
        //System.Environment.CurrentDirectory <- @"C:\Users\darien.shannon\Documents\Code\F#\Expert-FSharp-4.0\WeeklyWorkbookAnalysis\WeeklyWorkbookAnalysis\bin\Debug"
        //open Deedle
        //open System


        let jobsSold = Frame.ReadCsv(@"Data\Jobs Sold.csv")
        let jobsQuoted = Frame.ReadCsv(@"Data\Jobs Quoted.csv")
        let calander = new System.Globalization.GregorianCalendar()
        let estimatorAnalysis () =
            let estimatorQuotedJobsCount =
                jobsQuoted
                |> Frame.addCol "Week"
                    (jobsQuoted.GetColumn("Date Quoted")
                     |> Series.mapValues (fun value -> 
                                             System.Globalization.GregorianCalendar().GetWeekOfYear(
                                              System.DateTime.Parse(value),
                                              System.Globalization.CalendarWeekRule.FirstDay,
                                              System.DayOfWeek.Sunday)))
                |> Frame.addCol "Year"
                    (jobsQuoted.GetColumn("Date Quoted")
                    |> Series.mapValues (fun value -> System.DateTime.Parse(value).Year))
                |> Frame.aggregateRowsBy (seq ["TakeoffPerson"; "Year"; "Week"]) (seq ["Job Number"]) Stats.count
                |> Frame.sortRows "TakeoffPerson"
                |> Frame.renameColumn "Job Number" "Values"
                |> Frame.addCol "Report Type"
                    ((seq [for row in [0..jobsQuoted.Rows.KeyCount-1] do yield "Jobs Quoted Count"])
                     |> Series.ofValues)
                |> Frame.addCol "Employee Type"
                    ((seq [for row in [0..jobsSold.Rows.KeyCount-1] do yield "Estimator"])
                     |> Series.ofValues)
                |> Frame.renameColumn "TakeoffPerson" "Employee"
           
            let estimatorQuotedTons =
                jobsQuoted
                |> Frame.addCol "Week"
                    (jobsQuoted.GetColumn("Date Quoted")
                     |> Series.mapValues (fun value -> 
                                             System.Globalization.GregorianCalendar().GetWeekOfYear(
                                              System.DateTime.Parse(value),
                                              System.Globalization.CalendarWeekRule.FirstDay,
                                              System.DayOfWeek.Sunday)))
                |> Frame.addCol "Year"
                    (jobsQuoted.GetColumn("Date Quoted")
                    |> Series.mapValues (fun value -> System.DateTime.Parse(value).Year))
                |> Frame.aggregateRowsBy (seq ["TakeoffPerson"; "Year"; "Week"]) (seq ["Total Tons"]) Stats.sum
                |> Frame.sortRows "TakeoffPerson"
                |> Frame.renameColumn "Total Tons" "Values"
                |> Frame.addCol "Report Type"
                    ((seq [for row in [0..jobsQuoted.Rows.KeyCount-1] do yield "Tons Quoted"])
                     |> Series.ofValues)
                |> Frame.addCol "Employee Type"
                    ((seq [for row in [0..jobsSold.Rows.KeyCount-1] do yield "Estimator"])
                     |> Series.ofValues)
                |> Frame.renameColumn "TakeoffPerson" "Employee"

            let estimatorSoldJobCount =
                jobsSold
                |> Frame.addCol "Week"
                    (jobsSold.GetColumn("J. PO Date")
                     |> Series.mapValues (fun value -> 
                                             System.Globalization.GregorianCalendar().GetWeekOfYear(
                                              System.DateTime.Parse(value),
                                              System.Globalization.CalendarWeekRule.FirstDay,
                                              System.DayOfWeek.Sunday)))
                |> Frame.addCol "Year"
                    (jobsSold.GetColumn("J. PO Date")
                    |> Series.mapValues (fun value -> System.DateTime.Parse(value).Year))
                |> Frame.aggregateRowsBy (seq ["takeoffperson"; "Year"; "Week"]) (seq ["Job Number"]) Stats.count
                |> Frame.sortRows "takeoffperson"
                |> Frame.renameColumn "Job Number" "Values"
                |> Frame.renameColumn "takeoffperson" "Employee"
                |> Frame.addCol "Report Type"
                    ((seq [for row in [0..jobsSold.Rows.KeyCount-1] do yield "Jobs Sold Count"])
                     |> Series.ofValues)
                |> Frame.addCol "Employee Type"
                    ((seq [for row in [0..jobsSold.Rows.KeyCount-1] do yield "Estimator"])
                     |> Series.ofValues)
           
            let estimatorSoldTons =
                jobsSold
                |> Frame.addCol "Week"
                    (jobsSold.GetColumn("J. PO Date")
                     |> Series.mapValues (fun value -> 
                                             System.Globalization.GregorianCalendar().GetWeekOfYear(
                                              System.DateTime.Parse(value),
                                              System.Globalization.CalendarWeekRule.FirstDay,
                                              System.DayOfWeek.Sunday)))
                |> Frame.addCol "Year"
                    (jobsSold.GetColumn("J. PO Date")
                    |> Series.mapValues (fun value -> System.DateTime.Parse(value).Year))
                |> Frame.aggregateRowsBy (seq ["takeoffperson"; "Year"; "Week"]) (seq ["Total Tons"]) Stats.sum
                |> Frame.sortRows "takeoffperson"
                |> Frame.renameColumn "Total Tons" "Values"
                |> Frame.renameColumn "takeoffperson" "Employee"
                |> Frame.addCol "Report Type"
                    ((seq [for row in [0..jobsSold.Rows.KeyCount-1] do yield "Sold Tons"])
                     |> Series.ofValues)
                |> Frame.addCol "Employee Type"
                    ((seq [for row in [0..jobsSold.Rows.KeyCount-1] do yield "Estimator"])
                     |> Series.ofValues)


            let estimatorData =
                estimatorQuotedJobsCount
                |> Frame.stack estimatorQuotedTons
                |> Frame.stack estimatorSoldJobCount
                |> Frame.stack estimatorSoldTons


            let coordinatorQuotedJobsCount =
                jobsQuoted
                |> Frame.addCol "Week"
                    (jobsQuoted.GetColumn("Date Quoted")
                     |> Series.mapValues (fun value -> 
                                             System.Globalization.GregorianCalendar().GetWeekOfYear(
                                              System.DateTime.Parse(value),
                                              System.Globalization.CalendarWeekRule.FirstDay,
                                              System.DayOfWeek.Sunday)))
                |> Frame.addCol "Year"
                    (jobsQuoted.GetColumn("Date Quoted")
                    |> Series.mapValues (fun value -> System.DateTime.Parse(value).Year))
                |> Frame.aggregateRowsBy (seq ["Coordinator"; "Year"; "Week"]) (seq ["Job Number"]) Stats.count
                |> Frame.sortRows "Coordinator"
                |> Frame.renameColumn "Job Number" "Values"
                |> Frame.addCol "Report Type"
                    ((seq [for row in [0..jobsQuoted.Rows.KeyCount-1] do yield "Jobs Quoted Count"])
                     |> Series.ofValues)
                |> Frame.addCol "Employee Type"
                    ((seq [for row in [0..jobsSold.Rows.KeyCount-1] do yield "Coordinator"])
                     |> Series.ofValues)
                |> Frame.renameColumn "Coordinator" "Employee"
           
            let coordinatorQuotedTons =
                jobsQuoted
                |> Frame.addCol "Week"
                    (jobsQuoted.GetColumn("Date Quoted")
                     |> Series.mapValues (fun value -> 
                                             System.Globalization.GregorianCalendar().GetWeekOfYear(
                                              System.DateTime.Parse(value),
                                              System.Globalization.CalendarWeekRule.FirstDay,
                                              System.DayOfWeek.Sunday)))
                |> Frame.addCol "Year"
                    (jobsQuoted.GetColumn("Date Quoted")
                    |> Series.mapValues (fun value -> System.DateTime.Parse(value).Year))
                |> Frame.aggregateRowsBy (seq ["Coordinator"; "Year"; "Week"]) (seq ["Total Tons"]) Stats.sum
                |> Frame.sortRows "Coordinator"
                |> Frame.renameColumn "Total Tons" "Values"
                |> Frame.addCol "Report Type"
                    ((seq [for row in [0..jobsQuoted.Rows.KeyCount-1] do yield "Tons Quoted"])
                     |> Series.ofValues)
                |> Frame.addCol "Employee Type"
                    ((seq [for row in [0..jobsSold.Rows.KeyCount-1] do yield "Coordinator"])
                     |> Series.ofValues)
                |> Frame.renameColumn "Coordinator" "Employee"

            let jobInfoByJobNumber =
               jobsQuoted
               |> Frame.getSpecificColumns ["Job Number"; "Coordinator"]

           
            let coordinatorSoldJobCount =
                jobsSold
                |> Frame.merge_On jobInfoByJobNumber "Job Number" "" 
                |> Frame.addCol "Week"
                    (jobsSold.GetColumn("J. PO Date")
                     |> Series.mapValues (fun value -> 
                                             System.Globalization.GregorianCalendar().GetWeekOfYear(
                                              System.DateTime.Parse(value),
                                              System.Globalization.CalendarWeekRule.FirstDay,
                                              System.DayOfWeek.Sunday)))
                |> Frame.addCol "Year"
                    (jobsSold.GetColumn("J. PO Date")
                    |> Series.mapValues (fun value -> System.DateTime.Parse(value).Year))
                |> Frame.aggregateRowsBy (seq ["Coordinator"; "Year"; "Week"]) (seq ["Job Number"]) Stats.count
                |> Frame.sortRows "Coordinator"
                |> Frame.renameColumn "Job Number" "Values"
                |> Frame.addCol "Report Type"
                    ((seq [for row in [0..jobsSold.Rows.KeyCount-1] do yield "Jobs Sold Count"])
                     |> Series.ofValues)
                |> Frame.addCol "Employee Type"
                    ((seq [for row in [0..jobsSold.Rows.KeyCount-1] do yield "Coordinator"])
                     |> Series.ofValues)
                |> Frame.renameColumn "Coordinator" "Employee"
           
            let coordinatorSoldTons =
                jobsSold
                |> Frame.merge_On jobInfoByJobNumber "Job Number" "" 
                |> Frame.addCol "Week"
                    (jobsSold.GetColumn("J. PO Date")
                     |> Series.mapValues (fun value -> 
                                             System.Globalization.GregorianCalendar().GetWeekOfYear(
                                              System.DateTime.Parse(value),
                                              System.Globalization.CalendarWeekRule.FirstDay,
                                              System.DayOfWeek.Sunday)))
                |> Frame.addCol "Year"
                    (jobsSold.GetColumn("J. PO Date")
                    |> Series.mapValues (fun value -> System.DateTime.Parse(value).Year))
                |> Frame.aggregateRowsBy (seq ["Coordinator"; "Year"; "Week"]) (seq ["Total Tons"]) Stats.sum
                |> Frame.sortRows "Coordinator"
                |> Frame.renameColumn "Total Tons" "Values"
                |> Frame.addCol "Report Type"
                    ((seq [for row in [0..jobsSold.Rows.KeyCount-1] do yield "Sold Tons"])
                     |> Series.ofValues)
                |> Frame.addCol "Employee Type"
                    ((seq [for row in [0..jobsSold.Rows.KeyCount-1] do yield "Coordinator"])
                     |> Series.ofValues)
                |> Frame.renameColumn "Coordinator" "Employee"


            let coordinatorData =
                coordinatorQuotedJobsCount
                |> Frame.stack coordinatorQuotedTons
                |> Frame.stack coordinatorSoldJobCount
                |> Frame.stack coordinatorSoldTons

            let allData =
                estimatorData
                |> Frame.stack coordinatorData
            
            //estimatorData.SaveCsv(@"Output\Estimator Analysis.csv")
            //coordinatorData.SaveCsv(@"Output\Coordinator Analysis.csv")
            allData.SaveCsv(@"Output\Employee Analysis.csv")

            //let app = new Microsoft.Office.Interop.Excel.ApplicationClass(Visible = false)
            //let workbook = app.Workbooks.Open(@"Output\EstimatorAnalysis.csv")
            //let worksheet = (estAnalyCsv.Worksheets.[0] :?> Worksheet)
            let outputPath = System.IO.Path.GetFullPath(@"Output\")
            let resourcePath = System.IO.Path.GetFullPath(@"Resources\")

            let app = new ApplicationClass(Visible = false)
            let csv = app.Workbooks.Open(outputPath + "Employee Analysis.csv")
            let csvSheet = (csv.Worksheets.[1] :?> Worksheet)
            let employeeAnaly = app.Workbooks.Open(resourcePath + "Employee Analysis - Blank.xlsx")
            let employeeAnalySheet = (employeeAnaly.Worksheets.[1] :?> Worksheet)
            let csvRange = csvSheet.UsedRange
            employeeAnalySheet.Range("A2", "F" + string (csvRange.Rows.Count + 1)).Value2 <- csvSheet.UsedRange.Value2
            employeeAnalySheet.Range("A2:F2").Delete() |> ignore
            let tableSheet = (employeeAnaly.Worksheets.[2] :?> Worksheet)
            let table = (tableSheet.PivotTables("PivTabEmployeeAnalysis") :?> PivotTable)
            table.RefreshTable() |> ignore

            app.DisplayAlerts <- false
            employeeAnaly.SaveAs(outputPath + "Employee Analysis.xlsx")
            
                    
            Marshal.ReleaseComObject(csvSheet) |> ignore
            Marshal.ReleaseComObject(employeeAnalySheet) |> ignore
            Marshal.ReleaseComObject(tableSheet)  |> ignore
            csv.Close()
            Marshal.ReleaseComObject(csv)  |> ignore
            employeeAnaly.Close()
            Marshal.ReleaseComObject(employeeAnaly) |> ignore          
            app.Quit()
            Marshal.ReleaseComObject(app) |> ignore
            System.GC.Collect()
            System.IO.File.Delete(outputPath + "Employee Analysis.csv")



            


            


    
        
        estimatorAnalysis ()
        printfn "%s" "All Finished"

    open Deedle

    let test () =

        let removeDuplicateRows index (frame : Frame<'a, 'b>) =
          let unique = Seq.distinctBy (fun (a, b) -> a) (frame.GroupRowsBy(index).RowKeys)
          let nonDupKeys = 
              [for tup in unique do
                  let value, key = tup
                  yield key]
          frame.Rows.[nonDupKeys]
        
        
        let merge_On (infoFrame : Frame<'c, 'b>) column missingReplacement (initialFrame : Frame<'a,'b>) =
              let frame = initialFrame.Clone()
              let infoFrame = infoFrame.Clone()
                               |> removeDuplicateRows column 
                               |> Frame.indexRows column
              let initialSeries = frame.GetColumn(column)
              let infoFrameRows = infoFrame.RowKeys
              for colKey in infoFrame.ColumnKeys do
                  let newSeries =
                      [for v in initialSeries.ValuesAll do
                            if Seq.contains v infoFrameRows then  
                                let key = infoFrame.GetRow(v)
                                yield key.[colKey]
                            else
                                yield box missingReplacement ]
                  frame.AddColumn(colKey, newSeries)
              frame


        let primaryFrame =
             [(0, "Job Name", box "Job 1")
              (0, "City, State", box ("Reno", "NV"))
              (1, "Job Name", box "Job 2")
              (1, "City, State", box ("Portland", "OR"))
              (2, "Job Name", box "Job 3")
              (2, "City, State", box ("Portland", "OR"))
              (3, "Job Name", box "Job 4")
              (3, "City, State", box ("Sacramento", "CA"))] |> Frame.ofValues

        let infoFrame =
            [(0, "City, State", box ("Reno", "NV"))
             (0, "Lat", box "Reno_NV_Lat")
             (0, "Long", box "Reno_NV_Long")
             (1, "City, State", box ("Portland", "OR"))
             (1, "Lat", box "Portland_OR_Lat")
             (1, "Long", box "Portland_OR_Long")] |> Frame.ofValues


        primaryFrame |> merge_On infoFrame "City, State" null



        let fizzBuzz n = 
            [1..n]
            |> List.map (fun n ->
                          match n with
                          | fizzBuzz when fizzBuzz % 3 = 0 && fizzBuzz % 5 = 0 -> box "FizzBuzz"
                          | fizz when fizz % 3 = 0 -> box "Fizz"
                          | buzz when buzz % 5 = 0 -> box "Buzz"
                          | _ as value -> box value)
            |> List.iter (fun x -> printf "%A, " x)

        fizzBuzz 100

        let mutable counter = 0
        for x in [1..10] do
            counter <- counter + 1
            printf "%i, " counter

        let fizzBuzzList n =
            let rec fizzBuzz n acc =
                match n with
                | 0 -> acc
                | n when n % 3 = 0 && n % 5 = 0 -> fizzBuzz (n-1) ("FizzBuzz" :: acc)
                | n when n % 3 = 0 -> fizzBuzz (n-1) ("Fizz" :: acc)
                | n when n % 5 = 0 -> fizzBuzz (n-1) ("Buzz" :: acc)
                | _ as n -> fizzBuzz (n-1) ((sprintf "%i" n) :: acc)
            fizzBuzz n []

        let printFizzBuzz n =
            fizzBuzzList n
            |> List.iter (fun s -> printfn "%s" s)

        printFizzBuzz 1000

        let rec fizzBuzz n =
            match n with
            | 0 -> ""
            | n when n % 3 = 0 && n % 5 = 0 -> fizzBuzz(n-1) + ", FizzBuzz"
            | n when n % 3 = 0 -> fizzBuzz(n-1) + ", Fizz "
            | n when n % 5 = 0 -> fizzBuzz(n-1) + ", Buzz "
            | _ as n -> fizzBuzz(n-1) + sprintf ", %i " n
          
        fizzBuzz 100000
   

         