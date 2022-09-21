﻿using Repository;
using Repository.Interfaces;

namespace CakeCurious_API.Utilities
{
    public class ScopedRepositoryRegister
    {
        public static void AddScopedRepositories(IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRecipeRepository, RecipeRepository>();
            services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();
        }
    }
}