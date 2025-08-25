open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open System.Text.Json
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.OpenApi
open Microsoft.OpenApi.Models
open Microsoft.Data.Sqlite
open System

///////////////////////////////////////////////////////////////////////////////
// 도메인 모델
///////////////////////////////////////////////////////////////////////////////
type Product = {
    Id : int
    Name : string
    Price : decimal
    Category : string
}

type CreateProductRequest = {
    Name : string
    Price : decimal
    Category : string
}
///////////////////////////////////////////////////////////////////////////////
// 데이터베이스 (SQLite)
///////////////////////////////////////////////////////////////////////////////
module Db =
    let mutable connStr : string = ""

    let getConnection() =
        let conn = new SqliteConnection(connStr)
        conn.Open()
        conn

    let init() =
        use conn = getConnection()
        use cmd = conn.CreateCommand()

        cmd.CommandText <-
            """
            CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Price REAL NOT NULL,
                Category TEXT NOT NULL
            );
            """

        cmd.ExecuteNonQuery() |> ignore

        // 시드 데이터: 없을 때만 삽입
        use checkCmd = conn.CreateCommand()
        checkCmd.CommandText <- "SELECT COUNT(1) FROM Products;"
        let count = checkCmd.ExecuteScalar() :?> int64

        match count with
        | 0L ->
            let seed(name : string, price : decimal, category : string) =
                use c = conn.CreateCommand()
                c.CommandText <- "INSERT INTO Products (Name, Price, Category) VALUES ($n, $p, $c);"
                c.Parameters.AddWithValue("$n", name) |> ignore
                c.Parameters.AddWithValue("$p", float price) |> ignore
                c.Parameters.AddWithValue("$c", category) |> ignore
                c.ExecuteNonQuery() |> ignore

            [
                ("노트북", 1200000m, "전자제품")
                ("마우스", 50000m, "전자제품")
                ("키보드", 120000m, "전자제품")
            ]
            |> List.iter seed
        | _ -> ()
///////////////////////////////////////////////////////////////////////////////
// 비즈니스 로직
///////////////////////////////////////////////////////////////////////////////
module ProductService =
    let getAllProducts() =
        use conn = Db.getConnection()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT Id, Name, Price, Category FROM Products ORDER BY Id;"
        use reader = cmd.ExecuteReader()

        let rec loop acc =
            match reader.Read() with
            | true ->
                let p = {
                    Id = reader.GetInt32(0)
                    Name = reader.GetString(1)
                    Price = decimal(reader.GetDouble(2))
                    Category = reader.GetString(3)
                }

                loop(p :: acc)
            | false -> List.rev acc

        loop []

    let getProductById id =
        use conn = Db.getConnection()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT Id, Name, Price, Category FROM Products WHERE Id = $id;"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        use reader = cmd.ExecuteReader()

        match reader.Read() with
        | true ->
            Some {
                Id = reader.GetInt32(0)
                Name = reader.GetString(1)
                Price = decimal(reader.GetDouble(2))
                Category = reader.GetString(3)
            }
        | false -> None

    let createProduct(request : CreateProductRequest) =
        use conn = Db.getConnection()
        use tx = conn.BeginTransaction()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "INSERT INTO Products (Name, Price, Category) VALUES ($n, $p, $c);"
        cmd.Parameters.AddWithValue("$n", request.Name) |> ignore
        cmd.Parameters.AddWithValue("$p", float request.Price) |> ignore
        cmd.Parameters.AddWithValue("$c", request.Category) |> ignore
        cmd.ExecuteNonQuery() |> ignore

        use idCmd = conn.CreateCommand()
        idCmd.CommandText <- "SELECT last_insert_rowid();"
        let newId = idCmd.ExecuteScalar() :?> int64 |> int
        tx.Commit()

        {
            Id = newId
            Name = request.Name
            Price = request.Price
            Category = request.Category
        }

    let updateProduct id (request : CreateProductRequest) =
        use conn = Db.getConnection()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "UPDATE Products SET Name = $n, Price = $p, Category = $c WHERE Id = $id;"
        cmd.Parameters.AddWithValue("$n", request.Name) |> ignore
        cmd.Parameters.AddWithValue("$p", float request.Price) |> ignore
        cmd.Parameters.AddWithValue("$c", request.Category) |> ignore
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        let changed = cmd.ExecuteNonQuery()

        match changed with
        | x when x > 0 ->
            Some {
                Id = id
                Name = request.Name
                Price = request.Price
                Category = request.Category
            }
        | _ -> None

    let deleteProduct id =
        use conn = Db.getConnection()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "DELETE FROM Products WHERE Id = $id;"
        cmd.Parameters.AddWithValue("$id", id) |> ignore
        let changed = cmd.ExecuteNonQuery()
        changed > 0

