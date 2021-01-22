using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a store to persist the body of a request and response in files.
	/// </summary>
	internal class FileBodyStore : IBodyStore
	{
		private readonly ILogger<FileBodyStore> _logger;
		private readonly string _basePath;

		public FileBodyStore(ILogger<FileBodyStore> logger, IOptions<FileBodyStoreOptions> fileBodyStoreOptions)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_basePath = fileBodyStoreOptions?.Value.StoragePath ?? throw new ArgumentNullException(nameof(fileBodyStoreOptions));

			_logger.LogDebug("Using {StorageType} with storage path {StoragePath} as body store", nameof(FileBodyStore), _basePath);
		}

		/// <inheritdoc />
		public async Task<long> StoreRequestBodyAsync(Guid requestId, Stream bodyStream, CancellationToken cancellationToken = default)
		{
			try
			{
				_logger.LogTrace("{FileOperation} {FileBodyType} body for request {RequestId}", "Writing", "request", requestId);
				var size = await StoreBodyAsync(BuildRequestFilePath(requestId), bodyStream, cancellationToken);

				_logger.LogDebug("Writing of {FileBodyType} body for request {RequestId} completed with {BodySize} bytes", "request", requestId,
					size);
				return size;
			}
			catch (OperationCanceledException)
			{
				await RemoveRequestBodyAsync(requestId, CancellationToken.None);
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while {FileOperation} {FileBodyType} body for request {RequestId}", "writing", "request",
					requestId);
				throw;
			}
		}

		/// <inheritdoc />
		public async Task<long> StoreResponseBodyAsync(Guid requestId, Stream bodyStream, CancellationToken cancellationToken = default)
		{
			try
			{
				_logger.LogTrace("{FileOperation} {FileBodyType} body for request {RequestId}", "Writing", "response", requestId);
				var size = await StoreBodyAsync(BuildResponseFilePath(requestId), bodyStream, cancellationToken);

				_logger.LogDebug("Writing of {FileBodyType} body for request {RequestId} completed with {BodySize} bytes", "response",
					requestId, size);
				return size;
			}
			catch (OperationCanceledException)
			{
				await RemoveResponseBodyAsync(requestId, CancellationToken.None);
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while {FileOperation} {FileBodyType} body for request {RequestId}", "writing",
					"response", requestId);
				throw;
			}
		}

		/// <inheritdoc />
		public Task<Stream> OpenRequestBodyAsync(Guid requestId, CancellationToken cancellationToken = default)
		{
			try
			{
				_logger.LogDebug("{FileOperation} {FileBodyType} body for request {RequestId}", "Reading", "request", requestId);
				return Task.FromResult(File.OpenRead(BuildRequestFilePath(requestId)) as Stream);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while {FileOperation} {FileBodyType} body for request {RequestId}", "reading",
					"request", requestId);
				throw;
			}
		}

		/// <inheritdoc />
		public Task<Stream> OpenResponseBodyAsync(Guid requestId, CancellationToken cancellationToken = default)
		{
			try
			{
				_logger.LogDebug("{FileOperation} {FileBodyType} body for request {RequestId}", "Reading", "response", requestId);
				return Task.FromResult(File.OpenRead(BuildResponseFilePath(requestId)) as Stream);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while {FileOperation} {FileBodyType} body for request {RequestId}", "reading",
					"response", requestId);
				throw;
			}
		}

		/// <inheritdoc />
		public Task RemoveRequestBodyAsync(Guid requestId, CancellationToken cancellationToken = default)
		{
			try
			{
				_logger.LogDebug("{FileOperation} {FileBodyType} body for request {RequestId}", "Deleting", "request", requestId);
				File.Delete(BuildRequestFilePath(requestId));
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while {FileOperation} {FileBodyType} body for request {RequestId}", "deleting",
					"request", requestId);
				throw;
			}
		}

		/// <inheritdoc />
		public Task RemoveResponseBodyAsync(Guid requestId, CancellationToken cancellationToken = default)
		{
			try
			{
				_logger.LogDebug("{FileOperation} {FileBodyType} body for request {RequestId}", "Deleting", "response", requestId);
				File.Delete(BuildResponseFilePath(requestId));
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while {FileOperation} {FileBodyType} body for request {RequestId}", "deleting",
					"response", requestId);
				throw;
			}
		}

		/// <inheritdoc />
		public IAsyncDisposable GetRequestBodyRemoveDisposable(Guid requestId) => new DisposeAction(() => RemoveRequestBodyAsync(requestId));

		/// <inheritdoc />
		public IAsyncDisposable GetResponseBodyRemoveDisposable(Guid requestId)
			=> new DisposeAction(() => RemoveResponseBodyAsync(requestId));

		private string BuildRequestFilePath(Guid id)
			=> BuildFilePath("req_", id);

		private string BuildResponseFilePath(Guid id)
			=> BuildFilePath("res_", id);

		private string BuildFilePath(string prefix, Guid id)
			=> Path.Combine(_basePath, $"{prefix}{id:D}");

		private async Task<long> StoreBodyAsync(string fileName, Stream bodyStream, CancellationToken cancellationToken)
		{
			bodyStream.TryRewind();

			await using var fs = File.OpenWrite(fileName);
			await bodyStream.CopyToAsync(fs, cancellationToken);

			return fs.Length;
		}
	}

	/// <summary>
	/// Configuration options for the <see cref="FileBodyStore"/>.
	/// </summary>
	public class FileBodyStoreOptions
	{
		/// <summary>
		/// The path where to store files.
		/// </summary>
		/// <remarks>The default value is the system temporary path.</remarks>
		public string StoragePath { get; set; } = Path.GetTempPath();
	}

	internal class FileBodyStoreValidateOptions : IValidateOptions<FileBodyStoreOptions>
	{
		private readonly ILogger<FileBodyStoreValidateOptions> _logger;

		public FileBodyStoreValidateOptions(ILogger<FileBodyStoreValidateOptions> logger)
			=> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

		public ValidateOptionsResult Validate(string name, FileBodyStoreOptions options)
		{
			if (string.IsNullOrWhiteSpace(options.StoragePath)) return ValidateOptionsResult.Fail("Storage path must be set.");

			if (!Directory.Exists(options.StoragePath)) return ValidateOptionsResult.Fail("Configured path does not exist.");

			if (!CheckForFilePermissions(options.StoragePath))
				return ValidateOptionsResult.Fail("Not able to create, write, read and delete files on configured path.");

			return ValidateOptionsResult.Success;
		}

		private bool CheckForFilePermissions(string basePath)
		{
			var fileName = Guid.NewGuid();
			var path = Path.Combine(basePath, $"test_{fileName:D}");

			try
			{
				using (var fs = File.OpenWrite(path))
				{
					fs.WriteByte(0xff);
				}

				using (var fs = File.OpenRead(path))
				{
					fs.ReadByte();
				}

				File.Delete(path);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while checking file creation, read and write permission on configured body store path {Path}",
					basePath);
				return false;
			}
		}
	}
}
