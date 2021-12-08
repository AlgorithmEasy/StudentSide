using System.Security.Claims;
using System.Threading.Tasks;
using AlgorithmEasy.Shared.Requests;
using AlgorithmEasy.Shared.Responses;
using AlgorithmEasy.Shared.Services;
using AlgorithmEasy.Shared.Statuses;
using Microsoft.AspNetCore.Components.Authorization;

namespace AlgorithmEasy.StudentSide.Services.Authentication
{
    public class Backdoor : AuthenticationService
    {
        public override Task<LoginStatus> Login(LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Password))
                return null;

            User = new LoginResponse
            {
                UserId = request.UserId,
                Role = "Student"
            };

            var identity = GetClaimsIdentity("Login");
            var claims = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claims)));
            return Task.FromResult(LoginStatus.Success);
        }

        public override void Logout()
        {
            User = null;

            var identity = new ClaimsIdentity();
            var claims = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claims)));
        }
    }
}