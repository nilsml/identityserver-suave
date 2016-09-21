namespace IdentityServer

module Demo =
    open System
    open Suave
    open Suave.Successful
    open Suave.RequestErrors
    open Suave.Filters
    open Suave.Operators
    open SecurityMiddleware
    open Suave.Owin
   
    let getLinks =
        "Look at all my links"

    [<EntryPoint>]
    let main argv =
        let config = defaultConfig

        let app =
            choose [
                GET >=> path "/" >=> OwinApp.ofAppFunc "/" securityMiddleware >=> OK getLinks
                NOT_FOUND "Resource not found"
            ]

        startWebServer config app
        0 
