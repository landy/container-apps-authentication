open Microsoft.AspNetCore.Builder
open Giraffe

let routes = choose [
    route "/" >=> text "hello world"
]

let builder = WebApplication.CreateBuilder()
builder.Services.AddGiraffe() |> ignore

let app = builder.Build()
app.UseGiraffe routes

app.Run()
