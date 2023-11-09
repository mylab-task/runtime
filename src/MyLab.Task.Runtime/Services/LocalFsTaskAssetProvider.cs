using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using MyLab.Log;

namespace MyLab.Task.Runtime
{
    class LocalFsTaskAssetProvider : ITaskAssetProvider
    {
        private string _basePath;

        public LocalFsTaskAssetProvider(IOptions<RuntimeOptions> opts)
            : this(opts.Value.AssetPath)
        {
        }

        public LocalFsTaskAssetProvider(string basePath)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        public IEnumerable<TaskAssetSource> Provide()
        {
            var libDir = new DirectoryInfo(_basePath);

            if(!libDir.Exists)
            {
                throw new DirectoryNotFoundException("Assets directory not found")
                    .AndFactIs("path", _basePath);
            }

            return libDir.GetDirectories().Select(CreateTaskAsset);
        }

        private TaskAssetSource CreateTaskAsset(DirectoryInfo assetDir)
        {
            var fullAssemblyPath = Path.Combine(assetDir.FullName, assetDir.Name + ".dll");
            return new TaskAssetSource
            (
                assetDir.Name,  
                new FileAssemblyLoader(fullAssemblyPath)
            );
        }
    }

}
