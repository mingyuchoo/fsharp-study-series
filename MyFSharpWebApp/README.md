# MyFSharpWebApp

F#(.NET 9)로 구현한 ASP.NET Core Minimal API 예제입니다. `Products` 도메인에 대한 기본 CRUD를 제공하며, Swagger UI를 통해 문서화되어 있습니다.

## 프로젝트 생성 및 실행 방법

## 1. 프로젝트 생성

```bash
dotnet new web -lang F# -n FSharpWebService
cd FSharpWebService
```

## 2. 패키지 추가

이 저장소에는 필요한 패키지가 이미 포함되어 있습니다(`MyFSharpWebApp.fsproj`). 참고용 버전 정보:

- Microsoft.AspNetCore.OpenApi: 9.0.8
- Swashbuckle.AspNetCore: 9.0.3

새로 구성하는 경우에만 아래 명령을 실행하세요:

```bash
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Swashbuckle.AspNetCore
```

## 3. 실행

```bash
dotnet run
```

## 4. 최적화 빌드 및 실행

```bash
dotnet build -c Release
dotnet run -c Release
```

## 주요 특징

함수형 프로그래밍 스타일: F#의 함수형 특성을 활용하여 불변성과 순수 함수를 중심으로 구현했습니다.

모듈 구조: `ProductService`와 `ApiEndpoints` 모듈로 비즈니스 로직과 API 엔드포인트를 분리했습니다.

타입 안정성: F#의 강력한 타입 시스템을 활용하여 컴파일 타임에 많은 오류를 방지할 수 있습니다.

패턴 매칭: F#의 패턴 매칭을 사용하여 Option 타입을 안전하게 처리합니다.

## 프로젝트 구조

```text
MyFSharpWebApp/
├─ Program.fs                      # 도메인/서비스/API 엔드포인트/앱 구성
├─ MyFSharpWebApp.fsproj           # 대상 프레임워크/패키지 정의(net9.0)
├─ appsettings.json                # 기본 설정
├─ appsettings.Development.json    # 개발 환경 설정
└─ Properties/
   └─ launchSettings.json          # 실행 프로필 및 URL(5050/http, 7069/https)
```

## API 엔드포인트

- `GET /api/products` - 모든 제품 목록 조회
- `GET /api/products/{id}` - 특정 제품 조회  
- `POST /api/products` - 새 제품 생성
- `PUT /api/products/{id}` - 제품 정보 수정
- `DELETE /api/products/{id}` - 제품 삭제

## 테스트 예시

서버 실행 후 다음 URL로 테스트할 수 있습니다(Development, 기본 프로필 기준):

- Swagger UI: `http://localhost:5050/swagger` (또는 `https://localhost:7069/swagger`)
- 제품 목록: `GET http://localhost:5050/api/products`

간단한 cURL 예시:

```bash
# 목록 조회
curl http://localhost:5050/api/products

# 단건 조회
curl http://localhost:5050/api/products/1

# 생성
curl -X POST http://localhost:5050/api/products \
  -H "Content-Type: application/json" \
  -d '{"Name":"모니터","Price":300000,"Category":"전자제품"}'

# 수정
curl -X PUT http://localhost:5050/api/products/1 \
  -H "Content-Type: application/json" \
  -d '{"Name":"노트북(업데이트)","Price":1300000,"Category":"전자제품"}'

# 삭제
curl -X DELETE http://localhost:5050/api/products/1 -i
```

이 예시는 F#과 .NET 9.0을 사용한 기본적인 웹 서비스 구조를 보여줍니다. 실제 프로덕션 환경에서는 데이터베이스 연결, 인증/인가, 로깅, 오류 처리, 배포 파이프라인 등을 추가로 구현해야 합니다.
