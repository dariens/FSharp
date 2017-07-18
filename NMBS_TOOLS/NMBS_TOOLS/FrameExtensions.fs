    namespace NMBS_Tools.FrameExtensions
    
    open Deedle
    
    module Frame =

        let colCalcedFromCol colKey func newColKey df =
            let colValues = (df |> Frame.getCol colKey).Values 
            let newColValues = colValues |> Seq.map func 
            df |> Frame.addCol newColKey (newColValues |> Series.ofValues)

        let whereRowValuesInColMeetReq colIndex req frame =
            Frame.filterRowValues (fun (objSeries : ObjectSeries<'a>) ->
                    (req (objSeries.Get(colIndex)))) frame

        let whereRowValuesInColEqual colIndex acceptableValues frame =
            frame
            |> whereRowValuesInColMeetReq colIndex (fun obj -> Seq.contains obj acceptableValues)
            //Frame.filterRowValues (fun (objSeries : ObjectSeries<'a>) -> 
            //        (Seq.contains (objSeries.Get(colIndex)) acceptableValues)) frame

        let whereRowValuesInColDontEqual colIndex unacceptableValues frame =
            frame
            |> whereRowValuesInColMeetReq colIndex (fun obj -> (Seq.contains obj unacceptableValues) = false)

        let whereRowValuesInColAreInDateRange colIndex startDate endDate frame =
            frame
            |> whereRowValuesInColMeetReq colIndex (fun date ->
                                                        let date = System.DateTime.Parse(string date)
                                                        date >= startDate && date <= endDate)

        let removeDuplicateRows index (frame : Frame<'a, 'b>) =
          let nonDupKeys = frame.GroupRowsBy(index).RowKeys
                           |> Seq.distinctBy (fun (a, b) -> a)
                           |> Seq.map (fun (a, b) -> b)
          frame.Rows.[nonDupKeys]
        
        
        let merge_On (infoFrame : Frame<'c, 'b>) column missingReplacement (initialFrame : Frame<'a,'b>) =
              let frame = initialFrame.Clone()
              let infoFrame = infoFrame.Clone()
                               |> removeDuplicateRows column 
                               |> Frame.indexRowsString column
              let initialSeries = frame.GetColumn(column)
              let infoFrameRows = infoFrame.RowKeys
              for colKey in infoFrame.ColumnKeys do
                  let newSeries =
                      [for v in initialSeries.ValuesAll do
                            if Seq.contains v infoFrameRows then  
                                let key = infoFrame.GetRow(v)
                                yield key.[colKey]
                            else
                                yield box missingReplacement ]
                  frame.AddColumn(colKey, newSeries)
              frame

        let merge_On2 (infoFrame : Frame<'c, 'b>) column missingReplacement (initialFrame : Frame<'a,'b>) =
              let infoFrame = infoFrame
                              |> removeDuplicateRows column
                              |> Frame.indexRows column

              let infoMatched =
                  initialFrame.Rows
                  |> Series.map (fun k row ->
                      infoFrame.Rows.TryGet(row.GetAs(column)).ValueOrDefault)
                  |> Series.fillMissingWith missingReplacement
                  |> Frame.ofRows
              
              initialFrame.Join(infoMatched) |> ignore
              initialFrame

        

        let sumColumnsBy colIndex (frame: Frame<'a, 'b>) =
                let distinctValues = frame.GetColumn(colIndex).Values |> Seq.distinct
                let mutable row = -1
                let seriesList =
                  [for distinctValue in distinctValues do
                    row <- row + 1
                    let sumOfDistinctValue = frame
                                             |> whereRowValuesInColEqual colIndex [distinctValue]
                                             |> Frame.dropCol colIndex
                                             |> Stats.sum
                                             
                    for key in sumOfDistinctValue.Keys do                   
                         yield (box row, key, sumOfDistinctValue.[key])]
                let newFrame = seriesList |> Frame.ofValues
                newFrame.AddColumn(colIndex, distinctValues)
                newFrame
                


        let sumSpecificColumnsBy byColumn (wantedColumns) (frame: Frame<'a, 'b>) =
            let allColumns = byColumn :: wantedColumns
            let newFrame = frame.Columns.[allColumns]
            let summedFrame = newFrame |> sumColumnsBy byColumn
            summedFrame

        let getSpecificColumns (columns: 'b list) (frame: Frame<'a, 'b>) =
            frame.Columns.[columns]

        let renameColumn initialName finalName (frame: Frame<'a, 'b>) =
            frame.RenameColumn(initialName, finalName)
            frame

        let stack (stackFrame: Frame<'a, 'b>) (initialFrame: Frame<'a, 'b>) =
            let initialFrame = initialFrame |> Frame.dropSparseRows
            let stackFrame = stackFrame |> Frame.dropSparseRows
            let cols = initialFrame.Columns.Keys
            let allColumns =
                [for col in cols do
                    let initialValues = initialFrame.GetColumn(col).ValuesAll             
                    let newValues = stackFrame.GetColumn(col).ValuesAll      
                    let allValues = Seq.append initialValues newValues
                    yield (col, Series.ofValues allValues)]
            Frame.ofColumns allColumns

        let replaceVaueInColumnWith column oldValue newValue (frame : Frame<_,_>) =
            let newColumn = frame.GetColumn<obj>(column)
                            |> Series.mapValues (fun v -> match v with
                                                          | oldValue -> box newValue )
            frame.ReplaceColumn(column, newColumn)
            frame

        let removeCols columns (frame : Frame<_,_>) =
            let allColumns = frame.Columns.Keys
            let wantedColumns =
                [for col in allColumns do
                     if not (Seq.contains col columns) then
                         yield col]
            frame.Columns.[wantedColumns]