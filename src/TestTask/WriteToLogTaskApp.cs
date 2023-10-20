﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Task.RuntimeSdk;

namespace TestTask;

public class WriteToLogTaskApp : ITaskApp
{
    public void AddConfiguration(IConfigurationBuilder configBuilder)
    {
        
    }

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITaskLogic, WriteToLogTaskLogic>();
    }
}
