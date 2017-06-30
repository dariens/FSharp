namespace NMBS_Tools.EmployeeReports


module EmployeeReport =
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

    let createEmployeeReport () =

        let calander = new System.Globalization.GregorianCalendar()

        let addWeekAndYearColumns dateColumnKey calander frame =
            frame
                |> Frame.addCol "Week"
                    (frame.GetColumn(dateColumnKey)
                        |> Series.mapValues (fun value -> 
                                                System.Globalization.GregorianCalendar().GetWeekOfYear(
                                                     System.DateTime.Parse(value),
                                                     System.Globalization.CalendarWeekRule.FirstDay,
                                                     System.DayOfWeek.Sunday)))
                |> Frame.addCol "Year"
                    (frame.GetColumn(dateColumnKey)
                    |> Series.mapValues (fun value -> System.DateTime.Parse(value).Year))


        let estimatorAnalysis () =
            let estimatorQuotedJobsCount jobsQuoted =
                jobsQuoted
                |> addWeekAndYearColumns "Date Quoted" calander
                |> Frame.aggregateRowsBy (seq ["TakeoffPerson"; "Year"; "Week"]) (seq ["Job Number"]) Stats.count
                |> Frame.sortRows "TakeoffPerson"
                |> Frame.renameColumn "Job Number" "Values"
                |> Frame.addCol "Report Type"
                    ((seq [for row in [0..jobsQuoted.Rows.KeyCount-1] do yield "Jobs Quoted Count"])
                        |> Series.ofValues)
                |> Frame.addCol "Employee Type"
                    ((seq [for row in [0..jobsQuoted.Rows.KeyCount-1] do yield "Estimator"])
                        |> Series.ofValues)
                |> Frame.renameColumn "TakeoffPerson" "Employee"
           
            let estimatorQuotedTons jobsQuoted =
                jobsQuoted
                |> addWeekAndYearColumns "Date Quoted" calander
                |> Frame.aggregateRowsBy (seq ["TakeoffPerson"; "Year"; "Week"]) (seq ["Total Tons"]) Stats.sum
                |> Frame.sortRows "TakeoffPerson"
                |> Frame.renameColumn "Total Tons" "Values"
                |> Frame.addCol "Report Type"
                    ((seq [for row in [0..jobsQuoted.Rows.KeyCount-1] do yield "Tons Quoted"])
                        |> Series.ofValues)
                |> Frame.addCol "Employee Type"
                    ((seq [for row in [0..jobsQuoted.Rows.KeyCount-1] do yield "Estimator"])
                        |> Series.ofValues)
                |> Frame.renameColumn "TakeoffPerson" "Employee"

            let estimatorSoldJobCount jobsSold =
                jobsSold
                |> addWeekAndYearColumns "J. PO Date" calander
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
           
            let estimatorSoldTons jobsSold =
                jobsSold
                |> addWeekAndYearColumns "J. PO Date" calander
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


            let coordinatorQuotedJobsCount jobsQuoted =
                jobsQuoted
                |> addWeekAndYearColumns "Date Quoted" calander
                |> Frame.aggregateRowsBy (seq ["Coordinator"; "Year"; "Week"]) (seq ["Job Number"]) Stats.count
                |> Frame.sortRows "Coordinator"
                |> Frame.renameColumn "Job Number" "Values"
                |> Frame.addCol "Report Type"
                    ((seq [for row in [0..jobsQuoted.Rows.KeyCount-1] do yield "Jobs Quoted Count"])
                        |> Series.ofValues)
                |> Frame.addCol "Employee Type"
                    ((seq [for row in [0..jobsQuoted.Rows.KeyCount-1] do yield "Coordinator"])
                        |> Series.ofValues)
                |> Frame.renameColumn "Coordinator" "Employee"

            
           
            let coordinatorQuotedTons jobsQuoted =
                jobsQuoted
                |> addWeekAndYearColumns "Date Quoted" calander
                |> Frame.aggregateRowsBy (seq ["Coordinator"; "Year"; "Week"]) (seq ["Total Tons"]) Stats.sum
                |> Frame.sortRows "Coordinator"
                |> Frame.renameColumn "Total Tons" "Values"
                |> Frame.addCol "Report Type"
                    ((seq [for row in [0..jobsQuoted.Rows.KeyCount-1] do yield "Tons Quoted"])
                        |> Series.ofValues)
                |> Frame.addCol "Employee Type"
                    ((seq [for row in [0..jobsQuoted.Rows.KeyCount-1] do yield "Coordinator"])
                        |> Series.ofValues)
                |> Frame.renameColumn "Coordinator" "Employee"

           
            let coordinatorSoldJobCount jobsSold jobsQuoted=               
                jobsSold
                |> Frame.merge_On
                    (jobsQuoted |> Frame.getSpecificColumns ["Job Number"; "Coordinator"])
                    "Job Number" "" 
                |> addWeekAndYearColumns "J. PO Date" calander
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
           
            let coordinatorSoldTons jobsSold jobsQuoted =
                jobsSold
                |> Frame.merge_On
                    (jobsQuoted |> Frame.getSpecificColumns ["Job Number"; "Coordinator"])
                    "Job Number" "" 
                |> addWeekAndYearColumns "J. PO Date" calander
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


            let dataPath = System.IO.Path.GetFullPath(@"Data\")

            let xlsToCsv (file: string) =
            
                let fileName = file.Substring(0, file.LastIndexOf("."))
                if System.IO.File.Exists(file) then
                    let app = new ApplicationClass(Visible = false)
                    try
                        app.DisplayAlerts = false |> ignore
                        let xls = app.Workbooks.Open(fileName)
                        xls.SaveAs(fileName + ".csv",Microsoft.Office.Interop.Excel.XlFileFormat.xlCSV)
                        xls.Close(false)
                        Marshal.ReleaseComObject(xls) |> ignore
                    finally
                        app.Quit()
                        Marshal.ReleaseComObject(app) |> ignore

            

            let jobsSold =
                xlsToCsv (dataPath + "Jobs Sold.xls")
                Frame.ReadCsv(dataPath + "Jobs Sold.csv")

            let jobsQuoted = 
                xlsToCsv (dataPath + "Jobs Quoted.xls")
                Frame.ReadCsv(dataPath + "Jobs Quoted.csv")

            let estimatorData =
                (estimatorQuotedJobsCount jobsQuoted)
                |> Frame.stack (estimatorQuotedTons jobsQuoted)
                |> Frame.stack (estimatorSoldJobCount jobsSold)
                |> Frame.stack (estimatorSoldTons jobsSold)

            let coordinatorData =
                (coordinatorQuotedJobsCount jobsQuoted)
                |> Frame.stack (coordinatorQuotedTons jobsQuoted)
                |> Frame.stack (coordinatorSoldJobCount jobsSold jobsQuoted)
                |> Frame.stack (coordinatorSoldTons jobsSold jobsQuoted)

            let allData =
                estimatorData
                |> Frame.stack coordinatorData
            
            allData.SaveCsv(@"Output\Employee Analysis.csv")


            let outputPath = System.IO.Path.GetFullPath(@"Output\")
            let resourcePath = System.IO.Path.GetFullPath(@"Resources\")

            
            let app = new ApplicationClass(Visible = false)
            try
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
            finally         
                app.Quit()
                Marshal.ReleaseComObject(app) |> ignore
                System.GC.Collect()

            if System.IO.File.Exists(outputPath + "Employee Analysis.csv") then
                System.IO.File.Delete(outputPath + "Employee Analysis.csv")

        estimatorAnalysis()
        printfn "All Complete!"
