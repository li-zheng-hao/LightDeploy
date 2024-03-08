using System.Reflection;
using Autofac;
using LightDeploy.Core.FileProvider;
using LightDeploy.Core.Options;
using Masuit.Tools;
using Microsoft.Extensions.Configuration;
using Module = Autofac.Module;

namespace LightDeploy.Core.Autofac;

/// <summary>
/// 
/// </summary>
public class AutofacModuleRegister : Module
{
    private readonly IConfiguration _configuration;
    private ContainerBuilder _builder;

    public AutofacModuleRegister(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void Load(ContainerBuilder builder)
    {
        _builder = builder;

        var basePath = AppContext.BaseDirectory;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(it => it.FullName.StartsWith("LightDeploy")).ToList();

        assemblies.ForEach(it =>
        {
            var transient = it.GetExportedTypes()
                .Where(it => it.IsClass && it.IsAssignableTo(typeof(ITransientDependency))).ToArray();

            builder.RegisterTypes(transient).AsSelf().InstancePerDependency()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            builder.RegisterTypes(transient).AsImplementedInterfaces().InstancePerDependency()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            var scoped = it.GetExportedTypes()
                .Where(it => it.IsClass && it.IsAssignableTo(typeof(IScopedDependency))).ToArray();
            builder.RegisterTypes(scoped).AsSelf().InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            builder.RegisterTypes(scoped).AsImplementedInterfaces().InstancePerDependency()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            var singleton = it.GetExportedTypes()
                .Where(it => it.IsClass && it.IsAssignableTo(typeof(ISingletonDependency))).ToArray();
            builder.RegisterTypes(singleton).AsSelf().SingleInstance()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            builder.RegisterTypes(singleton).AsImplementedInterfaces().InstancePerDependency()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
        });

        AddFileProvider();
    }

    private void AddFileProvider()
    {
        var fileStorageOptions = _configuration.GetSection(FileStorageOptions.SectionName).Get<FileStorageOptions>();

        if (fileStorageOptions?.LocalStorages.IsNullOrEmpty() == false)
        {
            foreach (var localStorage in fileStorageOptions.LocalStorages!)
            {
                _builder.Register<IFileProvider>((it, b) =>
                {
                    var fileProvider = new LocalFileProvider
                    {
                        RootDir = localStorage.StorageRootDir
                    };
                    // 判断是否为相对路径
                    if (!Path.IsPathRooted(fileProvider.RootDir))
                    {
                        fileProvider.RootDir = Path.Combine(AppContext.BaseDirectory, fileProvider.RootDir);
                    }

                    fileProvider.FileNameGenerateStrategy = localStorage.FileNameGenerateStrategy;
                    return fileProvider;
                }).Named<IFileProvider>(localStorage.Key);
            }
        }

        if (fileStorageOptions?.MinIOStorages.IsNullOrEmpty() == false)
        {
            foreach (var minioStorage in fileStorageOptions.MinIOStorages!)
            {
                _builder.Register<IFileProvider>((it, b) =>
                {
                    var fileProvider = new MinioFileProvider(minioStorage);

                    return fileProvider;
                }).Named<IFileProvider>(minioStorage.Key);
            }
        }
      
    }
}