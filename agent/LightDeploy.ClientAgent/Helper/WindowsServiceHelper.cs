﻿using System.Globalization;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Serilog;

namespace LightDeploy.ClientAgent
{
    public static class WindowsServiceHelper
    {

       


        //public static string InstallWindowsService(string exePath)
        //{
        //    try
        //    {
        //        if (!exePath.Trim().ToLower().EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        //        {
        //            return $"{exePath} is not exe!";
        //        }

        //        string serviceName = GetServiceNameByFile(exePath);
        //        if (string.IsNullOrEmpty(serviceName))
        //        {
        //            return $"{exePath} is not windows service!";
        //        }

        //        if (ServiceIsExisted(serviceName))
        //        {
        //            return $"{serviceName} is exist!";
        //        }

        //        string[] cmdline = { };
        //        using (TransactedInstaller transactedInstaller = new TransactedInstaller())
        //        {
        //            using (AssemblyInstaller assemblyInstaller = new AssemblyInstaller(exePath, cmdline)
        //            {
        //                UseNewContext = true
        //            })
        //            {
        //                transactedInstaller.Installers.Add(assemblyInstaller);
        //                transactedInstaller.Install(new System.Collections.Hashtable());
        //            }

        //            return string.Empty;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //    }
        //}


        /// <summary>
        /// 判断服务是否已经存在
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>bool</returns>
        public static bool ServiceIsExisted(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            return services.Any(s => s.ServiceName == serviceName);
        }

        public static Tuple<ServiceController,string> GetWindowServiceByName(string serviceName)
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                return new Tuple<ServiceController, string>(services.FirstOrDefault(s => s.ServiceName == serviceName),null);
            }
            catch (Exception ex)
            {
                return new Tuple<ServiceController, string>(null,ex.Message);
            }
        }

        public static string GetWindowsServiceLocation(string serviceName)
        {
            var projectLocation= GetWindowServiceLocationForRaw(serviceName);
            //处理使用 nssm 安装的 Windows 服务程序
            if (projectLocation.EndsWith("nssm.exe", true, CultureInfo.CurrentCulture))
            {
                var _nssmOutput = "";
                var rt = ProcessHepler.RunExternalExe(projectLocation, $"get {serviceName} Application",
                    output => { _nssmOutput += Regex.Replace(output, @"\0", ""); return Task.CompletedTask; });

                if (!rt || string.IsNullOrEmpty(_nssmOutput.Trim()))
                {
                    return string.Empty;
                }

                projectLocation = _nssmOutput;
            }

            return projectLocation;
        }
        private static string GetWindowServiceLocationForRaw(string serviceName)
        {
            try
            {
                var serviceR = GetWindowServiceByName(serviceName);
                if(!string.IsNullOrEmpty(serviceR.Item2)) return null;
                if (serviceR.Item1 == null) return null;
                var machineName = Environment.MachineName;
                var registryPath = @"SYSTEM\CurrentControlSet\Services\" + serviceName;
                var keyHKLM = Registry.LocalMachine;

                RegistryKey key;
                if (machineName != "")
                {
                    key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, machineName).OpenSubKey(registryPath);
                }
                else
                {
                    key = keyHKLM.OpenSubKey(registryPath);
                }

                if (key == null) return null;
                var value = key.GetValue("ImagePath").ToString();
                if(value.IndexOf('"')>-1)
                {
                    var res = value.Split(' ')[0].Replace("\"","");
                    key.Close();
                    return res;
                }
                key.Close();
                return value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string StartService(string serviceName, int timeouSeconds=15)
        {
            int tryCount = 3;
            string errMessage=string.Empty;
            while (tryCount > 0)
            {
                errMessage=StartServiceInternal(serviceName, timeouSeconds);
                if (IsStart(serviceName))
                    return string.Empty;
                Thread.Sleep(1000);
                tryCount--;
            }

            return errMessage;
        }

        private static string StartServiceInternal(string serviceName, int timeouSeconds=15)
        {
            try
            {
                using (var service = new ServiceController(serviceName))
                {
                    var timeout = TimeSpan.FromSeconds(timeouSeconds);
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                    service.Refresh();
                    return string.Empty;
                }
            }
            catch (Exception e)
            {
                Log.Error(e,e.Message);
                return e.Message;
            }
        }

        public static bool IsStart(string serviceName)
        {
            try
            {
                using (var service = new ServiceController(serviceName))
                {
                    service.Refresh();
                    return service.Status == ServiceControllerStatus.Running;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string StopService(string serviceName, int timeoutMilliseconds=30)
        {
            try
            {
                using (var service = new ServiceController(serviceName))
                {
                    var timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    return string.Empty;
                }

            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static string GetStatus(string serviceName)
        {
            try
            {
                using (var service = new ServiceController(serviceName))
                {
                    service.Refresh();
                    return service.Status.ToString();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
