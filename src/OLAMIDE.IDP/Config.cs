// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace OLAMIDE.IDP
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResource("roles","role(s)",new List<string>{"role"}),
                new IdentityResource("country","your country",new List<string>{"country"}),
                new IdentityResource("subscription","subscription rule(s)",new List<string>{"subscription"})
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                //match the apiname on the API CONFIG
            new ApiResource("mideimageapi","IMAGE GALLERY",new List<string>{"role"}){
                Scopes= new List<string> { "mideimageapis" },
             ApiSecrets= { new Secret ("apisecrets".Sha256()) } }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {//match the scope on client
            new ApiScope("mideimageapis","IMAGE GALLERYs")
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            { new Client
             {
                AccessTokenType= AccessTokenType.Reference,
                //identity token lifetime,time the token is valid
                IdentityTokenLifetime=240,
                //requirement for setting refresh token
                AllowOfflineAccess=true,
                //updating token claims before expiration of refreshtoken if claims are updated
                UpdateAccessTokenClaimsOnRefresh=true,
                //access token lifetime default is 1hour
                AccessTokenLifetime=120,
               //max of 30days i.e refresh token lifetime
              // AbsoluteRefreshTokenLifetime=2000000,
               //refresh token lifetime- sliding(refreshes after every new requaest) tokenexpiration cannot go beyond absolute lifetime of 30
              // RefreshTokenExpiration= TokenExpiration.Sliding,
              // SlidingRefreshTokenLifetime=1400000,

                 ClientName="Image Gallery",
                 ClientId="csdclientgallery",
                 AllowedGrantTypes= GrantTypes.Code,
                 RedirectUris= new List<string>
                 {
                  "https://localhost:44389/signin-oidc"
                 },
                 PostLogoutRedirectUris=new List<string>
                 {
                  "https://localhost:44389/signout-callback-oidc"
                 },
                 AllowedScopes=
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Address,
                    "roles",
                    "mideimageapis",
                    "country",
                    "subscription"
                },
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                //RequirePkce=false,
                RequireConsent=true
                }
           };
    }
}