///////////////////////////////////////////////////////////////////////////////
// API 엔드포인트 정의
///////////////////////////////////////////////////////////////////////////////
module ApiEndpoints =
    open Microsoft.AspNetCore.Mvc

    // GET /api/products
    let getProducts() : IResult =
        let allProducts = ProductService.getAllProducts()
        Results.Ok(allProducts)

    // GET /api/products/{id}
    let getProduct(id : int) : IResult =
        match ProductService.getProductById id with
        | Some product -> Results.Ok(product)
        | None -> Results.NotFound($"제품 ID {id}를 찾을 수 없습니다.")

    // POST /api/products
    let createProduct(request : CreateProductRequest) : IResult =
        match System.String.IsNullOrWhiteSpace request.Name, request.Price <= 0m with
        | true, _ -> Results.BadRequest("제품명은 필수입니다.")
        | _, true -> Results.BadRequest("가격은 0보다 커야 합니다.")
        | _ ->
            let newProduct = ProductService.createProduct request
            Results.Created($"/api/products/{newProduct.Id}", newProduct)

    // PUT /api/products/{id}
    let updateProduct (id : int) (request : CreateProductRequest) : IResult =
        match System.String.IsNullOrWhiteSpace request.Name, request.Price <= 0m with
        | true, _ -> Results.BadRequest("제품명은 필수입니다.")
        | _, true -> Results.BadRequest("가격은 0보다 커야 합니다.")
        | _ ->
            match ProductService.updateProduct id request with
            | Some updatedProduct -> Results.Ok(updatedProduct)
            | None -> Results.NotFound($"제품 ID {id}를 찾을 수 없습니다.")

    // DELETE /api/products/{id}
    let deleteProduct(id : int) : IResult =
        match ProductService.deleteProduct id with
        | true -> Results.NoContent()
        | false -> Results.NotFound($"제품 ID {id}를 찾을 수 없습니다.")

///////////////////////////////////////////////////////////////////////////////
// 애플리케이션 설정
///////////////////////////////////////////////////////////////////////////////
let configureServices(services : IServiceCollection) =
    services.AddControllers() |> ignore
    services.AddEndpointsApiExplorer() |> ignore

    services.AddSwaggerGen(fun c ->
        let info = OpenApiInfo()
        info.Title <- "MyFSharpWebApp API"
        info.Version <- "v1"
        info.Description <- "제품(Products) 관리용 Minimal API"
        c.SwaggerDoc("v1", info)
    )
    |> ignore

let configureApp(app : WebApplication) =
    match app.Environment.IsDevelopment() with
    | true ->
        app.UseSwagger() |> ignore
        app.UseSwaggerUI() |> ignore
    | false -> ()

    app.UseHttpsRedirection() |> ignore

    // API 라우트 설정
    app
        .MapGet("/api/products", System.Func<IResult>(ApiEndpoints.getProducts))
        .WithTags("Products")
        .WithOpenApi(fun o ->
            o.Summary <- "제품 목록 조회"
            o.Description <- "등록된 모든 제품을 반환합니다."
            o
        )
    |> ignore

    app
        .MapGet("/api/products/{id:int}", System.Func<int, IResult>(ApiEndpoints.getProduct))
        .WithTags("Products")
        .WithOpenApi(fun o ->
            o.Summary <- "제품 단건 조회"
            o.Description <- "지정한 ID의 제품 정보를 반환합니다. 존재하지 않으면 404를 반환합니다."
            o
        )
    |> ignore

    app
        .MapPost("/api/products", System.Func<CreateProductRequest, IResult>(ApiEndpoints.createProduct))
        .WithTags("Products")
        .WithOpenApi(fun o ->
            o.Summary <- "제품 생성"
            o.Description <- "이름/가격/카테고리를 받아 신규 제품을 생성하고 201 Created와 함께 반환합니다."
            o
        )
    |> ignore

    app
        .MapPut(
            "/api/products/{id:int}",
            System.Func<int, CreateProductRequest, IResult>(fun id req -> ApiEndpoints.updateProduct id req)
        )
        .WithTags("Products")
        .WithOpenApi(fun o ->
            o.Summary <- "제품 수정"
            o.Description <- "지정한 ID의 제품 정보를 요청 본문 값으로 갱신합니다. 존재하지 않으면 404를 반환합니다."
            o
        )
    |> ignore

    app
        .MapDelete("/api/products/{id:int}", System.Func<int, IResult>(ApiEndpoints.deleteProduct))
        .WithTags("Products")
        .WithOpenApi(fun o ->
            o.Summary <- "제품 삭제"
            o.Description <- "지정한 ID의 제품을 삭제합니다. 성공 시 204 No Content를 반환합니다."
            o
        )
    |> ignore

    ()

///////////////////////////////////////////////////////////////////////////////
// 메인 함수
///////////////////////////////////////////////////////////////////////////////
[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    configureServices builder.Services

    // SQLite 연결 문자열 설정 및 DB 초기화
    let cs = builder.Configuration.GetConnectionString("Default")

    Db.connStr <-
        match isNull cs with
        | true -> "Data Source=app.db"
        | false -> cs

    let app = builder.Build()
    Db.init()
    configureApp app

    app.Run()
    0
