module DataDashboard.DataService

open System
open System.IO
open FSharp.Data
open DataDashboard.Types

// CSV 데이터 타입 정의
type SalesDataCsv =
    CsvProvider<"Date,Product,Revenue,Quantity,Region
2024-01-01,Product A,1500.00,10,North
2024-01-01,Product B,2300.50,15,South">

// 실시간 데이터 시뮬레이션
let generateRandomSalesData count =
    let random = Random ()
    let products = [| "Product A" ; "Product B" ; "Product C" ; "Product D" ; "Product E" |]
    let regions = [| "North" ; "South" ; "East" ; "West" ; "Central" |]

    [ 1..count ]
    |> List.map (fun i -> {
        Date = DateTime.Now.AddDays (float (-random.Next(30)))
        Product = products.[random.Next (products.Length)]
        Revenue = decimal (random.Next (1000, 5000)) + decimal (random.NextDouble ())
        Quantity = random.Next (1, 50)
        Region = regions.[random.Next (regions.Length)]
    })

// 메트릭 계산
let calculateMetrics (salesData : SalesData list) =
    if salesData.IsEmpty then
        []
    else
        let totalRevenue = salesData |> List.sumBy (fun x -> float x.Revenue)
        let totalQuantity = salesData |> List.sumBy (fun x -> float x.Quantity)
        let avgOrderValue = totalRevenue / float salesData.Length

        let topProduct =
            salesData
            |> List.groupBy (fun x -> x.Product)
            |> List.map (fun (product, sales) -> product, sales |> List.sumBy (fun s -> float s.Revenue))
            |> List.maxBy snd
            |> fst

        [
            {
                Name = "Total Revenue"
                Value = totalRevenue
                Change = 15.3
                Unit = "$"
            }
            {
                Name = "Total Orders"
                Value = float salesData.Length
                Change = -2.1
                Unit = ""
            }
            {
                Name = "Avg Order Value"
                Value = avgOrderValue
                Change = 8.7
                Unit = "$"
            }
            {
                Name = "Total Quantity"
                Value = totalQuantity
                Change = 12.4
                Unit = "units"
            }
        ]

// CSV에서 데이터 로드
let loadDataFromCsv filePath =
    try
        if File.Exists filePath then
            let csvData = SalesDataCsv.Load (filePath)

            let salesData =
                csvData.Rows
                |> Seq.map (fun row -> {
                    Date = row.Date
                    Product = row.Product
                    Revenue = row.Revenue
                    Quantity = row.Quantity
                    Region = row.Region
                })
                |> List.ofSeq

            Some salesData
        else
            None
    with ex ->
        printfn "Error loading CSV: %s" ex.Message
        None

// 데이터 로드 (실제로는 API 호출 등)
let loadData () =
    async {
        // 실제 환경에서는 API 호출, 데이터베이스 쿼리 등
        do! Async.Sleep (1000) // 로딩 시뮬레이션

        // CSV 파일이 있으면 로드, 없으면 랜덤 데이터 생성
        let salesData =
            match loadDataFromCsv "sales_data.csv" with
            | Some data -> data
            | None -> generateRandomSalesData 100

        let metrics = calculateMetrics salesData
        return (salesData, metrics)
    }

// 리포트 생성
let generateReport (salesData : SalesData list) (metrics : MetricData list) =
    let reportContent =
        let metricsSection =
            metrics
            |> List.map (fun m -> sprintf "%s: %.2f%s (%.1f%%)" m.Name m.Value m.Unit m.Change)
            |> String.concat "\n"

        let salesSummary =
            salesData
            |> List.groupBy (fun x -> x.Product)
            |> List.map (fun (product, sales) ->
                let totalRevenue = sales |> List.sumBy (fun s -> float s.Revenue)
                sprintf "%s: $%.2f" product totalRevenue
            )
            |> String.concat "\n"

        sprintf
            """
Data Analysis Report
Generated: %s

Key Metrics:
%s

Sales by Product:
%s

Total Records: %d
        """
            (DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss"))
            metricsSection
            salesSummary
            salesData.Length

    let fileName = sprintf "report_%s.txt" (DateTime.Now.ToString ("yyyyMMdd_HHmmss"))
    File.WriteAllText (fileName, reportContent)
    fileName
