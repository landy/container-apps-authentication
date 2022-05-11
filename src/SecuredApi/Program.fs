open System
open System.Security.Claims
open Microsoft.AspNetCore.Builder
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Identity.Web


type AuthResponse = {
    Path : string
    IsAuthenticated: bool
    Claims : string list
    Time : string
    Headers : string list
} 

let authHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let info =
            {
                Path = ctx.Request.Path.Value
                IsAuthenticated = ctx.User.Identity.IsAuthenticated
                Claims = ctx.User.Claims |> List.ofSeq |> List.map (fun c -> $"{c.Type}={c.Value}")
                Time = DateTime.UtcNow.ToString()
                Headers = ctx.Request.Headers |> List.ofSeq |> List.map (fun header -> $"{header.Key}={header.Value}")
            }
        json info next ctx

let routes = choose [
    route "/" >=> text "hello world"
    route "/auth" >=> authHandler
]

let builder = WebApplication.CreateBuilder()
builder.Services.AddAuthorization() |> ignore
builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration.GetSection("AzureAD")) |> ignore
builder.Services.AddAuthentication("EasyAuth") |> ignore// .AddAppServicesAuthentication() |> ignore
builder.Services.AddGiraffe() |> ignore

let app = builder.Build()
app.UseAuthentication() |> ignore
app.UseAuthorization() |> ignore
app.UseGiraffe routes

app.MapGet("/auth2", Func<HttpContext,AuthResponse>(fun (ctx:HttpContext) ->
    {
        Path = ctx.Request.Path.Value
        IsAuthenticated = ctx.User.Identity.IsAuthenticated
        Claims = ctx.User.Claims |> List.ofSeq |> List.map (fun c -> $"{c.Type}={c.Value}")
        Time = DateTime.UtcNow.ToString()
        Headers = ctx.Request.Headers |> List.ofSeq |> List.map (fun header -> $"{header.Key}={header.Value}")
    }
    )) |> ignore

app.Run()
