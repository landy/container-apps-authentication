open Microsoft.AspNetCore.Builder
open Giraffe
open Microsoft.AspNetCore.Http

let authHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        json ctx.User.Claims next ctx

let routes = choose [
    route "/" >=> text "hello world"
    route "/auth" >=> authHandler
]

let builder = WebApplication.CreateBuilder()
builder.Services.AddGiraffe() |> ignore

let app = builder.Build()
app.UseGiraffe routes

app.Run()
