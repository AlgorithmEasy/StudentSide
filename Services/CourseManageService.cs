using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AlgorithmEasy.Shared.Models;
using AlgorithmEasy.Shared.Responses;
using AlgorithmEasy.Shared.Services;
using AlgorithmEasy.StudentSide.Shared;
using Blazored.Toast.Services;

using ToastTuple = System.Tuple<Blazored.Toast.Services.ToastLevel, string>;

namespace AlgorithmEasy.StudentSide.Services
{
    public class CourseManageService
    {
        private readonly HttpClient _client;
        private readonly AuthenticationService _authentication;

        private IEnumerable<Course> _courses = new List<Course>();
        public IEnumerable<Course> Courses
        {
            get => _courses;
            set
            {
                _courses = value;
                OnCoursesSet?.Invoke(this, value);
            }
        }

        private IEnumerable<LearningHistory> _learningHistories = new List<LearningHistory>();
        public IEnumerable<LearningHistory> LearningHistories
        {
            get => _learningHistories;
            set
            {
                _learningHistories = value;
                OnLearningHistoriesSet?.Invoke(this, value);
            }
        }

#nullable enable
        private Course? _currentCourse;
        public Course? CurrentCourse
        {
            get => _currentCourse;
            set
            {
                if (CurrentCourse?.CourseId != value?.CourseId)
                    OnCurrentCourseChanged?.Invoke(this, value);
                _currentCourse = value;
            }
        }
#nullable disable

        public event EventHandler<IEnumerable<Course>> OnCoursesSet;
        public event EventHandler<IEnumerable<LearningHistory>> OnLearningHistoriesSet;
        public event EventHandler<Course> OnCurrentCourseChanged;

        public CourseManageService(HttpClient client, AuthenticationService authentication)
        {
            _client = client;
            _client.BaseAddress = new Uri("https://localhost:8001");

            _authentication = authentication;

            var jwt = authentication.GetAuthenticationStateAsync().Result.User?.FindFirst("Json Web Token")?.Value;
            if (!string.IsNullOrEmpty(jwt))
                _client.DefaultRequestHeaders.Authorization = new("Bearer", jwt);

            _authentication.LoginEventHandler += (_, user) =>
                _client.DefaultRequestHeaders.Authorization = new("Bearer", user.Token);
            _authentication.LogoutEventHandler += (_, _) =>
                _client.DefaultRequestHeaders.Authorization = null;
        }

        #region HttpClient
        public async Task<ToastTuple> GetCourses()
        {
            try
            {
                using var response = await _client.GetAsync("CourseManage/GetCourses");
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authentication.Logout();
                    return new(ToastLevel.Error, ErrorMessages.UnAuthorizedErrorMessage);
                }

                Courses = (await response.Content.ReadFromJsonAsync<GetCoursesResponse>())?.Courses;
                if (Courses == null)
                    return new(ToastLevel.Error, ErrorMessages.ResponseErrorMessage);
                return new(ToastLevel.Success, string.Empty);
            }
            catch
            {
                return new(ToastLevel.Error, ErrorMessages.ConnectErrorMessage);
            }
        }

        public async Task<ToastTuple> GetCourseDetail(int courseId)
        {
            try
            {
                using var response = await _client.GetAsync($"CourseManage/GetCourseDetail?courseId={courseId}");
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authentication.Logout();
                    return new(ToastLevel.Error, ErrorMessages.UnAuthorizedErrorMessage);
                }

                CurrentCourse = await response.Content.ReadFromJsonAsync<Course>();
                if (CurrentCourse == null)
                    return new(ToastLevel.Error, ErrorMessages.ResponseErrorMessage);
                return new(ToastLevel.Success, "加载课程内容成功。");
            }
            catch
            {
                return new(ToastLevel.Error, ErrorMessages.ConnectErrorMessage);
            }
        }

        public async Task<ToastTuple> GetPersonalLearningHistories()
        {
            try
            {
                using var response = await _client.GetAsync("LearningHistoryManage/GetPersonalLearningHistories");
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authentication.Logout();
                    return new(ToastLevel.Error, ErrorMessages.UnAuthorizedErrorMessage);
                }

                LearningHistories =
                    (await response.Content.ReadFromJsonAsync<GetPersonalLearningHistoriesResponse>())?.LearningHistories;
                if (LearningHistories == null)
                    return new(ToastLevel.Error, ErrorMessages.ResponseErrorMessage);
                return new(ToastLevel.Success, string.Empty);
            }
            catch
            {
                return new(ToastLevel.Error, ErrorMessages.ConnectErrorMessage);
            }
        }

        public async Task<ToastTuple> SaveLearningProgress(int courseId, int progress)
        {
            try
            {
                using var response = await _client.PutAsync(
                    "LearningHistoryManage/UpdateLearningProgress", new StringContent(string.Empty));
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
        #endregion
    }
}