﻿using System;
using System.Buffers.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis
{
	/// <summary>
	/// A Redis based implementation of a FusionCache backplane.
	/// </summary>
	public class RedisBackplane
		: IFusionCacheBackplane
	{
		private static readonly Encoding _encoding = Encoding.UTF8;
		private readonly RedisBackplaneOptions _options;
		private BackplaneSubscriptionOptions? _subscriptionOptions;
		private readonly SemaphoreSlim _connectionLock;
		private readonly ILogger? _logger;
		private IConnectionMultiplexer? _connection;
		private ISubscriber? _subscriber;
		private RedisChannel _channel;
		private Action<BackplaneMessage>? _handler;

		/// <summary>
		/// Initializes a new instance of the RedisBackplanePlugin class.
		/// </summary>
		/// <param name="optionsAccessor">The set of options to use with this instance of the backplane.</param>
		/// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance to use. If null, logging will be completely disabled.</param>
		public RedisBackplane(IOptions<RedisBackplaneOptions> optionsAccessor, ILogger<RedisBackplane>? logger = null)
		{
			if (optionsAccessor is null)
				throw new ArgumentNullException(nameof(optionsAccessor));

			// OPTIONS
			_options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(optionsAccessor.Value));

			// LOGGING
			if (logger is NullLogger<RedisBackplaneOptions>)
			{
				// IGNORE NULL LOGGER (FOR BETTER PERF)
				_logger = null;
			}
			else
			{
				_logger = logger;
			}

			_connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
		}

		private ConfigurationOptions GetConfigurationOptions()
		{
			if (_options.ConfigurationOptions is null && string.IsNullOrWhiteSpace(_options.Configuration))
				throw new InvalidOperationException("Unable to connect to Redis: no Configuration nor ConfigurationOptions have been specified");

			return _options.ConfigurationOptions ?? ConfigurationOptions.Parse(_options.Configuration);
		}

		private void EnsureConnection()
		{
			if (_connection is object)
				return;

			_connectionLock.Wait();
			try
			{
				if (_connection is object)
					return;

				_connection = ConnectionMultiplexer.Connect(GetConfigurationOptions());
			}
			finally
			{
				_connectionLock.Release();
			}

			if (_connection is null)
				throw new NullReferenceException("A connection to Redis is not available");

			OnAfterConnect();
		}

		private async ValueTask EnsureConnectionAsync(CancellationToken token = default)
		{
			token.ThrowIfCancellationRequested();

			if (_connection is object)
				return;

			await _connectionLock.WaitAsync(token).ConfigureAwait(false);
			try
			{
				if (_connection is object)
					return;

				_connection = await ConnectionMultiplexer.ConnectAsync(GetConfigurationOptions()).ConfigureAwait(false);
			}
			finally
			{
				_connectionLock.Release();
			}

			if (_connection is null)
				throw new NullReferenceException("A connection to Redis is not available");

			OnAfterConnect();
		}

		private void OnAfterConnect()
		{
			if (_subscriber is null)
				_subscriber = _connection!.GetSubscriber();
		}

		private void Disconnect()
		{
			if (_connection is null)
				return;

			try
			{
				_connection.Dispose();
			}
			catch (Exception exc)
			{
				if (_logger?.IsEnabled(LogLevel.Error) ?? false)
					_logger.Log(LogLevel.Error, exc, "An error occurred while disconnecting from Redis");
			}

			_connection = null;
		}

		/// <inheritdoc/>
		public void Subscribe(BackplaneSubscriptionOptions subscriptionOptions)
		{
			if (subscriptionOptions is null)
				throw new ArgumentNullException(nameof(subscriptionOptions));

			if (subscriptionOptions.ChannelName is null)
				throw new NullReferenceException("The BackplaneSubscriptionOptions.ChannelName cannot be null");

			if (subscriptionOptions.Handler is null)
				throw new NullReferenceException("The BackplaneSubscriptionOptions.Handler cannot be null");

			_subscriptionOptions = subscriptionOptions;

			_channel = _subscriptionOptions.ChannelName;
			_handler = _subscriptionOptions.Handler;

			_ = Task.Run(async () =>
			{
				// CONNECTION
				await EnsureConnectionAsync().ConfigureAwait(false);

				await _subscriber!.SubscribeAsync(_channel, (_, v) =>
				{
					var message = FromRedisValue(v, _logger);
					if (message is object)
					{
						_handler(message);
					}
				}).ConfigureAwait(false);
			});
		}

		/// <inheritdoc/>
		public void Unsubscribe()
		{
			_ = Task.Run(() => Disconnect());
		}

		/// <inheritdoc/>
		public async ValueTask PublishAsync(BackplaneMessage message, FusionCacheEntryOptions options, CancellationToken token)
		{
			await EnsureConnectionAsync().ConfigureAwait(false);

			var v = ToRedisValue(message, _logger);

			if (v.IsNull)
				return;

			//try
			//{
			var receivedCount = await _subscriber!.PublishAsync(_channel, v).ConfigureAwait(false);
			if (receivedCount == 0)
			{
				if (_logger?.IsEnabled(LogLevel.Error) ?? false)
					_logger.Log(LogLevel.Error, "An error occurred while trying to send a notification to the Redis backplane");

				return;
			}

			if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
				_logger.Log(LogLevel.Debug, "An eviction notification has been sent for {CacheKey}", message.CacheKey);
			//}
			//catch (Exception exc)
			//{
			//	if (_logger?.IsEnabled(LogLevel.Error) ?? false)
			//		_logger.Log(LogLevel.Error, exc, "An error occurred while trying to send a notification to the Redis backplane");
			//}
		}

		/// <inheritdoc/>
		public void Publish(BackplaneMessage message, FusionCacheEntryOptions options)
		{
			EnsureConnection();

			var v = ToRedisValue(message, _logger);

			if (v.IsNull)
				return;

			//try
			//{
			var receivedCount = _subscriber!.Publish(_channel, v);
			if (receivedCount == 0)
			{
				if (_logger?.IsEnabled(LogLevel.Error) ?? false)
					_logger.Log(LogLevel.Error, "An error occurred while trying to send a notification to the Redis backplane");

				return;
			}

			if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
				_logger.Log(LogLevel.Debug, "An eviction notification has been sent for {CacheKey}", message.CacheKey);
			//}
			//catch (Exception exc)
			//{
			//	if (_logger?.IsEnabled(LogLevel.Error) ?? false)
			//		_logger.Log(LogLevel.Error, exc, "An error occurred while trying to send a notification to the Redis backplane");
			//}
		}

		private static BackplaneMessage? FromRedisValue(RedisValue value, ILogger? logger)
		{
			try
			{
				byte[] data = value;
				var pos = 0;
				var res = new BackplaneMessage();

				// VERSION
				var version = data[pos];
				if (version != 0)
				{
					if (logger?.IsEnabled(LogLevel.Warning) ?? false)
						logger.Log(LogLevel.Warning, "The version header does not have the expected value of 0 (zero): instead the value is " + version);
					return null;
				}
				pos++;

				// SOURCE ID
				var tmp = BinaryPrimitives.ReadInt32LittleEndian(new ReadOnlySpan<byte>(data, pos, 4));
				pos += 4;
				res.SourceId = _encoding.GetString(data, pos, tmp);
				pos += tmp;

				// INSTANT TICKS
				res.InstantTicks = BinaryPrimitives.ReadInt64LittleEndian(new ReadOnlySpan<byte>(data, pos, 8));
				pos += 8;

				// ACTION
				res.Action = (BackplaneMessageAction)data[pos];
				pos++;

				// CACHE KEY
				tmp = BinaryPrimitives.ReadInt32LittleEndian(new ReadOnlySpan<byte>(data, pos, 4));
				pos += 4;
				res.CacheKey = _encoding.GetString(data, pos, tmp);
				//pos += tmp;

				return res;
			}
			catch (Exception exc)
			{
				if (logger?.IsEnabled(LogLevel.Warning) ?? false)
					logger.Log(LogLevel.Warning, exc, "An error occurred while converting a RedisValue into a BackplaneMessage");
			}

			return null;
		}

		private static RedisValue ToRedisValue(BackplaneMessage message, ILogger? logger)
		{
			try
			{
				var sourceIdByteCount = _encoding.GetByteCount(message.SourceId);
				var cacheKeyByteCount = _encoding.GetByteCount(message.CacheKey);

				var size =
					1 // VERSION
					+ 4 + sourceIdByteCount // SOURCE ID
					+ 8 // INSTANCE TICKS
					+ 1 // ACTION
					+ 4 + cacheKeyByteCount // CACHE KEY
				;

				var pos = 0;
				var res = new byte[size];

				// VERSION
				res[pos] = 0;
				pos++;

				// SOURCE ID
				BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>(res, pos, 4), sourceIdByteCount);
				pos += 4;
				_encoding.GetBytes(message.SourceId!, 0, message.SourceId!.Length, res, pos);
				pos += sourceIdByteCount;

				// INSTANT TICKS
				BinaryPrimitives.WriteInt64LittleEndian(new Span<byte>(res, pos, 8), message.InstantTicks);
				pos += 8;

				// ACTION
				res[pos] = (byte)message.Action;
				pos++;

				// CACHE KEY
				BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>(res, pos, 4), cacheKeyByteCount);
				pos += 4;
				_encoding.GetBytes(message.CacheKey, 0, message.CacheKey!.Length, res, pos);
				//pos += cacheKeyByteCount;

				return res;
			}
			catch (Exception exc)
			{
				if (logger?.IsEnabled(LogLevel.Warning) ?? false)
					logger.Log(LogLevel.Warning, exc, "An error occurred while converting a BackplaneMessage into a RedisValue");
			}

			return RedisValue.Null;
		}
	}
}
