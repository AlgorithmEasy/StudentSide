using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AlgorithmEasy.Shared.Models;
using AlgorithmEasy.Shared.Requests;
using AlgorithmEasy.Shared.Responses;
using AlgorithmEasy.Shared.Services;
using AlgorithmEasy.StudentSide.Shared;
using Blazored.Toast.Services;

using ToastTuple = System.Tuple<Blazored.Toast.Services.ToastLevel, string>;

namespace AlgorithmEasy.StudentSide.Services
{
    public class ProjectManageService
    {
        private readonly HttpClient _client;
        private readonly AuthenticationService _authentication;

        private IEnumerable<Project> _projects = new List<Project>();
        public IEnumerable<Project> Projects
        {
            get => _projects;
            private set
            {
                _projects = value;
                OnProjectsSet?.Invoke(this, value);
            }
        }

#nullable enable
        private Project? _currentProject;
        public Project? CurrentProject
        {
            get => _currentProject;
            private set
            {
                if (_currentProject?.ProjectName != value?.ProjectName)
                    OnCurrentProjectChanged?.Invoke(this, value);
                _currentProject = value;
            }
        }
#nullable disable

        public event EventHandler<IEnumerable<Project>> OnProjectsSet;

#nullable enable
        public event EventHandler<Project?>? OnCurrentProjectChanged;
#nullable disable

        public ProjectManageService(HttpClient client, AuthenticationService authentication)
        {
            _authentication = authentication;

            _client = client;
            _client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ALGORITHMEASY_PROJECT_CENTER_URL")!);

            var jwt = authentication.GetAuthenticationStateAsync().Result.User?.FindFirst("Json Web Token")?.Value;
            if (!string.IsNullOrEmpty(jwt))
                _client.DefaultRequestHeaders.Authorization = new("Bearer", jwt);

            _authentication.LoginEventHandler += (_, user) =>
                _client.DefaultRequestHeaders.Authorization = new("Bearer", user.Token);
            _authentication.LogoutEventHandler += (_, _) =>
                _client.DefaultRequestHeaders.Authorization = null;
        }

        public void LoadProject(string projectName)
        {
            CurrentProject = Projects.SingleOrDefault(project => project.ProjectName == projectName);
        }

        public void LoadProject(Project project)
        {
            LoadProject(project.ProjectName);
        }

        #region HttpClient
        public async Task<ToastTuple> GetPersonalProjects()
        {
            try
            {
                using var response = await _client.GetAsync("ProjectManage/GetPersonalProjects");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authentication.Logout();
                    return new(ToastLevel.Error, ErrorMessages.UnAuthorizedErrorMessage);
                }

                var projects = await response.Content.ReadFromJsonAsync<GetPersonalProjectsResponse>();
                Projects = projects?.Projects ?? new List<Project>();
                if (projects?.Projects == null)
                    return new(ToastLevel.Error, ErrorMessages.ResponseErrorMessage);
                return new(ToastLevel.Success, string.Empty);
            }
            catch
            {
                return new(ToastLevel.Error, ErrorMessages.ConnectErrorMessage);
            }
        }

        public async Task<ToastTuple> CreateProject(string projectName)
        {
            try
            {
                using var response = await _client.PostAsJsonAsync(
                    "ProjectManage/CreateProject",
                    new CreateProjectRequest
                    {
                        ProjectName = projectName
                    });
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authentication.Logout();
                    return new(ToastLevel.Error, ErrorMessages.UnAuthorizedErrorMessage);
                }
                var message = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                    return new(ToastLevel.Error, message);
                return new(ToastLevel.Success, message);
            }
            catch
            {
                return new(ToastLevel.Error, ErrorMessages.ConnectErrorMessage);
            }
        }

        public async Task<ToastTuple> SaveProject()
        {
            if (CurrentProject == null)
                return new(ToastLevel.Error, "请先打开一个项目");
            try
            {
                using var response = await _client.PutAsync(
                    $"ProjectManage/SaveProject?projectName={CurrentProject.ProjectName}",
                    new StringContent(CurrentProject.Workspace));
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authentication.Logout();
                    return new(ToastLevel.Error, ErrorMessages.UnAuthorizedErrorMessage);
                }

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

        public async Task<ToastTuple> RenameProject(string oldProjectName, string newProjectName)
        {
            if (oldProjectName == newProjectName)
                return new(ToastLevel.Error, $"新项目名{newProjectName}与原项目名相同，请重新输入。");
            try
            {
                using var response = await _client.PutAsJsonAsync(
                    "ProjectManage/RenameProject",
                    new RenameProjectRequest
                    {
                        OldProjectName = oldProjectName,
                        NewProjectName = newProjectName
                    });
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authentication.Logout();
                    return new(ToastLevel.Error, ErrorMessages.UnAuthorizedErrorMessage);
                }

                var message = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK) return new(ToastLevel.Error, message);
                if (CurrentProject != null && CurrentProject?.ProjectName == oldProjectName)
                    CurrentProject.ProjectName = newProjectName;
                return new(ToastLevel.Success, message);
            }
            catch
            {
                return new(ToastLevel.Error, ErrorMessages.ConnectErrorMessage);
            }
        }

        public async Task<ToastTuple> DeleteProject(string projectName)
        {
            try
            {
                using var response = await _client.DeleteAsync($"ProjectManage/DeleteProject?ProjectName={projectName}");
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authentication.Logout();
                    return new(ToastLevel.Error, ErrorMessages.UnAuthorizedErrorMessage);
                }

                var message = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK) return new(ToastLevel.Error, message);
                if (CurrentProject?.ProjectName == projectName)
                    CurrentProject = null;
                return new(ToastLevel.Success, message);
            }
            catch
            {
                return new(ToastLevel.Error, ErrorMessages.ConnectErrorMessage);
            }
        }
        #endregion
    }
}