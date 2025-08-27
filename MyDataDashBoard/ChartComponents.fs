module DataDashboard.ChartComponents

open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Controls.Shapes
open Plotly.NET
open Plotly.NET.LayoutObjects
open DataDashboard.Types

// 차트를 Base64 이미지로 변환 (단순화된 버전)
let createChartImage (chartType : ChartType) (salesData : SalesData list) (width : int) (height : int) =
    let chart =
        match chartType with
        | LineChart ->
            let dates = salesData |> List.map (fun x -> x.Date)
            let revenues = salesData |> List.map (fun x -> float x.Revenue)

            Chart.Line (dates, revenues)
            |> Chart.withTitle "Revenue Over Time"
            |> Chart.withXAxisStyle "Date"
            |> Chart.withYAxisStyle "Revenue ($)"

        | BarChart ->
            let productRevenue =
                salesData
                |> List.groupBy (fun x -> x.Product)
                |> List.map (fun (product, sales) -> product, sales |> List.sumBy (fun s -> float s.Revenue))

            let products = productRevenue |> List.map fst
            let revenues = productRevenue |> List.map snd
            Chart.Column (products, revenues) |> Chart.withTitle "Revenue by Product"

        | PieChart ->
            let regionRevenue =
                salesData
                |> List.groupBy (fun x -> x.Region)
                |> List.map (fun (region, sales) -> region, sales |> List.sumBy (fun s -> float s.Revenue))

            Chart.Pie (valuesLabels = regionRevenue) |> Chart.withTitle "Revenue by Region"

        | ScatterPlot ->
            let quantities = salesData |> List.map (fun x -> float x.Quantity)
            let revenues = salesData |> List.map (fun x -> float x.Revenue)

            Chart.Scatter (quantities |> Seq.zip revenues, StyleParam.Mode.Markers)
            |> Chart.withTitle "Quantity vs Revenue"
            |> Chart.withXAxisStyle "Quantity"
            |> Chart.withYAxisStyle "Revenue ($)"

    chart |> Chart.withSize (Width = width, Height = height) |> Chart.show // 실제로는 이미지로 변환하여 반환

// 메트릭 카드 컴포넌트
let metricCard (metric : MetricData) =
    Border.create [
        Border.background "#f8f9fa"
        Border.cornerRadius 8
        Border.padding 16
        Border.margin (0, 0, 8, 8)
        Border.child (
            StackPanel.create [
                StackPanel.orientation Orientation.Vertical
                StackPanel.children [
                    TextBlock.create [
                        TextBlock.text metric.Name
                        TextBlock.fontSize 12
                        TextBlock.foreground "#6c757d"
                        TextBlock.margin (0, 0, 0, 4)
                    ]
                    TextBlock.create [
                        TextBlock.text (sprintf "%.2f%s" metric.Value metric.Unit)
                        TextBlock.fontSize 24
                        TextBlock.fontWeight FontWeight.Bold
                        TextBlock.margin (0, 0, 0, 4)
                    ]
                    TextBlock.create [
                        TextBlock.text (sprintf "%.1f%%" metric.Change)
                        TextBlock.fontSize 12
                        TextBlock.foreground (if metric.Change >= 0.0 then "#28a745" else "#dc3545")
                    ]
                ]
            ]
        )
    ]

// 차트 선택 버튼
let chartTypeButton chartType currentType onSelect =
    Button.create [
        Button.content (string chartType)
        Button.padding (12, 8)
        Button.margin (0, 0, 8, 0)
        Button.background (if chartType = currentType then "#007bff" else "#e9ecef")
        Button.foreground (if chartType = currentType then "White" else "Black")
        Button.onClick (fun _ -> onSelect chartType)
    ]

// 메인 차트 영역 (실제로는 WebView나 이미지 컨트롤 사용)
let chartArea (chartType : ChartType) (salesData : SalesData list) =
    Border.create [
        Border.background "White"
        Border.cornerRadius 8
        Border.padding 16
        Border.child (
            StackPanel.create [
                StackPanel.orientation Orientation.Vertical
                StackPanel.children [
                    TextBlock.create [
                        TextBlock.text (sprintf "%A Chart" chartType)
                        TextBlock.fontSize 18
                        TextBlock.fontWeight FontWeight.Bold
                        TextBlock.margin (0, 0, 0, 16)
                    ]
                    // 실제로는 차트 이미지나 WebView
                    Border.create [ Border.background "#f8f9fa" ; Border.width 600 ; Border.height 400 ]
                    TextBlock.create [
                        TextBlock.text (sprintf "Data points: %d" salesData.Length)
                        TextBlock.fontSize 12
                        TextBlock.foreground "#6c757d"
                        TextBlock.margin (0, 8, 0, 0)
                        TextBlock.horizontalAlignment HorizontalAlignment.Center
                    ]
                ]
            ]
        )
    ]
