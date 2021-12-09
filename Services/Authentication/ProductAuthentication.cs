using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using AlgorithmEasy.Shared.Requests;
using AlgorithmEasy.Shared.Responses;
using AlgorithmEasy.Shared.Services;
using AlgorithmEasy.Shared.Statuses;
using Microsoft.AspNetCore.Components.Authorization;

namespace AlgorithmEasy.StudentSide.Services.Authentication
{
    public class ProductAuthentication : AuthenticationService
    {
        private readonly HttpClient _client;

        public ProductAuthentication(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri("https://localhost:6001");
        }

        public override async Task<LoginStatus> Login(LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Password))
                return LoginStatus.LoginFailed;

            try
            {
                using var response = await _client.PostAsJsonAsync("Authentication/Login", request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return LoginStatus.LoginFailed;

                User = await response.Content.ReadFromJsonAsync<LoginResponse>();
            }
            catch
            {
                return LoginStatus.ConnectServerFailed;
            }

            var identity = GetClaimsIdentity("Login");
            var claims = new ClaimsPrincipal(identity);

            LoginEventHandle();

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claims)));
            return LoginStatus.Success;
        }

        public override void Logout()
        {
            User = null;

            var identity = new ClaimsIdentity();
            var claims = new ClaimsPrincipal(identity);

            LogoutEventHandle();

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claims)));
        }
    }
}