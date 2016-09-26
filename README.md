# Hosting and using IdentityServer on Suave.

This example spawns IdentityServer running on localhost port 8003 and shows a `authenticated` combinator that works with the IdentityServer middleware to provide OAuth2 authorization.

Then it lauches a web server on port 8083 with a protected resource path "/devices".

IdentityServer: https://127.0.0.1:8003/identityserver  
Protected Resource : https://127.0.0.1:8083/devices
