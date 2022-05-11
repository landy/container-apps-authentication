open System
open Microsoft.AspNetCore.Builder
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Identity.Web

let authHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let info =
            {|
                path = ctx.Request.Path.Value
                isAuthenticated = ctx.User.Identity.IsAuthenticated
                claims = ctx.User.Claims
                time = DateTime.UtcNow.ToString()
                headers = ctx.Request.Headers
            |}
        json info next ctx

let routes = choose [
    route "/" >=> text "hello world"
    route "/auth" >=> authHandler
]

let builder = WebApplication.CreateBuilder()
builder.Services.AddGiraffe() |> ignore
builder.Services.AddAuthorization() |> ignore
builder.Services.AddAuthentication().AddAppServicesAuthentication() |> ignore

let app = builder.Build()
app.UseGiraffe routes
app.UseAuthentication() |> ignore
app.UseAuthorization() |> ignore

app.Run()
