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

    let getAllInfo (reportPath : string ) (getInfoFunction : Workbook -> 'TOutput) =
        let tempExcelApp = new Microsoft.Office.Interop.Excel.ApplicationClass(Visible = false)
        let tempReportPath = System.IO.Path.GetTempFileName()
        File.Delete(tempReportPath)
        File.Copy(reportPath, tempReportPath)
        let workbook = tempExcelApp.Workbooks.Open(tempReportPath)
        let info = getInfoFunction workbook
        workbook.Close()
        Marshal.ReleaseComObject(workbook) |> ignore
        System.GC.Collect() |> ignore
        printfn "Finished processing %s." reportPath 
        tempExcelApp.Quit()
        Marshal.ReleaseComObject(tempExcelApp) |> ignore
        System.GC.Collect() |> ignore
        //printfn "Finished processing all files."
        info
   
   
    module Array2D =

        // Some generic functions for a single array that are required for the merge function but may also be helpful in some other cases:
    
        // arrayDimension = 0 for rows and 1 for columns,
        // index represents the index of the row or column that you want 
        let getSlice arrayDimension index (array: 'T[,]) = 
            match arrayDimension with
            | 0 -> array.[index..index, *] |> Seq.cast<'T> |> Seq.toArray
            | 1 -> array.[*,index..index] |> Seq.cast<'T> |> Seq.toArray
            | _ -> failwith "arrayDimension must be either 0 for rows and 1 for columns"
            // getSlice 0 1 read "get the second row"
            // getSlice 1 0 reads "get the first column"

        // Converts an Array2D into a list of array's
        // arrayDimension = 0 for rows and 1 for columns, 
        let toArrayOfSlices arrayDimension (array : 'T[,]) : 'T[] list =
            let iteratorDimension =
                match arrayDimension with
                | 0 -> 1
                | 1 -> 0
                | _ -> failwith "arrayDimension must be either 0 for rows and 1 for columns"
            [for i = 0 to array.GetLength(arrayDimension) - 1 do
                yield array |> getSlice arrayDimension i ]


        // MergeType.
        // Join is side-by-side and stack is on top of each-other.
        type MergeType = Join=0 | Stack=1

        let merge (array1 : 'T [,]) (array2 : 'T [,]) (mergeType : MergeType) =

            // this is the dimension that is used in the array.GetLength function
            // It will be 0 for rows and 1 for columns. 
            let maintainedArrayDim =
                match mergeType with
                | MergeType.Join -> 0 // the number of rows remains the same when joining
                | MergeType.Stack -> 1 // the number of columns remain the same when stacking
                | _ -> failwith "mergeType enum value must be either 0 for Join, or 1 for Stack"

            // Length of 'maintainedArrayDim' must be equal for both arrays
            // i.e. for join, both arrays must have the same number of rows
            // for stack, both arrays must have the same number of columns

            let maintainedDimLength = array1.GetLength(maintainedArrayDim)
            if array1.GetLength(maintainedArrayDim) <> array2.GetLength(maintainedArrayDim) then
                failwith @"Both arrays must have the same number of columns for Array.Stack and same number of rows for Array.Join"

            else
                // create list of array's from rows for each array 
                let array1Slices = array1 |> toArrayOfSlices 0  
                let array2Slices = array2 |> toArrayOfSlices 0

                // For Join type we need to do some funky business.
                // For Stack type we just need to use List.append and create a array2D out of it. 
                match mergeType with 
                | MergeType.Join ->
                    array2D [for i=0 to (maintainedDimLength - 1) do
                                    yield Array.concat [array1Slices.[i] ; array2Slices.[i]]]
                | MergeType.Stack ->
                    array2D (List.append array1Slices array2Slices)
                | _ -> failwith "mergeType enum value must be either 0 for Join, or 1 for Stack"
 
    let getAllLoadsAsArray (bom: Workbook) =
        let arrayList =
            [for sheet in bom.Worksheets do
                let sheet = (sheet :?> Worksheet)
                if sheet.Name.Contains("L (") then
                    yield sheet.Range("A14","M55").Value2 :?> obj [,]]

    

        //Array2D.joinMany (Array2D.joinByCols) arrayList
        //[List.fold (Array2D.joinByCols) (array2D [[];[]]) arrayList]
        arrayList


    let excelPaths = @"C:\Users\darien.shannon\Desktop\4317-0092 Joist BOMs-For_Import_06-28-17.xlsm"

    let loads = getAllInfo excelPaths getAllLoadsAsArray

    
    for load in loads do
        printfn "Rows: %i, Cols: %i" (load.GetLength(0)) (load.GetLength(1))





                       



    



    

    


