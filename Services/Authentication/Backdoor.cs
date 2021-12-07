﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AlgorithmEasy.Shared.Models;
using AlgorithmEasy.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace AlgorithmEasy.StudentSide.Services.Authentication
{
    public class Backdoor : AuthenticationService
    {
        public override async Task<User> Login(string userId, byte[] password)
        {
            if (string.IsNullOrEmpty(userId) || password == null)
                return null;

            User = new User
            {
                UserId = userId,
                Role = new Role
                {
                    RoleName = "Student"
                },
                Nickname = "Backdoor"
            };

            var identity = GetClaimsIdentity("Login");
            var claims = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claims)));
            return User;
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