open NMBS_Tools.DSM_Analysis
open NMBS_Tools.EmployeeReports
open NMBS_Tools.CustomerReports
open NMBS_Tools.BOM_Seismic_Seperation
open NMBS_Tools.TCWidths
open System

[<EntryPoint>]
[<STAThreadAttribute>]
let main argv = 

    //FeedbackReport.sendAllFeedbackToExcel()

    //EmployeeReport.createEmployeeReport()

    //CustomerReports.createCustomerAnalysis()
    
    (*
    printfn "Please enter Sds (then click enter): "
    let sds = float (System.Console.ReadLine())

    
    let reportPath =
        let openFile = new System.Windows.Forms.OpenFileDialog()
        openFile.Filter <- "Excel files (*.xlsx)|*.xlsx"
        openFile.Title <- "Select BOM"
        if (openFile.ShowDialog())= (System.Windows.Forms.DialogResult.OK) then
            Some openFile.FileName
        else
            None

    match reportPath with
    | Some reportPath -> Seperator.getAllBomInfo reportPath sds |> ignore
                         ()
    | None -> printfn "No BOM Selected."
        
    *)

    CreateReport.TCAnalysis()

    printfn "Click enter to exit."

    let s = System.Console.ReadLine()
    0 // return an integer exit code
