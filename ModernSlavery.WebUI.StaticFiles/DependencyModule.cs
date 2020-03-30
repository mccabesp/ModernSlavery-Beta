using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.WebUI.StaticFiles
{
    public class DependencyModule : IDependencyModule
    {
        private readonly ILogger _logger;
        private readonly SharedOptions _sharedOptions;
        private readonly IFileRepository _fileRepository;
        public DependencyModule(
            ILogger<DependencyModule> logger,
            SharedOptions sharedOptions, 
            IFileRepository fileRepository)
        {
            _logger = logger;
            _sharedOptions = sharedOptions;
            _fileRepository = fileRepository;
        }

        public void Register(IDependencyBuilder builder)
        {
            //TODO: Register dependencies here
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Configure dependencies here

            //Ensure ShortCodes, SicCodes and SicSections exist on remote 
            Task.WaitAll(
                _fileRepository.PushRemoteFileAsync(Filenames.ShortCodes, _sharedOptions.DataPath),
                _fileRepository.PushRemoteFileAsync(Filenames.SicCodes, _sharedOptions.DataPath),
                _fileRepository.PushRemoteFileAsync(Filenames.SicSections, _sharedOptions.DataPath)
            );
        }
    }
}