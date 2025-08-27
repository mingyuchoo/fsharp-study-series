module DataDashboard.MainView

open System
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.FuncUI.Types
open Avalonia.Threading
open DataDashboard.Types
open DataDashboard.ChartComponents
open DataDashboard.DataService

// 상태 초기화
let init () =
    {
        SalesData = []
        Metrics = []
        SelectedChartType = LineChart
        SelectedDateRange = (DateTime.Now.AddDays (-30.0), DateTime.Now)
        IsLoading = false
        LastUpdated = DateTime.Now
        ErrorMessage = None
    },
    Elmish.Cmd.ofMsg LoadData

// UI 스레드로 디스패치 보장하는 데이터 로드 커맨드
let private cmdLoadData : Elmish.Cmd<Msg> =
    Elmish.Cmd.ofEffect (fun dispatch ->
        async {
            let! (sales, metrics) = loadData ()
            Dispatcher.UIThread.Post (fun _ -> dispatch (DataLoaded (sales, metrics)))
        }
        |> Async.StartImmediate
    )

// 상태 업데이트
let update (msg : Msg) (state : DashboardState) =
    match msg with
    | LoadData ->
        {
            state with
                IsLoading = true
                ErrorMessage = None
        },
        cmdLoadData

    | DataLoaded (salesData, metrics) ->
        {
            state with
                SalesData = salesData
                Metrics = metrics
                IsLoading = false
                LastUpdated = DateTime.Now
        },
        Elmish.Cmd.none

    | DataLoadFailed error ->
        {
            state with
                IsLoading = false
                ErrorMessage = Some error
        },
        Elmish.Cmd.none

    | ChangeChartType chartType ->
        {
            state with
                SelectedChartType = chartType
        },
        Elmish.Cmd.none

    | ChangeDateRange dateRange ->
        {
            state with
                SelectedDateRange = dateRange
        },
        Elmish.Cmd.none

    | RefreshData ->
        {
            state with
                IsLoading = true
        },
        cmdLoadData

    | ExportReport ->
        let fileName = generateReport state.SalesData state.Metrics
        state, Elmish.Cmd.ofMsg (ReportExported fileName)

    | ReportExported fileName ->
        // 실제로는 사용자에게 알림 표시
        printfn "Report exported: %s" fileName
        state, Elmish.Cmd.none

// 헤더 컴포넌트
let header state dispatch =
    Border.create [
        Border.background "#343a40"
        Border.padding (16, 12)
        Border.child (
            Grid.create [
                Grid.columnDefinitions "*,auto,auto"
                Grid.children [
                    StackPanel.create [
                        Grid.column 0
                        StackPanel.orientation Orientation.Vertical
                        StackPanel.children [
                            TextBlock.create [
                                TextBlock.text "Data Analytics Dashboard"
                                TextBlock.fontSize 20
                                TextBlock.fontWeight FontWeight.Bold
                                TextBlock.foreground "White"
                            ]
                            TextBlock.create [
                                TextBlock.text (
                                    sprintf "Last updated: %s" (state.LastUpdated.ToString ("yyyy-MM-dd HH:mm:ss"))
                                )
                                TextBlock.fontSize 12
                                TextBlock.foreground "#adb5bd"
                            ]
                        ]
                    ]
                    Button.create [
                        Grid.column 1
                        Button.content "Refresh"
                        Button.padding (12, 8)
                        Button.margin (0, 0, 8, 0)
                        Button.background "#007bff"
                        Button.foreground "White"
                        Button.onClick (fun _ -> dispatch RefreshData)
                        Button.isEnabled (not state.IsLoading)
                    ]
                    Button.create [
                        Grid.column 2
                        Button.content "Export Report"
                        Button.padding (12, 8)
                        Button.background "#28a745"
                        Button.foreground "White"
                        Button.onClick (fun _ -> dispatch ExportReport)
                    ]
                ]
            ]
        )
    ]

// 메트릭 섹션
let metricsSection metrics =
    StackPanel.create [
        StackPanel.orientation Orientation.Vertical
        StackPanel.margin (16, 16, 16, 8)
        StackPanel.children [
            TextBlock.create [
                TextBlock.text "Key Metrics"
                TextBlock.fontSize 18
                TextBlock.fontWeight FontWeight.Bold
                TextBlock.margin (0, 0, 0, 12)
            ]
            StackPanel.create [
                StackPanel.orientation Orientation.Horizontal
                StackPanel.children (metrics |> List.map (fun m -> metricCard m :> IView))
            ]
        ]
    ]

// 차트 섹션
let chartSection state dispatch =
    StackPanel.create [
        StackPanel.orientation Orientation.Vertical
        StackPanel.margin 16
        StackPanel.children [
            TextBlock.create [
                TextBlock.text "Data Visualization"
                TextBlock.fontSize 18
                TextBlock.fontWeight FontWeight.Bold
                TextBlock.margin (0, 0, 0, 12)
            ]
            StackPanel.create [
                StackPanel.orientation Orientation.Horizontal
                StackPanel.margin (0, 0, 0, 16)
                StackPanel.children [
                    chartTypeButton LineChart state.SelectedChartType (ChangeChartType >> dispatch)
                    chartTypeButton BarChart state.SelectedChartType (ChangeChartType >> dispatch)
                    chartTypeButton PieChart state.SelectedChartType (ChangeChartType >> dispatch)
                    chartTypeButton ScatterPlot state.SelectedChartType (ChangeChartType >> dispatch)
                ]
            ]
            chartArea state.SelectedChartType state.SalesData
        ]
    ]

// 로딩 오버레이
let loadingOverlay =
    Border.create [
        Border.background "#80000000"
        Border.child (
            StackPanel.create [
                StackPanel.orientation Orientation.Vertical
                StackPanel.horizontalAlignment HorizontalAlignment.Center
                StackPanel.verticalAlignment VerticalAlignment.Center
                StackPanel.children [
                    TextBlock.create [
                        TextBlock.text "Loading..."
                        TextBlock.fontSize 18
                        TextBlock.foreground "White"
                        TextBlock.horizontalAlignment HorizontalAlignment.Center
                    ]
                ]
            ]
        )
    ]

// 메인 뷰
let view (state : DashboardState) dispatch =
    Grid.create [
        Grid.rowDefinitions "auto,*"
        Grid.children [
            header state dispatch
            ScrollViewer.create [
                Grid.row 1
                ScrollViewer.content (
                    StackPanel.create [
                        StackPanel.orientation Orientation.Vertical
                        StackPanel.children [
                            if not state.Metrics.IsEmpty then
                                metricsSection state.Metrics
                            if not state.SalesData.IsEmpty then
                                chartSection state dispatch
                            match state.ErrorMessage with
                            | Some error ->
                                Border.create [
                                    Border.background "#f8d7da"
                                    Border.padding 16
                                    Border.margin 16
                                    Border.child (
                                        TextBlock.create [
                                            TextBlock.text (sprintf "Error: %s" error)
                                            TextBlock.foreground "#721c24"
                                        ]
                                    )
                                ]
                            | None -> ()
                        ]
                    ]
                )
            ]
            if state.IsLoading then
                loadingOverlay
        ]
    ]
