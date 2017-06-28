namespace NMBS_Tools.DSM_Analysis

module FeedbackReport =
    #if INTERACTIVE
    #r "Microsoft.Office.Interop.Excel.dll"
    System.Environment.CurrentDirectory <- @"C:\Users\darien.shannon\Documents\Code\F#\FSharp\NMBS_TOOLS\NMBS_TOOLS\bin\Debug"
    #endif

    open System
    open Microsoft.Office.Interop.Excel
    open System.IO
    open System.Runtime.InteropServices

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

    type ExcelInfo =
        | JoistFeedback of JFeedback

    let private getAllInfo (reportPaths : string list) (getInfoFunction : Workbook -> 'TOutput list) =
        let tempExcelApp = new Microsoft.Office.Interop.Excel.ApplicationClass(Visible = false)
        let info =
            [for reportPath in reportPaths do
                let tempReportPath = System.IO.Path.GetTempFileName()
                File.Delete(tempReportPath)
                File.Copy(reportPath, tempReportPath)
                let workbook = tempExcelApp.Workbooks.Open(tempReportPath)
                yield getInfoFunction workbook
                workbook.Close()
                Marshal.ReleaseComObject(workbook) |> ignore
                System.GC.Collect() |> ignore
                printfn "Finished processing %s." reportPath] |> List.concat
        tempExcelApp.Quit()
        Marshal.ReleaseComObject(tempExcelApp) |> ignore
        System.GC.Collect() |> ignore
        printfn "Finished processing all files."
        info

        /// Get Joist FB Info
    let getJoistFeedbackFromSheet (worksheet : Worksheet ) =
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
                yield JoistFeedback(joistFeedback)]
                
        feedback

    let getJoistFeedbackFromWorkbook (workbook : Workbook) =
        let returns =
            [ for s in workbook.Worksheets do
                let s = s :?> Worksheet
                let invalidSheets = ["Current"; "JOIST FEEDBACK"; "DECK FEEDBACK"; "Tables"]
                if (List.contains s.Name invalidSheets) = false then
                    yield getJoistFeedbackFromSheet s ]
        let jfeedbackList = List.concat returns
        jfeedbackList

    let dsmReports =
        let reportPath = @"Data\Weekly Sales Reports"
        let reportDirectory = new DirectoryInfo(reportPath)
        let files = reportDirectory.GetFiles();
        [for file in files do yield file.FullName]


    let getAllFeedback () =
        let excelInfoList = getAllInfo dsmReports getJoistFeedbackFromWorkbook
        [for info in excelInfoList do
            match info with
            | JoistFeedback fb -> yield fb ]

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
        

        let outputPath = System.IO.Path.GetFullPath(@"Output\")
        let resourcePath = System.IO.Path.GetFullPath(@"Resources\")


        workbook.SaveAs(outputPath + @"Feedback Analysis.xlsx")
        Marshal.ReleaseComObject(worksheet) |> ignore
        workbook.Close()
        Marshal.ReleaseComObject(workbook) |> ignore
        app.Quit()
        Marshal.ReleaseComObject(app) |> ignore
        System.GC.Collect()

    
    let cleanFeedback (feedbackList : JFeedback list) =
        let cleanName (name : string) =
            let name = name.Trim().ToUpper()
            match name with
            | "CAN AM" | "CAN AM NO BID"       -> "CANAM"
            | "NMBS`"                          -> "NMBS"
            | "VALLEY NO BID" | "VALLEY JOIST" -> "VALLEY"
            | "VUL"                            -> "VULCRAFT"
            | _                                -> name
        
        let cleanFeedbackLines (feedback : JFeedback) =
            let feedbackLines =
                feedback.FeedbackLines
                |> List.map (fun f -> {f with Company = cleanName (string f.Company)})
                |> List.filter (fun f -> Convert.ToDouble(f.Tons) <> 0.0 &&
                                         Convert.ToDouble(f.SellingPrice) <> 0.0 &&
                                         Convert.ToDouble(f.PricePerTonDel) <> 0.0 &&
                                         Convert.ToDouble(f.PricePerTonPlant) <> 0.0)
            {feedback with FeedbackLines = feedbackLines }

        feedbackList |> List.map cleanFeedbackLines          

    let sendAllFeedbackToExcel () =
        let feedback = cleanFeedback (getAllFeedback())
        feedbackToExcel feedback
        printfn "Complete!"

    sendAllFeedbackToExcel()
