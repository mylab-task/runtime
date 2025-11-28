
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace MyLab.Task.Runtime
{
    static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddSubconfig(this IConfigurationBuilder cb, IConfigurationSection? section, string path)
        {
            if (cb is null) throw new ArgumentNullException(nameof(cb));
            if (string.IsNullOrEmpty(path)) throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));

            if (section is null) return cb;

            var newDict = new Dictionary <string, string>();
            RetrieveKeys(section, path, newDict);

            cb.AddInMemoryCollection(newDict);

            return cb;
        } 

        static void RetrieveKeys(IConfigurationSection section, string? parentPath, Dictionary<string, string> targetDict)
        {
            var children = section.GetChildren();
        
            foreach (var child in children)
            {
                var childKey = parentPath != null ? $"{parentPath}:" + child.Key : child.Key;
                if(child.Value != null)
                {
                    targetDict.Add(childKey, child.Value);
                }
                else
                {
                    RetrieveKeys(child, childKey, targetDict);
                }
            }
        }
    }
}
