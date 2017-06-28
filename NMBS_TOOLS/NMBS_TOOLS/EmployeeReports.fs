namespace NMBS_Tools.EmployeeReports


module EmployeeReport =
    open Deedle
    open System
    open NMBS_Tools.FrameExtensions
    open Microsoft.Office.Interop.Excel
    open System.Runtime.InteropServices

    let createEmployeeReport () =
        #if INTERACTIVE
        #r "../packages/Deedle.1.2.5/lib/net40/Deedle.dll"
        System.Environment.CurrentDirectory <- @"C:\Users\darien.shannon\Documents\Code\F#\FSharp\NMBS_TOOLS\NMBS_TOOLS\bin\Debug"
        #endif

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
            
            allData.SaveCsv(@"Output\Employee Analysis.csv")


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

        estimatorAnalysis()
        printfn "All Complete!"
