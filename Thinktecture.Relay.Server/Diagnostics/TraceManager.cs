using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;
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

		private const string _ON_PREMISE_CONNECTOR_HEADER_EXTENSION = ".ct.headers";
		private const string _ON_PREMISE_CONNECTOR_CONTENT_EXTENSION = ".ct.content";
		private const string _ON_PREMISE_TARGET_HEADER_EXTENSION = ".optt.headers";
		private const string _ON_PREMISE_TARGET_CONTENT_EXTENSION = ".optt.content";

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

		public void Trace(IOnPremiseConnectorRequest onPremiseConnectorRequest, IOnPremiseTargetResponse onPremiseTargetResponse, Guid traceConfigurationId)
		{
			try
			{
				if (!Directory.Exists(_configuration.TraceFileDirectory))
				{
					Directory.CreateDirectory(_configuration.TraceFileDirectory);
				}

				var filenamePrefix = $"{Path.Combine(_configuration.TraceFileDirectory, traceConfigurationId.ToString())}-{DateTime.Now.Ticks}";

				_traceFileWriter.WriteHeaderFile(filenamePrefix + _ON_PREMISE_CONNECTOR_HEADER_EXTENSION, onPremiseConnectorRequest.HttpHeaders);
				_traceFileWriter.WriteContentFile(filenamePrefix + _ON_PREMISE_CONNECTOR_CONTENT_EXTENSION, onPremiseConnectorRequest.Body);

				_traceFileWriter.WriteHeaderFile(filenamePrefix + _ON_PREMISE_TARGET_HEADER_EXTENSION, onPremiseTargetResponse.HttpHeaders);
				_traceFileWriter.WriteContentFile(filenamePrefix + _ON_PREMISE_TARGET_CONTENT_EXTENSION, onPremiseTargetResponse.Body);
			}
			catch (Exception ex)
			{
				_logger.Warn(ex, "Could not create trace");
			}
		}

		public async Task<IEnumerable<Trace>> GetTracesAsync(Guid traceConfigurationId)
		{
			var prefix = Path.Combine(_configuration.TraceFileDirectory, traceConfigurationId.ToString());

			var traceFilePrefixes = Directory.GetFiles(_configuration.TraceFileDirectory)
				.ToList()
				.Where(f => f.StartsWith(prefix))
				.Select(f => Path.GetFileName(
					f.Replace(_ON_PREMISE_CONNECTOR_HEADER_EXTENSION, String.Empty)
						.Replace(_ON_PREMISE_CONNECTOR_CONTENT_EXTENSION, String.Empty)
						.Replace(_ON_PREMISE_TARGET_CONTENT_EXTENSION, String.Empty)
						.Replace(_ON_PREMISE_TARGET_HEADER_EXTENSION, String.Empty)))
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
					_logger.Warn(ex, "Could not read trace file information for prefix {0}", traceFilePrefix);
				}
			}

			return traceFileInfos;
		}

		public async Task<TraceFile> GetTraceFileAsync(string headerFileName)
		{
			var path = Path.Combine(_configuration.TraceFileDirectory, headerFileName);

			var result = new TraceFile()
			{
				Headers = await _traceFileReader.ReadHeaderFileAsync(path),
			};

			if (result.IsContentAvailable)
			{
				// TODO: Please refactor me :(
				var contentPath = path.Replace(_ON_PREMISE_CONNECTOR_HEADER_EXTENSION, _ON_PREMISE_CONNECTOR_CONTENT_EXTENSION);

				if (contentPath.Equals(path))
				{
					contentPath = path.Replace(_ON_PREMISE_TARGET_HEADER_EXTENSION, _ON_PREMISE_TARGET_CONTENT_EXTENSION);
				}

				result.Content = await _traceFileReader.ReadContentFileAsync(contentPath);
			}

			return result;
		}

		private async Task<Trace> GetTraceAsync(string filePrefix)
		{
			var filePrefixWithDirectory = Path.Combine(_configuration.TraceFileDirectory, filePrefix);

			var clientHeaders = await _traceFileReader.ReadHeaderFileAsync(filePrefixWithDirectory + _ON_PREMISE_CONNECTOR_HEADER_EXTENSION);
			var onPremiseTargetHeaders = await _traceFileReader.ReadHeaderFileAsync(filePrefixWithDirectory + _ON_PREMISE_TARGET_HEADER_EXTENSION);

			var tracingDate = new DateTime(long.Parse(filePrefix.Split('-').Last()));

			return new Trace()
			{
				OnPremiseConnectorTrace = new TraceFile()
				{
					ContentFileName = filePrefix + _ON_PREMISE_CONNECTOR_CONTENT_EXTENSION,
					HeaderFileName = filePrefix + _ON_PREMISE_CONNECTOR_HEADER_EXTENSION,
					Headers = clientHeaders
				},
				OnPremiseTargetTrace = new TraceFile()
				{
					ContentFileName = filePrefix + _ON_PREMISE_TARGET_CONTENT_EXTENSION,
					HeaderFileName = filePrefix + _ON_PREMISE_TARGET_HEADER_EXTENSION,
					Headers = onPremiseTargetHeaders
				},
				TracingDate = tracingDate
			};
		}
	}
}
