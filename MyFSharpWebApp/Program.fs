open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open System.Text.Json
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.OpenApi
open Microsoft.OpenApi.Models

///////////////////////////////////////////////////////////////////////////////
// 도메인 모델
///////////////////////////////////////////////////////////////////////////////
type Product = {
    Id: int
    Name: string
    Price: decimal
    Category: string
}

type CreateProductRequest = {
    Name: string
    Price: decimal
    Category: string
}
///////////////////////////////////////////////////////////////////////////////
// 인메모리 데이터 저장소
///////////////////////////////////////////////////////////////////////////////
let mutable products = [
    { Id = 1; Name = "노트북"; Price = 1200000m; Category = "전자제품" }
    { Id = 2; Name = "마우스"; Price = 50000m; Category = "전자제품" }
    { Id = 3; Name = "키보드"; Price = 120000m; Category = "전자제품" }
]

let mutable nextId = 4
///////////////////////////////////////////////////////////////////////////////
// 비즈니스 로직
///////////////////////////////////////////////////////////////////////////////
module ProductService =
    let getAllProducts () = products
    
    let getProductById id = 
        products |> List.tryFind (fun p -> p.Id = id)
    
    let createProduct (request: CreateProductRequest) =
        let newProduct = {
            Id = nextId
            Name = request.Name
            Price = request.Price
            Category = request.Category
        }
        nextId <- nextId + 1
        products <- newProduct :: products
        newProduct
    
    let updateProduct id (request: CreateProductRequest) =
        match products |> List.tryFind (fun p -> p.Id = id) with
        | Some _ ->
            let updatedProduct = {
                Id = id
                Name = request.Name
                Price = request.Price
                Category = request.Category
            }
            products <- products |> List.map (fun p -> if p.Id = id then updatedProduct else p)
            Some updatedProduct
        | None -> None
    
    let deleteProduct id =
        match products |> List.tryFind (fun p -> p.Id = id) with
        | Some product ->
            products <- products |> List.filter (fun p -> p.Id <> id)
            true
        | None -> false

///////////////////////////////////////////////////////////////////////////////
// API 엔드포인트 정의
///////////////////////////////////////////////////////////////////////////////
module ApiEndpoints =
    open Microsoft.AspNetCore.Mvc
    
    // GET /api/products
    let getProducts () : IResult =
        let allProducts = ProductService.getAllProducts()
        Results.Ok(allProducts)
    
    // GET /api/products/{id}
    let getProduct (id: int) : IResult =
        match ProductService.getProductById id with
        | Some product -> Results.Ok(product)
        | None -> Results.NotFound($"제품 ID {id}를 찾을 수 없습니다.")
    
    // POST /api/products
    let createProduct (request: CreateProductRequest) : IResult =
        if System.String.IsNullOrWhiteSpace(request.Name) then
            Results.BadRequest("제품명은 필수입니다.")
        elif request.Price <= 0m then
            Results.BadRequest("가격은 0보다 커야 합니다.")
        else
            let newProduct = ProductService.createProduct request
            Results.Created($"/api/products/{newProduct.Id}", newProduct)
    
    // PUT /api/products/{id}
    let updateProduct (id: int) (request: CreateProductRequest) : IResult =
        if System.String.IsNullOrWhiteSpace(request.Name) then
            Results.BadRequest("제품명은 필수입니다.")
        elif request.Price <= 0m then
            Results.BadRequest("가격은 0보다 커야 합니다.")
        else
            match ProductService.updateProduct id request with
            | Some updatedProduct -> Results.Ok(updatedProduct)
            | None -> Results.NotFound($"제품 ID {id}를 찾을 수 없습니다.")
    
    // DELETE /api/products/{id}
    let deleteProduct (id: int) : IResult =
        if ProductService.deleteProduct id then
            Results.NoContent()
        else
            Results.NotFound($"제품 ID {id}를 찾을 수 없습니다.")

///////////////////////////////////////////////////////////////////////////////
// 애플리케이션 설정
///////////////////////////////////////////////////////////////////////////////
let configureServices (services: IServiceCollection) =
    services.AddControllers() |> ignore
    services.AddEndpointsApiExplorer() |> ignore
    services.AddSwaggerGen(fun c ->
        let info = OpenApiInfo()
        info.Title <- "MyFSharpWebApp API"
        info.Version <- "v1"
        info.Description <- "제품(Products) 관리용 Minimal API"
        c.SwaggerDoc("v1", info)
    ) |> ignore

let configureApp (app: WebApplication) =
    if app.Environment.IsDevelopment() then
        app.UseSwagger() |> ignore
        app.UseSwaggerUI() |> ignore
    
    app.UseHttpsRedirection() |> ignore
    
    // API 라우트 설정
    app
        .MapGet("/api/products", System.Func<IResult>(ApiEndpoints.getProducts))
        .WithTags("Products")
        .WithOpenApi(fun o ->
            o.Summary <- "제품 목록 조회"
            o.Description <- "등록된 모든 제품을 반환합니다."
            o)
        |> ignore

    app
        .MapGet("/api/products/{id:int}", System.Func<int, IResult>(ApiEndpoints.getProduct))
        .WithTags("Products")
        .WithOpenApi(fun o ->
            o.Summary <- "제품 단건 조회"
            o.Description <- "지정한 ID의 제품 정보를 반환합니다. 존재하지 않으면 404를 반환합니다."
            o)
        |> ignore

    app
        .MapPost("/api/products", System.Func<CreateProductRequest, IResult>(ApiEndpoints.createProduct))
        .WithTags("Products")
        .WithOpenApi(fun o ->
            o.Summary <- "제품 생성"
            o.Description <- "이름/가격/카테고리를 받아 신규 제품을 생성하고 201 Created와 함께 반환합니다."
            o)
        |> ignore

    app
        .MapPut("/api/products/{id:int}", System.Func<int, CreateProductRequest, IResult>(ApiEndpoints.updateProduct))
        .WithTags("Products")
        .WithOpenApi(fun o ->
            o.Summary <- "제품 수정"
            o.Description <- "지정한 ID의 제품 정보를 요청 본문 값으로 갱신합니다. 존재하지 않으면 404를 반환합니다."
            o)
        |> ignore

    app
        .MapDelete("/api/products/{id:int}", System.Func<int, IResult>(ApiEndpoints.deleteProduct))
        .WithTags("Products")
        .WithOpenApi(fun o ->
            o.Summary <- "제품 삭제"
            o.Description <- "지정한 ID의 제품을 삭제합니다. 성공 시 204 No Content를 반환합니다."
            o)
        |> ignore
    
    app

///////////////////////////////////////////////////////////////////////////////
// 메인 함수
///////////////////////////////////////////////////////////////////////////////
[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    
    configureServices builder.Services
    
    let app = builder.Build()
    configureApp app
    
    app.Run()
    0