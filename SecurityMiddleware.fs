namespace IdentityServer

module SecurityMiddleware =
    open Owin
    open Microsoft.Owin.Logging
    open Microsoft.Owin.Builder
    open Microsoft.Owin.Security.Jwt
    open IdentityServer3.AccessTokenValidation
    open IdentityServer3.Core.Configuration
    open IdentityServer3.Core.Models
    open IdentityServer3.Core.Services.InMemory
    open System.Security.Cryptography.X509Certificates
    open Suave
    open System
    open System.Collections.Generic
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

    let certificate = new X509Certificate2("IdentityServerExample.pfx","easy")

    let securityMiddleware _ = 

      let app = new AppBuilder()

      let authenticationOptions = new IdentityServerBearerTokenAuthenticationOptions 
                                          (
                                              Authority = "http://127.0.0.1:8003/identityserver",
                                              RequiredScopes = ["devices"], 
                                              EnableValidationResultCache = true,
                                              ValidationMode = ValidationMode.ValidationEndpoint,
                                              ValidationResultCacheDuration = TimeSpan.FromMinutes(5.0)                                                
                                          )

      app.SetLoggerFactory(new MyCustomLoggerFactory())

      let builder = app.UseIdentityServerBearerTokenAuthentication(authenticationOptions)
      let owinApp = builder.Build()

      owinApp

    let identityServer = 

        let builder = new AppBuilder()

        let scopes = [
          new Scope(Name = "devices")]

        let clients = [
          new Client(
            ClientName = "Smartphone App",
            ClientId="smartphone",
            Enabled=true,
            AccessTokenType=AccessTokenType.Jwt,
            Flow = Flows.ClientCredentials,
            ClientSecrets = new List<Secret>([ new Secret("89C4C260-08B5-4B8F-82A7-7B513B8CB8BA".Sha256())]),
            AllowedScopes = new List<string>(scopes |> Seq.map (fun v -> v.Name)))]
        let users =
          new List<InMemoryUser>( seq{
            yield new InMemoryUser(Username = "bob", Password = "secret", Subject = "1")
            yield new InMemoryUser(Username = "alice", Password = "secret", Subject = "2")})

        let serviceFactory =
          (new IdentityServerServiceFactory())
            .UseInMemoryScopes(scopes)
            .UseInMemoryClients(clients)
            .UseInMemoryUsers(users)
        let options =
          new IdentityServerOptions(
            SiteName = "AtwoodHome",
            Factory = serviceFactory,
            SigningCertificate = certificate,
            RequireSsl = false,
            EnableWelcomePage = true)
        let builder = builder.UseIdentityServer(options)
        builder.Build()
         