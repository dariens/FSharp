open NMBS_Tools.DSM_Analysis
open NMBS_Tools.EmployeeReports
open NMBS_Tools.CustomerReports
open NMBS_Tools.BOM_Seismic_Seperation

[<EntryPoint>]
let main argv = 

    //FeedbackReport.sendAllFeedbackToExcel()

    //EmployeeReport.createEmployeeReport()

    //CustomerReports.createCustomerAnalysis()

    let bomInfo = NMBS_Tools.BOM_Seismic_Seperation.Seperator.allBomInfo()

    let test = bomInfo.Joists
               |> List.map (fun joist -> joist.LC3Loads bomInfo.Loads 1.0)
               |> List.filter (fun this -> this.IsEmpty = false)



    printfn "Click enter to exit."

    let s = System.Console.ReadLine()
    0 // return an integer exit code
