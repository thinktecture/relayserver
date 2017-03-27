﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog.Interface;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.Configuration;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Diagnostics
{
	internal class TraceManager : ITraceManager
	{
		private readonly ITraceRepository _traceRepository;
		private readonly ITraceFileWriter _traceFileWriter;
	    private readonly ITraceFileReader _traceFileReader;
	    private readonly IConfiguration _configuration;
		private readonly ILogger _logger;

	    private const string OnPremiseConnectorHeaderExtension = ".ct.headers";
		private const string OnPremiseConnectorContentExtension = ".ct.content";
		private const string OnPremiseTargetHeaderExtension = ".optt.headers";
		private const string OnPremiseTargetContentExtension = ".optt.content";

		public TraceManager(ITraceRepository traceRepository, ITraceFileWriter traceFileWriter, ITraceFileReader traceFileReader, IConfiguration configuration, ILogger logger)
		{
			_traceRepository = traceRepository;
			_traceFileWriter = traceFileWriter;
		    _traceFileReader = traceFileReader;
		    _configuration = configuration;
			_logger = logger;
		}

		public Guid? GetCurrentTraceConfigurationId(Guid linkId)
		{
			return _traceRepository.GetCurrentTraceConfigurationId(linkId);
		}

		public void Trace(IOnPremiseConnectorRequest onPremiseConnectorRequest, IOnPremiseTargetReponse onPremiseTargetReponse, Guid traceConfigurationId)
		{
			try
			{
				if (!Directory.Exists(_configuration.TraceFileDirectory))
				{
					Directory.CreateDirectory(_configuration.TraceFileDirectory);
				}

				var filenamePrefix = $"{Path.Combine(_configuration.TraceFileDirectory, traceConfigurationId.ToString())}-{DateTime.Now.Ticks}";

				_traceFileWriter.WriteHeaderFile(filenamePrefix + OnPremiseConnectorHeaderExtension, onPremiseConnectorRequest.HttpHeaders);
				_traceFileWriter.WriteContentFile(filenamePrefix + OnPremiseConnectorContentExtension, onPremiseConnectorRequest.Body);

				_traceFileWriter.WriteHeaderFile(filenamePrefix + OnPremiseTargetHeaderExtension, onPremiseTargetReponse.HttpHeaders);
				_traceFileWriter.WriteContentFile(filenamePrefix + OnPremiseTargetContentExtension, onPremiseTargetReponse.Body);
			}
			catch (Exception ex)
			{
				_logger.Warn("Could not create trace", ex);
			}
		}

		public async Task<IEnumerable<Trace>> GetTracesAsync(Guid traceConfigurationId)
		{
			var prefix = Path.Combine(_configuration.TraceFileDirectory, traceConfigurationId.ToString());

			var traceFilePrefixes = Directory.GetFiles(_configuration.TraceFileDirectory)
				.ToList()
				.Where(f => f.StartsWith(prefix))
				.Select(f => Path.GetFileName(
					f.Replace(OnPremiseConnectorHeaderExtension, string.Empty)
					.Replace(OnPremiseConnectorContentExtension, string.Empty)
					.Replace(OnPremiseTargetContentExtension, string.Empty)
					.Replace(OnPremiseTargetHeaderExtension, string.Empty)))
					.Distinct().ToList();
			var traceFileInfos = new List<Trace>();

			foreach (var traceFilePrefix in traceFilePrefixes)
			{
				try
				{
					traceFileInfos.Add(await GetTraceAsync(traceFilePrefix));
				}
				catch (Exception ex)
				{
					_logger.Warn("Could not read trace file information for prefix " + traceFilePrefix, ex);
				}
			}

			return traceFileInfos;
		}

	    public async Task<TraceFile> GetTraceFileAsync(string headerFileName)
	    {
	        var path = Path.Combine(_configuration.TraceFileDirectory, headerFileName);

	        var result = new TraceFile
            {
	            Headers = await _traceFileReader.ReadHeaderFileAsync(path),
	        };

	        if (result.IsContentAvailable)
	        {
                // TODO: Please refactor me :(
	            var contentPath = path.Replace(OnPremiseConnectorHeaderExtension, OnPremiseConnectorContentExtension);

	            if (contentPath.Equals(path))
	            {
	                contentPath = path.Replace(OnPremiseTargetHeaderExtension, OnPremiseTargetContentExtension);
	            }

	            result.Content = await _traceFileReader.ReadContentFileAsync(contentPath);
	        }

	        return result;
	    }

        private async Task<Trace> GetTraceAsync(string filePrefix)
		{
			var filePrefixWithDirectory = Path.Combine(_configuration.TraceFileDirectory, filePrefix);

			var clientHeaders = await _traceFileReader.ReadHeaderFileAsync(filePrefixWithDirectory + OnPremiseConnectorHeaderExtension);
			var onPremiseTargetHeaders = await _traceFileReader.ReadHeaderFileAsync(filePrefixWithDirectory + OnPremiseTargetHeaderExtension);

			var tracingDate = new DateTime(long.Parse(filePrefix.Split('-').Last()));

            return new Trace
            {
                OnPremiseConnectorTrace = new TraceFile
                {
                    ContentFileName = filePrefix + OnPremiseConnectorContentExtension,
                    HeaderFileName = filePrefix + OnPremiseConnectorHeaderExtension,
                    Headers = clientHeaders
                },

                OnPremiseTargetTrace = new TraceFile
                {
                    ContentFileName = filePrefix + OnPremiseTargetContentExtension,
                    HeaderFileName =  filePrefix + OnPremiseTargetHeaderExtension,
                    Headers = onPremiseTargetHeaders
                },

                TracingDate = tracingDate
            };
		}
	}
}