using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AlgorithmEasy.Shared.Requests;
using AlgorithmEasy.Shared.Utils;
using AlgorithmEasy.StudentSide.Shared;
using Blazored.Toast.Services;
using ToastTuple = System.Tuple<Blazored.Toast.Services.ToastLevel, string>;

namespace AlgorithmEasy.StudentSide.Services
{
    public class UserManageService
    {
        private readonly HttpClient _client;

        public UserManageService(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ALGORITHMEASY_USER_CENTER_URL")!);
        }

        public async Task<ToastTuple> Register(RegisterRequest request)
        {
            request.Password = (request.UserId + request.Password).GetSha256String();
            try
            {
                using var response = await _client.PostAsJsonAsync("Authentication/Register", request);
                var message = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                    return new(ToastLevel.Success, message);
                return new(ToastLevel.Error, message);
            }
            catch
            {
                return new(ToastLevel.Error, ErrorMessages.ConnectErrorMessage);
            }
        }
    }
}