﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion.Backplane;

namespace ZiggyCreatures.Caching.Fusion.Internals.Backplane
{
	internal partial class BackplaneAccessor
	{
		private async ValueTask ExecuteOperationAsync(string operationId, string key, Func<CancellationToken, Task> action, string actionDescription, FusionCacheEntryOptions options, CancellationToken token)
		{
			if (IsCurrentlyUsable(operationId, key) == false)
				return;

			token.ThrowIfCancellationRequested();

			var actionDescriptionInner = actionDescription + (options.AllowBackgroundBackplaneOperations ? " (background)" : null);

			if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
				_logger.LogDebug("FUSION (O={CacheOperationId} K={CacheKey}): " + actionDescriptionInner, operationId, key);

			await FusionCacheExecutionUtils
				.RunAsyncActionAdvancedAsync(
					action,
					Timeout.InfiniteTimeSpan,
					false,
					options.AllowBackgroundBackplaneOperations == false,
					exc => ProcessError(operationId, key, exc, actionDescriptionInner),
					false,
					token
				)
				.ConfigureAwait(false)
			;
		}

		public async ValueTask<bool> PublishAsync(string operationId, BackplaneMessage message, FusionCacheEntryOptions options, CancellationToken token)
		{
			if (IsCurrentlyUsable(operationId, message.CacheKey) == false)
				return false;

			if (string.IsNullOrEmpty(message.SourceId))
			{
				// AUTO-ASSIGN LOCAL SOURCE ID
				message.SourceId = _cache.InstanceId;
			}
			else if (message.SourceId != _cache.InstanceId)
			{
				// IGNORE MESSAGES -NOT- FROM THIS SOURCE
				if (_logger?.IsEnabled(LogLevel.Warning) ?? false)
					_logger.Log(LogLevel.Warning, "FUSION (O={CacheOperationId} K={CacheKey}): cannot send a backplane message with a SourceId different than the local one (IFusionCache.InstanceId)", operationId, message.CacheKey);

				return false;
			}

			if (message.IsValid() == false)
			{
				// IGNORE INVALID MESSAGES
				if (_logger?.IsEnabled(LogLevel.Warning) ?? false)
					_logger.Log(LogLevel.Warning, "FUSION (O={CacheOperationId} K={CacheKey}): cannot send an invalid backplane message", operationId, message.CacheKey);

				return false;
			}

			token.ThrowIfCancellationRequested();

			await ExecuteOperationAsync(
				operationId,
				message.CacheKey!,
				async ct =>
				{
					await _backplane.PublishAsync(message, options, ct).ConfigureAwait(false);

					// EVENT
					_events.OnMessagePublished(operationId, message);
				},
				"sending backplane notification",
				options,
				token
			).ConfigureAwait(false);

			return true;
		}
	}
}
