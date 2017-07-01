namespace NMBS_Tools.BOM_Seismic_Seperation

module Seperator =
    #if INTERACTIVE
    //#r "../packages/Deedle.1.2.5/lib/net40/Deedle.dll"
    #r "Microsoft.Office.Interop.Excel.dll"
    //System.Environment.CurrentDirectory <- @"C:\Users\darien.shannon\Documents\Code\F#\FSharp\NMBS_TOOLS\NMBS_TOOLS\bin\Debug"
    #endif

    open System
    open System.IO
    open Microsoft.Office.Interop.Excel
    open System.Runtime.InteropServices
    open NMBS_Tools.ArrayExtensions
    
    let getAllInfo (reportPath : string ) (getInfoFunction : Workbook -> 'TOutput) =
        let tempExcelApp = new Microsoft.Office.Interop.Excel.ApplicationClass(Visible = false)
        tempExcelApp.DisplayAlerts <- false
        let tempReportPath = System.IO.Path.GetTempFileName()      
        File.Delete(tempReportPath)
        File.Copy(reportPath, tempReportPath)
        let workbook = tempExcelApp.Workbooks.Open(tempReportPath)
        let info = getInfoFunction workbook
        workbook.Close(false)
        Marshal.ReleaseComObject(workbook) |> ignore
        System.GC.Collect() |> ignore
        printfn "Finished processing %s." reportPath 
        tempExcelApp.Quit()
        Marshal.ReleaseComObject(tempExcelApp) |> ignore
        System.GC.Collect() |> ignore
        printfn "Finished processing all files."
        info
   
    
    let getAllLoadsAsArray (bom: Workbook) =
        let arrayList =
            seq [for sheet in bom.Worksheets do
                    let sheet = (sheet :?> Worksheet)
                    if sheet.Name.Contains("L (") then
                        yield sheet.Range("A14","M55").Value2 :?> obj [,]]    
        Array2D.joinMany (Array2D.joinByRows) arrayList


    let excelPaths = @"C:\Users\darien.shannon\Desktop\4317-0092 Joist BOMs-For_Import_06-28-17.xlsm"

    let loads = getAllInfo excelPaths getAllLoadsAsArray

    type Load =
        {
        Number : string;
        Type : string;
        Category : string
        Position : string
        Load1Value : float
        Load1DistanceFt : float
        Load1DistanceIn : float
        Load2DistanceFt : float
        Load2DistanceIn : float
        Ref : string
        LoadCase : string
        }





                       



    



    

    


