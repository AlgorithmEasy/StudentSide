using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AlgorithmEasy.Shared.Requests;
using AlgorithmEasy.Shared.Services;
using AlgorithmEasy.StudentSide.Shared;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.SignalR.Client;

using ToastTuple = System.Tuple<Blazored.Toast.Services.ToastLevel, string>;

namespace AlgorithmEasy.StudentSide.Services
{
    public class ProgramExecuteService
    {
        private readonly HttpClient _client;
        private readonly AuthenticationService _authentication;

        public bool Running { get; private set; }
        public string ConnectId { get; set; }

        public EventHandler BeforeExecute;
        public EventHandler AfterExecute;

        public ProgramExecuteService(HttpClient client, AuthenticationService authentication)
        {
            _client = client;
            _client.BaseAddress =
                new Uri(Environment.GetEnvironmentVariable("ALGORITHMEASY_PROGRAM_EXECUTION_CENTER_URL")!);

            _authentication = authentication;

            var jwt = authentication.GetAuthenticationStateAsync().Result.User?.FindFirst("Json Web Token")?.Value;
            if (!string.IsNullOrEmpty(jwt))
                _client.DefaultRequestHeaders.Authorization = new("Bearer", jwt);

            _authentication.LoginEventHandler += (_, user) =>
                _client.DefaultRequestHeaders.Authorization = new("Bearer", user.Token);
            _authentication.LogoutEventHandler += (_, _) =>
                _client.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<ToastTuple> ExecutePythonCode(string code)
        {
            var request = new ExecutePythonRequest
            {
                ConnectId = ConnectId,
                Code = code
            };

            Running = true;
            BeforeExecute?.Invoke(this, EventArgs.Empty);
            try
            {
                using var response = await _client.PostAsJsonAsync("ProgramExecute/ExecutePython", request);
                Running = false;
                AfterExecute?.Invoke(this, EventArgs.Empty);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authentication.Logout();
                    return new(ToastLevel.Error, ErrorMessages.UnAuthorizedErrorMessage);
                }

                return new(ToastLevel.Success, "项目运行结束。");
            }
            catch
            {
                Running = false;
                AfterExecute?.Invoke(this, EventArgs.Empty);
                return new(ToastLevel.Error, ErrorMessages.ConnectErrorMessage);
            }
        }
    }
}