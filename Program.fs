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

    let identity _ =
      let config = { defaultConfig with bindings = [ HttpBinding.createSimple Protocol.HTTP "127.0.0.1" 8003] }
      let app =
          choose [
              pathStarts "/identityserver" >=> (OwinApp.ofAppFunc "/identityserver" identityServer =>= NOT_FOUND "File not found" )
              NOT_FOUND "No handlers found"
              ]
      let a,startServer = startWebServerAsync config app
      Async.Start startServer
      a

    let userKey = "server.User"

    open System.Security.Claims

    let authenticated = fun (ctx:HttpContext) ->
      async{
        if ctx.userState.ContainsKey userKey then
          let claims = ctx.userState.[userKey] :?> ClaimsPrincipal
          if claims.Identity.IsAuthenticated then
            return Some ctx
          else
            return None
        else
          return None
      }

    [<EntryPoint>]
    let main argv =
        // Launch identity server first
        let listening = identity ()
        // Wait for the server to start listening
        listening |> Async.RunSynchronously |> printfn "Identity server started: %A"
        // The token authentication middleware will contact the identity server on construction
        let securityMiddleware = securityMiddleware ()
        let app =
            choose [
                path "/devices" >=> (OwinApp.ofAppFunc "/devices" securityMiddleware =>= choose [ authenticated >=> OK "Hello authenticated user" ; OK "Hello, this is a restricted area" ])
                path "/test" >=> OK "Hello world, welcome to public recreational area."
                NOT_FOUND "No handlers found"
            ]
        // launch the app
        startWebServer defaultConfig app
        0 
