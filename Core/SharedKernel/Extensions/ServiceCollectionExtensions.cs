using System.Reflection;

namespace BN.PROJECT.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWithAllInterfaces<T>(this IServiceCollection services) where T : class
        {
            services.AddSingleton<T, T>();
            Type[] interfaces = typeof(T).GetInterfaces();
            foreach (Type serviceType in interfaces)
            {
                services.AddSingleton(serviceType, (IServiceProvider x) => x.GetRequiredService(typeof(T)));
            }

            return services;
        }

        public static IServiceCollection AddWithAllInterfaces<T>(this IServiceCollection services, T service) where T : class
        {
            services.AddSingleton(service);
            Type[] interfaces = typeof(T).GetInterfaces();
            foreach (Type serviceType in interfaces)
            {
                services.AddSingleton(serviceType, (IServiceProvider x) => x.GetRequiredService(typeof(T)));
            }

            return services;
        }

        public static IServiceCollection DeepRemove<T>(this IServiceCollection services)
        {
            Type originalType = typeof(T);
            ServiceDescriptor serviceDescriptor = services.FirstOrDefault((ServiceDescriptor s) => s.ServiceType == originalType);
            if (serviceDescriptor == null)
            {
                return services;
            }

            Type type = (originalType.IsInterface ? (serviceDescriptor.ImplementationType ?? serviceDescriptor.ImplementationFactory?.Target?.GetType().GenericTypeArguments.FirstOrDefault() ?? serviceDescriptor.ImplementationInstance?.GetType()) : originalType);
            if (type == null)
            {
                return services;
            }

            Remove(type, type);
            Type[] interfaces = type.GetInterfaces();
            foreach (Type servType2 in interfaces)
            {
                Remove(servType2, type);
            }

            return services;
            void Remove(Type servType, Type implType)
            {
                ServiceDescriptor serviceDescriptor2 = services.FirstOrDefault((ServiceDescriptor y) => y.ServiceType == servType && ((y.ImplementationFactory?.Target?.ToString()?.Contains(implType.Name)).GetValueOrDefault() || y.ImplementationType == implType || y.ImplementationInstance?.GetType() == implType));
                if (serviceDescriptor2 != null)
                {
                    services.Remove(serviceDescriptor2);
                }
            }
        }

        public static void AddWithAllDerivedTypes<T>(this IServiceCollection services)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                services.AddWithAllDerivedTypes<T>(assembly);
            }
        }

        public static void AddWithAllDerivedTypes<T>(this IServiceCollection services, Assembly assembly)
        {
            foreach (TypeInfo item in assembly.DefinedTypes.Where(Filter<T>))
            {
                services.AddScoped(typeof(T), item);
            }
        }

        private static bool Filter<T>(TypeInfo info)
        {
            if (!info.GetInterfaces().Contains(typeof(T)) || info.IsAbstract)
            {
                if (info.BaseType != null && info.BaseType == typeof(T))
                {
                    return info.BaseType.IsAbstract;
                }

                return false;
            }

            return true;
        }
    }
}