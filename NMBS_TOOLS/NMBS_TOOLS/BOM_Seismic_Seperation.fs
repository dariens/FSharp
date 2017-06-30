namespace NMBS_Tools.BOM_Seismic_Seperation

module Seperator =
    #if INTERACTIVE
    //#r "../packages/Deedle.1.2.5/lib/net40/Deedle.dll"
    #r "Microsoft.Office.Interop.Excel.dll"
    //System.Environment.CurrentDirectory <- @"C:\Users\darien.shannon\Documents\Code\F#\FSharp\NMBS_TOOLS\NMBS_TOOLS\bin\Debug"
    #endif

    open System
    open NMBS_Tools.FrameExtensions
    open Microsoft.Office.Interop.Excel
    open System.Runtime.InteropServices

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
    
    let getAllLoadsAsArray (bom: Workbook) =
        let arrayList =
            [for sheet in bom.Worksheets do
                let sheet = (sheet :?> Worksheet)
                yield sheet.Range("A14","M55").Value2 :?> obj array]
        arrayList
        //let rec combineArrays arrayList =
        //    match arrayList with
        //    | [] -> array2D [[];[]]
        //    | head :: tail -> head |> Array2D.append (combineArrays tail)   /// Array2D.append ??? createa  function for this
        //combineArrays arrayList

    let array1 = array2D [[111;112]; [121;122]]
    let array2 = array2D [[211;212]; [221;222]]
    let arrayList = [array1;array2]

    module Array2D =
        let append (array1 : 'T [,]) (array2 : 'T [,]) =
            sprintf "temp"

    


