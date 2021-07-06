using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilme
{
    public static class SdpTransformServiceExtensions
    {
        public static IServiceCollection AddSdpTransform(this IServiceCollection services)
        {
            services.AddSingleton<ISdpTransform, SdpTransform>();

            return services;
        }
    }
}
