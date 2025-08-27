module DataDashboard.Types

open System

// 데이터 모델
type SalesData = {
    Date : DateTime
    Product : string
    Revenue : decimal
    Quantity : int
    Region : string
}

type MetricData = {
    Name : string
    Value : float
    Change : float
    Unit : string
}

// 차트 타입
type ChartType =
    | LineChart
    | BarChart
    | PieChart
    | ScatterPlot

// 애플리케이션 상태
type DashboardState = {
    SalesData : SalesData list
    Metrics : MetricData list
    SelectedChartType : ChartType
    SelectedDateRange : DateTime * DateTime
    IsLoading : bool
    LastUpdated : DateTime
    ErrorMessage : string option
}

// 메시지 (MVU 패턴)
type Msg =
    | LoadData
    | DataLoaded of SalesData list * MetricData list
    | DataLoadFailed of string
    | ChangeChartType of ChartType
    | ChangeDateRange of (DateTime * DateTime)
    | RefreshData
    | ExportReport
    | ReportExported of string
