# MyDataDashBoard

F#과 Avalonia FuncUI를 사용한 데이터 분석 대시보드 애플리케이션입니다.

## 📋 개요

이 프로젝트는 F# 함수형 프로그래밍과 Avalonia UI 프레임워크를 활용하여 구축된 데이터 시각화 대시보드입니다. MVU(Model-View-Update) 패턴을 사용하여 반응형 사용자 인터페이스를 제공합니다.

## 🚀 주요 기능

- **실시간 데이터 시각화**: 라인 차트, 바 차트, 파이 차트, 산점도 지원
- **메트릭 대시보드**: 총 매출, 주문 수, 평균 주문 가치 등 핵심 지표 표시
- **데이터 소스**: CSV 파일 로드 또는 랜덤 데이터 생성
- **리포트 생성**: 분석 결과를 텍스트 파일로 내보내기
- **반응형 UI**: Avalonia FuncUI를 통한 크로스 플랫폼 데스크톱 애플리케이션

## 🛠️ 기술 스택

- **언어**: F# (.NET 9.0)
- **UI 프레임워크**: Avalonia 11.3.4 + FuncUI 1.5.1
- **아키텍처**: MVU (Model-View-Update) 패턴
- **차트 라이브러리**: Plotly.NET 5.1.0
- **데이터 처리**: FSharp.Data 6.6.0
- **상태 관리**: Elmish 4.0.2

## 📁 프로젝트 구조

```text
MyDataDashBoard/
├── Types.fs              # 데이터 모델 및 메시지 타입 정의
├── DataService.fs         # 데이터 로드 및 처리 로직
├── ChartComponents.fs     # 차트 및 UI 컴포넌트
├── MainView.fs           # 메인 뷰 및 MVU 패턴 구현
├── Program.fs            # 애플리케이션 진입점
└── MyDataDashBoard.fsproj # 프로젝트 파일
```

## 🏗️ 아키텍처

### 데이터 모델 (`Types.fs`)

- `SalesData`: 판매 데이터 구조체
- `MetricData`: 메트릭 정보 구조체
- `DashboardState`: 애플리케이션 전체 상태
- `Msg`: MVU 패턴의 메시지 타입

### 데이터 서비스 (`DataService.fs`)

- CSV 파일 로드 기능
- 랜덤 데이터 생성
- 메트릭 계산 로직
- 리포트 생성 기능

### UI 컴포넌트 (`ChartComponents.fs`)

- 메트릭 카드 컴포넌트
- 차트 타입 선택 버튼
- 차트 영역 렌더링

### 메인 뷰 (`MainView.fs`)

- MVU 패턴 구현 (init, update, view)
- 비동기 데이터 로딩
- UI 스레드 안전성 보장

## 🚀 실행 방법

### 사전 요구사항

- .NET 9.0 SDK
- F# 컴파일러

### 실행 명령

```bash
# 의존성 복원
dotnet restore

# 애플리케이션 실행
dotnet run

# 코드 포맷팅
fantomas .
```

## 📊 지원하는 차트 타입

1. **라인 차트**: 시간에 따른 매출 추이
2. **바 차트**: 제품별 매출 비교
3. **파이 차트**: 지역별 매출 분포
4. **산점도**: 수량과 매출의 상관관계

## 📈 메트릭

대시보드에서 제공하는 주요 메트릭:

- 총 매출 (Total Revenue)
- 총 주문 수 (Total Orders)
- 평균 주문 가치 (Average Order Value)
- 총 판매 수량 (Total Quantity)

## 📄 데이터 형식

CSV 파일을 사용할 경우 다음 형식을 따라야 합니다:

```csv
Date,Product,Revenue,Quantity,Region
2024-01-01,Product A,1500.00,10,North
2024-01-01,Product B,2300.50,15,South
```

## 🔧 개발 환경 설정

### 코드 포맷팅

프로젝트는 Fantomas를 사용하여 F# 코드 포맷팅을 관리합니다.

```bash
dotnet fantomas .
```

### 빌드

```bash
dotnet build
```

## 📝 라이선스

이 프로젝트는 학습 목적으로 제작되었습니다.

## 🤝 기여

F# 학습 시리즈의 일부로, 함수형 프로그래밍과 GUI 개발의 결합을 보여주는 예제입니다.
