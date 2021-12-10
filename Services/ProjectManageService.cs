﻿using System;
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

namespace AlgorithmEasy.StudentSide.Services
{
    public class ProjectManageService
    {
        private readonly HttpClient _client;
        private readonly AuthenticationService _authentication;

        private IEnumerable<Project> _projects;
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
        public event EventHandler<Project> OnCurrentProjectChanged;

        public ProjectManageService(HttpClient client, AuthenticationService authentication)
        {
            _client = client;
            _client.BaseAddress = new Uri("https://localhost:7001");

            _authentication = authentication;
            _authentication.LoginEventHandler += (_, arg) =>
                _client.DefaultRequestHeaders.Authorization = new("Bearer", arg.Token);
            _authentication.LogoutEventHandler += (_, _) =>
                _client.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<Tuple<ToastLevel, string>> GetPersonalProjects()
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
                return null;
            }
            catch
            {
                return new(ToastLevel.Error, ErrorMessages.ConnectErrorMessage);
            }
        }

        public async Task<Tuple<ToastLevel, string>> CreateProject(string projectName)
        {
            var request = new CreateProjectRequest
            {
                ProjectName = projectName
            };

            string message;
            try
            {
                using var response = await _client.PostAsJsonAsync("ProjectManage/CreateProject", request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authentication.Logout();
                    return new(ToastLevel.Error, ErrorMessages.UnAuthorizedErrorMessage);
                }
                message = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                    return new(ToastLevel.Error, message);
            }
            catch
            {
                return new(ToastLevel.Error, ErrorMessages.ConnectErrorMessage);
            }

            var ret = await GetPersonalProjects();
            if (ret.Item1 != ToastLevel.Success)
                return ret;

            CurrentProject = Projects?.SingleOrDefault(project => project.ProjectName == projectName);
            return new(ToastLevel.Success, message);
        }

        public async Task<Tuple<ToastLevel, string>> SaveProject()
        {
            var request = new SaveProjectRequest
            {
                ProjectName = CurrentProject!.ProjectName,
                Workspace = CurrentProject!.Workspace
            };
            try
            {
                using var response = await _client.PostAsJsonAsync("ProjectManage/SaveProject", request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authentication.Logout();
                    return new(ToastLevel.Error, ErrorMessages.UnAuthorizedErrorMessage);
                }

                var message = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                    return new(ToastLevel.Success, message);

                CurrentProject = null;
                return new(ToastLevel.Error, message);
            }
            catch
            {
                return new(ToastLevel.Error, ErrorMessages.ConnectErrorMessage);
            }
        }

        public async Task<Tuple<ToastLevel, string>> RenameProject(string oldProjectName, string newProjectName)
        {
            var request = new RenameProjectRequest
            {
                OldProjectName = oldProjectName,
                NewProjectName = newProjectName
            };
            try
            {
                using var response = await _client.PostAsJsonAsync("ProjectManage/RenameProject", request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authentication.Logout();
                    return new(ToastLevel.Error, ErrorMessages.UnAuthorizedErrorMessage);
                }

                var message = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (CurrentProject != null && CurrentProject?.ProjectName == oldProjectName)
                        CurrentProject.ProjectName = newProjectName;
                    return new(ToastLevel.Success, message);
                }

                return new(ToastLevel.Error, message);
            }
            catch
            {
                return new(ToastLevel.Error, ErrorMessages.ConnectErrorMessage);
            }
        }

        public async Task<Tuple<ToastLevel, string>> DeleteProject(string projectName)
        {
            var request = new DeleteProjectRequest
            {
                ProjectName = projectName,
            };
            try
            {
                using var response = await _client.PostAsJsonAsync("ProjectManage/DeleteProject", request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authentication.Logout();
                    return new(ToastLevel.Error, ErrorMessages.UnAuthorizedErrorMessage);
                }

                var message = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (CurrentProject?.ProjectName == projectName)
                        CurrentProject = null;
                    return new(ToastLevel.Success, message);
                }

                return new(ToastLevel.Error, message);
            }
            catch
            {
                return new(ToastLevel.Error, ErrorMessages.ConnectErrorMessage);
            }
        }
    }
}