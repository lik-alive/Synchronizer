using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Synchronizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            String name = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";
            String[] resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            String resourceName = null;
            foreach (String resource in resources)
            {
                if (resource.EndsWith(name))
                {
                    resourceName = resource;
                    break;
                }
            }
            if (resourceName == null) return null;

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                byte[] block = new byte[stream.Length];
                stream.Read(block, 0, block.Length);
                return Assembly.Load(block);
            }
        }
    }
}
