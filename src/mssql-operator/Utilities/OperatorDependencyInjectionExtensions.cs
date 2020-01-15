using System;
using System.Collections.Generic;
using System.Text;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using OperatorSharp;
using OperatorSharp.CustomResources;

namespace MSSqlOperator.Utilities
{
    public static class OperatorDependencyInjectionExtensions
    {
        public static ServiceCollection AddOperator<TResource, TOperator>(this ServiceCollection services) where TResource: CustomResource
            where TOperator: Operator<TResource>
        {
            services.AddScoped<IEventRecorder<TResource>, EventRecorder<TResource>>();
            services.AddScoped<TOperator>();

            return services;
        }
    }
}
