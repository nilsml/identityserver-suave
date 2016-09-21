namespace IdentityServer

module SecurityMiddleware =
    open Owin
    open Microsoft.Owin.Logging
    open Microsoft.Owin.Builder
    open Microsoft.Owin.Security.Jwt
    open IdentityServer3.AccessTokenValidation
    open Suave
    open System
    open System.Diagnostics
    open FSharp.Configuration


    type CustomLogger () =
        interface ILogger with
            member this.WriteCore(eventType:TraceEventType, eventId:int, state:Object, ex:Exception, formatter:Func<obj, exn, string>) =
                printf "Hello logging"
                true
        

    type MyCustomLoggerFactory () =
        interface ILoggerFactory with
            member this.Create (name : string) =
                new CustomLogger() :> ILogger

    type Settings = AppSettings<"app.config">

    let securityMiddleware = 

        let app = new AppBuilder()

        let authenticationOptions = new IdentityServerBearerTokenAuthenticationOptions 
                                            (
                                                Authority = Settings.ApiAuthorizationIdentityServer.AbsoluteUri,
                                                RequiredScopes = ["indentityServerDemo"], 
                                                EnableValidationResultCache = true,
                                                ValidationMode = ValidationMode.ValidationEndpoint,
                                                ValidationResultCacheDuration = TimeSpan.FromMinutes(5.0)                                                
                                            )

        app.SetLoggerFactory(new MyCustomLoggerFactory())

        let builder = app.UseIdentityServerBearerTokenAuthentication(authenticationOptions)
        let owinApp = builder.Build()

        owinApp
         