#r "Packages/Deedle/Deedle.dll"

open Deedle

let scriptLocation = __SOURCE_DIRECTORY__
let dataPath = scriptLocation + @"\Data"

let startDate = System.DateTime(2016, 1,1)
let endDate = System.DateTime(2016,12,31)

let filterRows (dataFrame : Frame<'a, 'b>) colIndex predicate =
    let column = dataFrame.GetColumn(colIndex)
    let trueRowKeys = column.Where(fun value -> predicate value.Value = true).Keys
    let filteredDataFrame = dataFrame.Rows.[trueRowKeys]
    filteredDataFrame

let mergeFramesOn (frame1 : Frame<'a, 'b>) (frame2 : Frame<'a, 'b>) frame1ColIndex frame2ColIndex =
    let frame1Column = frame1.GetColumn(frame1ColIndex)
    let frame2Column = frame2.GetColumn(frame2ColIndex)
    let trueFrame1Rows = frame1Column.Where(fun value -> frame2Column.Values |> Seq.contains value.Value).Keys
    let frame2Cols = frame2.Columns.Keys
    for key in frame2.Columns.Keys do
        if key != frame2ColIndex then
            let column = frame2.GetColumn(key)
            for i = 0 to i = frame2.RowCount do
                

        
        


let joistSold =
    let joistSold = Deedle.Frame.ReadCsv(dataPath + @"\Fallon\Jobs Sold.csv")
    joistSold.ReplaceColumn("J. PO Date", joistSold.GetColumn<System.DateTime>("J. PO Date"))
    let isInDateRange value = value >= startDate && value <= endDate
    let joistSold = filterRows joistSold "J. PO Date" isInDateRange
    joistSold

let secondColumn = joistSold.Columns.["This"]


let joistSoldPODateFilt = joistSold.Columns.[["J. PO Date"; "Job Number"]]

let quotedVsSold = Deedle.Frame.ReadCsv(dataPath + @"\Fallon\Quoted VS Sold.csv")

