open NMBS_Tools.DSM_Analysis
open NMBS_Tools.EmployeeReports
open NMBS_Tools.CustomerReports
open NMBS_Tools.BOM_Seismic_Seperation

[<EntryPoint>]
let main argv = 

    //FeedbackReport.sendAllFeedbackToExcel()

    //EmployeeReport.createEmployeeReport()

    //CustomerReports.createCustomerAnalysis()

    let loadNotes = NMBS_Tools.BOM_Seismic_Seperation.Seperator.getAllLoadNotes()


    printfn "Click enter to exit."

    let s = System.Console.ReadLine()
    0 // return an integer exit code
