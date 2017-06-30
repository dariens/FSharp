open NMBS_Tools.DSM_Analysis
open NMBS_Tools.EmployeeReports
open NMBS_Tools.CustomerReports

[<EntryPoint>]
let main argv = 

    //FeedbackReport.sendAllFeedbackToExcel()

    EmployeeReport.createEmployeeReport()

    //CustomerReports.createCustomerAnalysis()

    printfn "Click enter to exit."

    let s = System.Console.ReadLine()
    0 // return an integer exit code
