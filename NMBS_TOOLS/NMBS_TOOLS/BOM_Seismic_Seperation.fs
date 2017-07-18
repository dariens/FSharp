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
        Type : string;
        Category : string
        Position : obj
        Load1Value : obj
        Load1DistanceFt : obj
        Load1DistanceIn : obj
        Load2Value : obj
        Load2DistanceFt : obj
        Load2DistanceIn : obj
        Ref : obj
        LoadCases : int list
        }

        member this.LoadCaseString =
            match this.LoadCases with
            | [] -> ""
            | _ -> 
                this.LoadCases
                |> List.map string
                |> List.reduce (fun s1 s2 -> s1 + "," + s2)
            
        static member create(loadType, category, position, load1Value, load1DistanceFt, load1DistanceIn, load2Value, load2DistanceFt, load2DistanceIn, ref, loadcases) =
            {Type = loadType; Category = category; Position = position; Load1Value = load1Value;
             Load1DistanceFt = load1DistanceFt; Load1DistanceIn = load1DistanceIn; Load2Value = load2Value;
             Load2DistanceFt = load2DistanceFt; Load2DistanceIn = load2DistanceIn; Ref = ref; LoadCases = loadcases}

    type LoadNote =
        {
        LoadNumber : string
        Load : Load
        }

    let getLoadNotes (note : string) =
        let loadNoteStart = note.IndexOf("(")
        let loadNotes = note.Substring(loadNoteStart)
        let loadNotes = loadNotes.Split([|"("; ","; ")"|], StringSplitOptions.RemoveEmptyEntries)
        let loadNotes = loadNotes |> List.ofArray
        loadNotes |> List.map (fun (s : string) -> s.Trim())
    
    type Joist =
        {
        Mark : string
        JoistSize : string
        LoadNoteString : string option
        }

        member this.LoadNoteList =
            match (this.LoadNoteString) with
            | Some notes -> getLoadNotes notes
            | None -> []

        member this.UDL =
            let size = this.JoistSize
            if size.Contains("/") then
                let sizeAsArray = size.Split( [|"LH"; "K"; "/"|], StringSplitOptions.RemoveEmptyEntries)
                let TL = float sizeAsArray.[1]
                let LL = float sizeAsArray.[2]
                let DL = TL - LL
                Some(Load.create("U", "CL", "TC", DL,
                             null, null, null, null, null, null, [3]))
            else
                None       

        member this.Sds sds =
            match this.UDL with
            | Some udl -> 
                let sds = 0.14 * sds * System.Convert.ToDouble(udl.Load1Value)
                Some (Load.create ("U", "SM", "TC", sds,
                              null, null, null, null, null, null, [3]))
            | None -> None

        member this.LC3Loads (loadNotes :LoadNote list) sds =
            match this.UDL, (this.Sds sds) with
            | Some udl, Some sds ->
                loadNotes
                |> List.filter (fun note -> this.LoadNoteList |> List.contains note.LoadNumber)
                |> List.map (fun note -> note.Load)
                |> List.filter (fun load -> load.Category <> "WL" && load.Category <> "SM" && (load.LoadCases = [] || (load.LoadCases |> List.contains 1)))
                |> List.map (fun load -> {load with LoadCases = [3]})
                |> List.append [udl; sds]
            | _ -> []


    type _AdditionalJoist =
        {
        LocationFt : string
        LocationIn : string
        Load : float
        }

        member this.ToLoad() =
            {
            Type = "C"
            Category = "CL"
            Position = "TC"
            Load1Value = this.Load * 1000.0
            Load1DistanceFt = this.LocationFt
            Load1DistanceIn = this.LocationIn
            Load2Value = null
            Load2DistanceFt = null
            Load2DistanceIn = null
            Ref = null
            LoadCases = []
            }
    
    type AdditionalJoist =
        {
        Mark : string
        AdditionalJoists : Load list
        }

    type Girder =
        {
        Mark : string
        GirderSize : string
        OverallLengthFt : float
        OverallLengthIn : float
        TcxlLengthFt : float
        TcxlLengthIn : float
        TcxrLengthFt : float
        TcxrLengthIn : float
        LoadNoteString : string option
        AdditionalJoists : Load list

        }

        member this.LoadNoteList =
            match (this.LoadNoteString) with
            | Some notes -> Some (getLoadNotes notes)
            | None -> None

        member this.BaseLength =
            (this.OverallLengthFt + this.OverallLengthIn/12.0) -
             (this.TcxlLengthFt + this.TcxlLengthIn/12.0) -
              (this.TcxrLengthFt + this.TcxlLengthIn/12.0)

        member this.UDL =
            let size = this.GirderSize
            let sizeAsArray = size.Split( [|"G"; "BG"; "VG"; "N"; "K"|], StringSplitOptions.RemoveEmptyEntries)
            let numberOfSpaces = float sizeAsArray.[1]
            let load = sizeAsArray.[2].Split([|"/"|], StringSplitOptions.RemoveEmptyEntries)
            if (Array.length load) = 2 then
                let TL = float load.[0]
                let LL = float load.[1]
                let DL = 1000.0 * (TL - LL)
                let UDL = DL/(this.BaseLength / numberOfSpaces)
                Load.create("U", "CL", "TC", UDL,
                             null, null, null, null, null, null, [3])
            else
                failwith "GirderSize is not in correct format"

        member this.SDS sds = 
            let SDS = (Convert.ToDouble(this.UDL.Load1Value)) * 0.14 * sds
            Load.create("U", "SM", "TC", SDS,
                          null, null, null, null, null, null, [3])

        member this.LC3Loads (loadNotes :LoadNote list) sds =
                let additionalJoistLoads =
                    this.AdditionalJoists
                    |> List.map (fun load -> {load with LoadCases = [3]})
                loadNotes
                |> List.filter (fun note ->
                    match this.LoadNoteList with
                    | Some loadNoteList -> loadNoteList |> List.contains note.LoadNumber
                    | None -> false)
                |> List.map (fun note -> note.Load)
                |> List.filter (fun load -> load.Category <> "WL" && load.Category <> "SM" && (load.LoadCases = [] || (load.LoadCases |> List.contains 1)))
                |> List.map (fun load -> {load with LoadCases = [3]})
                |> List.append [this.UDL; this.SDS sds]
                |> List.append additionalJoistLoads


            



    module CleanBomInfo =

        let nullableToOption<'T> value =
            match (box value) with
            | null  -> None
            | value when value = (box "") -> None
            | _ -> Some ((box value) :?> 'T)


        module CleanLoads =

            let getLoadCases (loadCaseString: string) =
                let loadNotes = loadCaseString.Split([|","|], StringSplitOptions.RemoveEmptyEntries)
                let loadNotes = loadNotes
                                |> List.ofArray
                                |> List.map (fun string -> System.Int32.Parse(string))
                loadNotes

            let getLoadFromArraySlice (a : obj []) =
                {
                Type = string a.[1]
                Category = string a.[2]
                Position = a.[3]
                Load1Value = a.[5]
                Load1DistanceFt =  a.[6]
                Load1DistanceIn = a.[7]
                Load2Value = a.[8]
                Load2DistanceFt = a.[9]
                Load2DistanceIn = a.[10]
                Ref = a.[11]
                LoadCases = getLoadCases (string a.[12])
                }

            let getLoadNotesFromArray (a2D : obj[,]) =
                let mutable startIndex = Array2D.base1 a2D 
                let endIndex = (a2D |> Array2D.length1) - (if startIndex = 0 then 1 else 0)
                let loadNotes = 
                    let mutable loadNumber = ""
                    [for currentIndex = startIndex to endIndex do
                        if a2D.[currentIndex, 1] <> null && a2D.[currentIndex,1] <> (box "") then
                            if a2D.[currentIndex, 0] <> null && a2D.[currentIndex, 0] <> (box "") then
                                loadNumber <- (string a2D.[currentIndex, 0]).Trim()
                            yield {LoadNumber = loadNumber; Load = getLoadFromArraySlice a2D.[currentIndex, *]}]
                loadNotes

        module CleanJoists =

            let getJoistsFromArray (a2D : obj [,]) =
                let mutable startIndex = Array2D.base1 a2D 
                let endIndex = (a2D |> Array2D.length1) - (if startIndex = 0 then 1 else 0)
                let joists : Joist list =
                    [for currentIndex = startIndex to endIndex do
                        if a2D.[currentIndex, 0] <> null && a2D.[currentIndex, 0] <> (box "") then
                            yield
                                {
                                Mark = string a2D.[currentIndex, 0]
                                JoistSize = string a2D.[currentIndex, 2]
                                LoadNoteString = nullableToOption<string> a2D.[currentIndex, 26]
                                }]
                joists

        module CleanGirders =

            let toDouble (s : obj) =
                match (box s) with
                | v when v = (box "") -> 0.0
                | _ -> Convert.ToDouble(s)

            let getGirdersFromArray (a2D : obj [,]) =
                let mutable startIndex = Array2D.base1 a2D
                let colIndex = Array2D.base2 a2D
                let endIndex = (a2D |> Array2D.length1) - (if startIndex = 0 then 1 else 0)
                let girders : Girder list =
                    [for currentIndex = startIndex to endIndex do
                        if a2D.[currentIndex, colIndex] <> null && a2D.[currentIndex, colIndex] <> (box "") then
                            yield
                                {
                                Mark = string a2D.[currentIndex, colIndex]
                                GirderSize = string a2D.[currentIndex, colIndex + 2]
                                OverallLengthFt = toDouble(a2D.[currentIndex, colIndex + 3])
                                OverallLengthIn = toDouble(a2D.[currentIndex, colIndex + 4])
                                TcxlLengthFt = toDouble(a2D.[currentIndex, colIndex + 6])
                                TcxlLengthIn = toDouble(a2D.[currentIndex, colIndex + 7])
                                TcxrLengthFt = toDouble(a2D.[currentIndex, colIndex + 9])
                                TcxrLengthIn = toDouble(string a2D.[currentIndex, colIndex + 10])
                                LoadNoteString =  nullableToOption<string> a2D.[currentIndex, colIndex + 25]
                                AdditionalJoists = []
                                }]
                girders

            let getAdditionalJoistsFromArraySlice (a : obj []) =
                let mutable col = 16
                [while col <= 28 do
                    if (a.[col] <> null && a.[col] <> (box "")) || (a.[col + 1] <> null && a.[col + 1] <> (box "")) then
                        let additionalJoist =
                            {
                            LocationFt = string a.[col]
                            LocationIn = string a.[col + 1]
                            Load = Convert.ToDouble(a.[col + 2])
                            }
                        yield additionalJoist.ToLoad()
                    col <- col + 4]

            let getAdditionalJoistsFromArray (a2D : obj [,]) =
                let mutable startIndex = Array2D.base1 a2D
                let colIndex = Array2D.base2 a2D
                let endIndex = (a2D |> Array2D.length1) - (if startIndex = 0 then 1 else 0)
                let additionalJoists : AdditionalJoist list =
                    [for currentIndex = startIndex to endIndex do
                        if a2D.[currentIndex, colIndex] <> null && a2D.[currentIndex, colIndex] <> (box "") then
                            yield
                                {
                                Mark = string a2D.[currentIndex, colIndex]
                                AdditionalJoists = getAdditionalJoistsFromArraySlice a2D.[currentIndex, *]
                                } ]
                additionalJoists

            let addAdditionalJoistLoadsToGirders (girders: Girder list, additionalJoists : AdditionalJoist list) =
                [for girder in girders do
                    let additionalJoistsOnGirder = additionalJoists |> List.filter (fun addJoist -> addJoist.Mark = girder.Mark)
                    let additionalLoads =
                        [for addJoist in additionalJoistsOnGirder do
                            for load in addJoist.AdditionalJoists do
                                yield load] 
                    let additionalJoists = girder.AdditionalJoists |> List.append additionalLoads
                    yield {girder with AdditionalJoists = additionalJoists}]
                    
    let saveWorkbook title (workbook : Workbook) =
            let saveFile = new System.Windows.Forms.SaveFileDialog()
            saveFile.Filter <- "Excel files (*.xlsx)|*.xlsx"
            saveFile.Title <- title
            saveFile.ShowDialog() |> ignore
            if saveFile.FileName <> "" then
                workbook.CheckCompatibility <- false
                workbook.SaveAs(saveFile.FileName)
    
    let getAllInfo reportPath getInfoFunction modifyWorkbookFunction (sds : float) =
        let tempExcelApp = new Microsoft.Office.Interop.Excel.ApplicationClass(Visible = false)
        //let bom = tempExcelApp.Workbooks.Open(bomPath)
        try 
            tempExcelApp.DisplayAlerts <- false
            let tempReportPath = System.IO.Path.GetTempFileName()      
            File.Delete(tempReportPath)
            File.Copy(reportPath, tempReportPath)
            let workbook = tempExcelApp.Workbooks.Open(tempReportPath)
            let info = getInfoFunction workbook
            modifyWorkbookFunction workbook info sds
            
            workbook |> saveWorkbook "Save New BOM"

            workbook.Close(false)
            Marshal.ReleaseComObject(workbook) |> ignore
            System.GC.Collect() |> ignore
            printfn "Finished processing %s." reportPath 
            printfn "Finished processing all files."
            info
        finally
            tempExcelApp.Quit()
            Marshal.ReleaseComObject(tempExcelApp) |> ignore
            System.GC.Collect() |> ignore            

    let getInfo (bom: Workbook) =

        let workSheetNames = [for sheet in bom.Worksheets -> (sheet :?> Worksheet).Name] 

        let loads =
            let filteredWorkSheetNames = workSheetNames |> List.filter (fun name -> name.Contains("L ("))
            if (List.isEmpty filteredWorkSheetNames) then
                []
            else
                let arrayList =
                    seq [for sheet in bom.Worksheets do
                            let sheet = (sheet :?> Worksheet)
                            if sheet.Name.Contains("L (") then
                                let loads = sheet.Range("A14","M55").Value2 :?> obj [,]
                                yield loads]   
                let loadsAsArray = Array2D.joinMany (Array2D.joinByRows) arrayList
                let loads = CleanBomInfo.CleanLoads.getLoadNotesFromArray loadsAsArray
                loads

        let joists =
            let filteredWorkSheetNames = workSheetNames |> List.filter (fun name -> name.Contains("J ("))
            if (List.isEmpty filteredWorkSheetNames) then
                []
            else
                let arrayList =
                    seq [for sheet in bom.Worksheets do
                            let sheet = (sheet :?> Worksheet)
                            if sheet.Name.Contains("J (") then
                                if (sheet.Range("A21").Value2 :?> string) = "MARK" then
                                    yield sheet.Range("A23","AA40").Value2 :?> obj [,]
                                else
                                    yield sheet.Range("A16", "AA45").Value2 :?> obj [,]]

                let joistsAsArray = Array2D.joinMany (Array2D.joinByRows) arrayList
                let joists = CleanBomInfo.CleanJoists.getJoistsFromArray joistsAsArray
                joists

        let girdersAndAdditionalJoists =
            let filteredWorkSheetNames = workSheetNames |> List.filter (fun name -> name.Contains("G ("))
            if (List.isEmpty filteredWorkSheetNames) then
                []
            else
                let arrayList1 =
                    seq [for sheet in bom.Worksheets do
                            let sheet = (sheet :?> Worksheet)
                            if sheet.Name.Contains("G (") then
                                if (sheet.Range("A26").Value2 :?> string) = "MARK" then
                                    yield sheet.Range("A28","AA45").Value2 :?> obj [,]
                                else
                                    yield sheet.Range("A14", "AA45").Value2 :?> obj [,]]
                let arrayList2 =
                    seq [for sheet in bom.Worksheets do
                            let sheet = (sheet :?> Worksheet)
                            if sheet.Name.Contains("G (") then
                                yield sheet.Range("AB14","BG45").Value2 :?> obj [,]]
                let girdersAsArray = Array2D.joinMany (Array2D.joinByRows) arrayList1
                let girders = CleanBomInfo.CleanGirders.getGirdersFromArray girdersAsArray
                let additionalJoistsAsArray = Array2D.joinMany (Array2D.joinByRows) arrayList2
                let additionalJoists =
                    CleanBomInfo.CleanGirders.getAdditionalJoistsFromArray additionalJoistsAsArray

                (CleanBomInfo.CleanGirders.addAdditionalJoistLoadsToGirders (girders, additionalJoists))

        (joists, girdersAndAdditionalJoists, loads)


    type BomInfo = 
        {
        Joists : Joist list
        Girders : Girder list
        Loads : LoadNote list
        }

    let modifyWorkbookFunction (bom : Workbook) (bomInfo : Joist list * Girder list * LoadNote list) sds: Unit =
        
        let workSheetNames = [for sheet in bom.Worksheets -> (sheet :?> Worksheet).Name] 

        let switchSmToLc3 (a2D : obj [,]) =
            let startRow = Array2D.base1 a2D
            let endRow = (Array2D.length1 a2D) - (if startRow = 0 then 1 else 0)
            let startCol = Array2D.base2 a2D
            for currentIndex = startRow to endRow do
                let lc = (string a2D.[currentIndex, startCol + 12]).Trim()
                if a2D.[currentIndex, startCol + 2] = (box "SM") && (lc = "1" || lc = "") then
                    a2D.[currentIndex, startCol + 12] <- box "3"

        let changeSmLoadsToLC3() =
            let filteredWorkSheetNames = workSheetNames |> List.filter (fun name -> name.Contains("L ("))
            if (List.isEmpty filteredWorkSheetNames) then
                ()
            else
                for sheet in bom.Worksheets do
                    let sheet = (sheet :?> Worksheet)
                    if sheet.Name.Contains("L (") then
                        let loads = sheet.Range("A14","M55").Value2 :?> obj [,]
                        switchSmToLc3 loads
                        sheet.Range("A14", "M55").Value2 <- loads

        let addLoadNote (mark : string) (note : string) =
            if (mark.Length > 0 && note.Length > 0) then
                let loadNote = "S" + mark
                let insertLocation = note.IndexOf(")")
                let newNote = note.Substring(0, insertLocation) + ", " + loadNote + ")"
                newNote
            else
               ""

        let test = addLoadNote "J1" "[C] (2, 3, 5, 16)"

        let addLC3LoadsToLoadNotes() =
            let joists, girders, loads = bomInfo
            let joistsWithLC3Loads = joists |> List.filter (fun joist -> List.isEmpty (joist.LC3Loads loads sds) = false)
            let filteredWorkSheetNames = workSheetNames |> List.filter (fun name -> name.Contains ("J ("))
            if (List.isEmpty filteredWorkSheetNames) then ()
            else
                for sheet in bom.Worksheets do
                    let sheet = (sheet :?> Worksheet)
                    if sheet.Name.Contains("J (") then
                        let array =
                            if (sheet.Range("A21").Value2 :?> string) = "MARK" then
                                sheet.Range("A23","AA40").Value2 :?> obj [,]
                            else
                                sheet.Range("A16", "AA45").Value2 :?> obj [,]
                        for i = 1 to (Array2D.length1 array) - 1 do
                            let joistMarksWithLC3Loads =
                                joistsWithLC3Loads |> List.map (fun joist -> joist.Mark)
                            let mark = string array.[i, 1]
                            if (joistMarksWithLC3Loads |> List.contains mark) then
                                array.[i, 27] <- box (addLoadNote mark (string array.[i, 27]))
                        if (sheet.Range("A21").Value2 :?> string) = "MARK" then
                            sheet.Range("A23","AA40").Value2 <- array
                        else
                            sheet.Range("A16", "AA45").Value2 <- array

        let addLC3Loads sds =

            let addLoadSheet() =
                let workSheetNames = [for sheet in bom.Worksheets -> (sheet :?> Worksheet).Name] 
                let indexOfLastLoadSheet, lastLoadSheetNumber =
                    let lastLoadSheetNumber = workSheetNames
                                              |> List.filter (fun sheet -> sheet.Contains("L ("))
                                              |> List.map (fun sheet -> System.Int32.Parse(sheet.Split([|"(";")"|], StringSplitOptions.RemoveEmptyEntries).[1]))
                                              |> List.max
                    (bom.Worksheets.[sprintf "L (%i)" lastLoadSheetNumber] :?> Worksheet).Index, lastLoadSheetNumber
                                 

                let blankLoadWorksheet = bom.Worksheets.["L_A"] :?> Worksheet
                blankLoadWorksheet.Visible <- Microsoft.Office.Interop.Excel.XlSheetVisibility.xlSheetVisible
                blankLoadWorksheet.Copy(bom.Worksheets.[indexOfLastLoadSheet + 1])
                blankLoadWorksheet.Visible <- Microsoft.Office.Interop.Excel.XlSheetVisibility.xlSheetHidden
                let newLoadSheet = (bom.Worksheets.[indexOfLastLoadSheet + 1]) :?> Worksheet
                newLoadSheet.Name <- "L (" + string(lastLoadSheetNumber + 1) + ")"
                newLoadSheet
                
            
            let joists, girders, loads = bomInfo
          
            let joistsWithLC3Loads = joists |> List.filter (fun joist -> List.isEmpty (joist.LC3Loads loads sds) = false)
            
            let mutable row = 1
            let mutable maxJoistIndex = List.length joistsWithLC3Loads

            let mutable newLoadSheet = addLoadSheet()
            let mutable array = newLoadSheet.Range("A14", "M55").Value2 :?> obj [,]            
            
            let mutable joistIndex = 0  
                   


            while joistIndex < maxJoistIndex do
                let joist = joistsWithLC3Loads.[joistIndex]

                if row + (List.length (joist.LC3Loads loads sds)) >= 42 then
                    newLoadSheet.Range("A14", "M55").Value2 <- array.Clone()
                    newLoadSheet <- addLoadSheet()
                    array <- newLoadSheet.Range("A14", "M55").Value2 :?> obj [,]
                    row <- 1
                    joistIndex <- joistIndex - 1
                else
                    array.[row, 1] <- box ("S" + joist.Mark)
                    for load in (joist.LC3Loads loads sds) do
                        array.[row, 2] <- box (load.Type)
                        array.[row, 3] <- box (load.Category)
                        array.[row, 4] <- load.Position
                        array.[row, 6] <- load.Load1Value
                        array.[row, 7] <- load.Load1DistanceFt
                        array.[row, 8] <- load.Load1DistanceIn
                        array.[row, 9] <- load.Load2Value
                        array.[row, 10] <- load.Load2DistanceFt
                        array.[row, 11] <- load.Load2DistanceIn
                        array.[row, 12] <- load.Ref
                        array.[row, 13] <- box (load.LoadCaseString)
                        row <- row + 1
                if joistIndex = maxJoistIndex - 1 then
                    newLoadSheet.Range("A14", "M55").Value2 <- array
                joistIndex <- joistIndex + 1

            let girdersWithLC3Loads = girders |> List.filter (fun girder -> List.isEmpty (girder.LC3Loads loads sds) = false)
            
            let mutable row = 1

            let mutable maxGirderIndex = List.length girdersWithLC3Loads

            let mutable newLoadSheet = addLoadSheet()

            let mutable array = newLoadSheet.Range("A14", "M55").Value2 :?> obj [,]            
            
            let mutable girderIndex = 0   

            while girderIndex < maxGirderIndex do
                let girder = girdersWithLC3Loads.[girderIndex]

                if row + (List.length (girder.LC3Loads loads sds)) >= 42 then
                    newLoadSheet.Range("A14", "M55").Value2 <- array.Clone()
                    newLoadSheet <- addLoadSheet()
                    array <- newLoadSheet.Range("A14", "M55").Value2 :?> obj [,]
                    row <- 1
                    girderIndex <- girderIndex - 1
                else
                    array.[row, 1] <- box ("S" + girder.Mark)
                    for load in (girder.LC3Loads loads sds) do
                        array.[row, 2] <- box (load.Type)
                        array.[row, 3] <- box (load.Category)
                        array.[row, 4] <- load.Position
                        array.[row, 6] <- load.Load1Value
                        array.[row, 7] <- load.Load1DistanceFt
                        array.[row, 8] <- load.Load1DistanceIn
                        array.[row, 9] <- load.Load2Value
                        array.[row, 10] <- load.Load2DistanceFt
                        array.[row, 11] <- load.Load2DistanceIn
                        array.[row, 12] <- load.Ref
                        array.[row, 13] <- box (load.LoadCaseString)
                        row <- row + 1
                if girderIndex = maxGirderIndex - 1 then
                    newLoadSheet.Range("A14", "M55").Value2 <- array
                girderIndex <- girderIndex + 1

        changeSmLoadsToLC3()
        addLC3LoadsToLoadNotes()
        addLC3Loads sds

    let getAllBomInfo bomPath sds =
        let (joists, girders, loads) = getAllInfo bomPath getInfo modifyWorkbookFunction sds  /// warning is OK since this will always return a list of three itmes
        {
        Joists = joists
        Girders = girders
        Loads = loads
        }
















        





                       



    



    

    


