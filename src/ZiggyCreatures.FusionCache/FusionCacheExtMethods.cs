﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ZiggyCreatures.Caching.Fusion
{
	/// <summary>
	/// A set of extension methods that add some commonly used overloads to any instance of a <see cref="IFusionCache"/> object.
	/// </summary>
	public static partial class FusionCacheExtMethods
	{
		/// <summary>
		/// Creates a new <see cref="FusionCacheEntryOptions"/> instance by duplicating the DefaultEntryOptions and optionally applying a setup action.
		/// </summary>
		/// <param name="cache">The <see cref="FusionCache"/> instance.</param>
		/// <param name="setupAction">An optional setup action to further configure the newly created <see cref="FusionCacheEntryOptions"/> instance.</param>
		/// <param name="duration">An optional duration to directly change the <see cref="FusionCacheEntryOptions.Duration"/> of the newly created <see cref="FusionCacheEntryOptions"/> instance.</param>
		/// <param name="includeOptionsModifiers">Specify if options modifiers should be duplicated as well.</param>
		/// <returns>The newly created <see cref="FusionCacheEntryOptions"/>.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Please use the signature without the last bool param", true)]
		public static FusionCacheEntryOptions CreateEntryOptions(this IFusionCache cache, Action<FusionCacheEntryOptions>? setupAction, TimeSpan? duration, bool includeOptionsModifiers)
		{
			return cache.CreateEntryOptions(setupAction, duration);
		}

		#region GetOrSet overloads (with default value)

		/// <summary>
		/// Get the value of type <typeparamref name="TValue"/> in the cache for the specified <paramref name="key"/>: if not there, the <paramref name="defaultValue"/> will be saved according with the <paramref name="duration"/> provided.
		/// </summary>
		/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="defaultValue">In case the value is not in the cache this value will be saved and returned instead.</param>
		/// <param name="duration">The value for the newly created <see cref="FusionCacheEntryOptions.Duration"/> property, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		public static ValueTask<TValue> GetOrSetAsync<TValue>(this IFusionCache cache, string key, TValue defaultValue, TimeSpan duration, CancellationToken token = default)
		{
			return cache.GetOrSetAsync<TValue>(key, defaultValue, cache.DefaultEntryOptions.Duplicate(duration).SetIsSafeForAdaptiveCaching(), token);
		}

		/// <summary>
		/// Get the value of type <typeparamref name="TValue"/> in the cache for the specified <paramref name="key"/>: if not there, the <paramref name="defaultValue"/> will be saved according with the <paramref name="duration"/> provided.
		/// </summary>
		/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="defaultValue">In case the value is not in the cache this value will be saved and returned instead.</param>
		/// <param name="duration">The value for the newly created <see cref="FusionCacheEntryOptions.Duration"/> property, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		public static TValue GetOrSet<TValue>(this IFusionCache cache, string key, TValue defaultValue, TimeSpan duration, CancellationToken token = default)
		{
			return cache.GetOrSet<TValue>(key, defaultValue, cache.DefaultEntryOptions.Duplicate(duration).SetIsSafeForAdaptiveCaching(), token);
		}

		/// <summary>
		/// Get the value of type <typeparamref name="TValue"/> in the cache for the specified <paramref name="key"/>: if not there, the <paramref name="defaultValue"/> will be saved according with the <see cref="FusionCacheEntryOptions"/> resulting by calling the provided <paramref name="setupAction"/> lambda.
		/// </summary>
		/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="defaultValue">In case the value is not in the cache this value will be saved and returned instead.</param>
		/// <param name="setupAction">The setup action used to further configure the newly created <see cref="FusionCacheEntryOptions"/> object, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		public static ValueTask<TValue> GetOrSetAsync<TValue>(this IFusionCache cache, string key, TValue defaultValue, Action<FusionCacheEntryOptions> setupAction, CancellationToken token = default)
		{
			return cache.GetOrSetAsync<TValue>(key, defaultValue, cache.CreateEntryOptions(setupAction).SetIsSafeForAdaptiveCaching(), token);
		}

		/// <summary>
		/// Get the value of type <typeparamref name="TValue"/> in the cache for the specified <paramref name="key"/>: if not there, the <paramref name="defaultValue"/> will be saved according with the <see cref="FusionCacheEntryOptions"/> resulting by calling the provided <paramref name="setupAction"/> lambda.
		/// </summary>
		/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="defaultValue">In case the value is not in the cache this value will be saved and returned instead.</param>
		/// <param name="setupAction">The setup action used to further configure the newly created <see cref="FusionCacheEntryOptions"/> object, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		public static TValue GetOrSet<TValue>(this IFusionCache cache, string key, TValue defaultValue, Action<FusionCacheEntryOptions> setupAction, CancellationToken token = default)
		{
			return cache.GetOrSet<TValue>(key, defaultValue, cache.CreateEntryOptions(setupAction).SetIsSafeForAdaptiveCaching(), token);
		}

		#endregion

		#region GetOrDefault overloads

		/// <summary>
		/// Get the value of type <typeparamref name="TValue"/> in the cache for the specified <paramref name="key"/>: if not there, the <paramref name="defaultValue"/> will be returned.
		/// </summary>
		/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="defaultValue">The defualt value to return if the value for the given <paramref name="key"/> is not in the cache.</param>
		/// <param name="setupAction">The setup action used to further configure the newly created <see cref="FusionCacheEntryOptions"/> object, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		/// <returns>The value in the cache or the <paramref name="defaultValue"/> .</returns>
		public static ValueTask<TValue> GetOrDefaultAsync<TValue>(this IFusionCache cache, string key, Action<FusionCacheEntryOptions> setupAction, TValue defaultValue = default, CancellationToken token = default)
		{
			return cache.GetOrDefaultAsync<TValue>(key, defaultValue, cache.CreateEntryOptions(setupAction).SetIsSafeForAdaptiveCaching(), token);
		}

		/// <summary>
		/// Get the value of type <typeparamref name="TValue"/> in the cache for the specified <paramref name="key"/>: if not there, the <paramref name="defaultValue"/> will be returned.
		/// </summary>
		/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="defaultValue">The defualt value to return if the value for the given <paramref name="key"/> is not in the cache.</param>
		/// <param name="setupAction">The setup action used to further configure the newly created <see cref="FusionCacheEntryOptions"/> object, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		/// <returns>The value in the cache or the <paramref name="defaultValue"/> .</returns>
		public static TValue GetOrDefault<TValue>(this IFusionCache cache, string key, Action<FusionCacheEntryOptions> setupAction, TValue defaultValue = default, CancellationToken token = default)
		{
			return cache.GetOrDefault<TValue>(key, defaultValue, cache.CreateEntryOptions(setupAction).SetIsSafeForAdaptiveCaching(), token);
		}

		/// <summary>
		/// Get the value of type <typeparamref name="TValue"/> in the cache for the specified <paramref name="key"/>: if not there, the <paramref name="defaultValue"/> will be returned.
		/// </summary>
		/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="defaultValue">The defualt value to return if the value for the given <paramref name="key"/> is not in the cache.</param>
		/// <param name="setupAction">The setup action used to further configure the newly created <see cref="FusionCacheEntryOptions"/> object, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		/// <returns>The value in the cache or the <paramref name="defaultValue"/> .</returns>
		public static ValueTask<TValue> GetOrDefaultAsync<TValue>(this IFusionCache cache, string key, TValue defaultValue, Action<FusionCacheEntryOptions> setupAction, CancellationToken token = default)
		{
			return cache.GetOrDefaultAsync<TValue>(key, defaultValue, cache.CreateEntryOptions(setupAction).SetIsSafeForAdaptiveCaching(), token);
		}

		/// <summary>
		/// Get the value of type <typeparamref name="TValue"/> in the cache for the specified <paramref name="key"/>: if not there, the <paramref name="defaultValue"/> will be returned.
		/// </summary>
		/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="defaultValue">The defualt value to return if the value for the given <paramref name="key"/> is not in the cache.</param>
		/// <param name="setupAction">The setup action used to further configure the newly created <see cref="FusionCacheEntryOptions"/> object, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		/// <returns>The value in the cache or the <paramref name="defaultValue"/> .</returns>
		public static TValue GetOrDefault<TValue>(this IFusionCache cache, string key, TValue defaultValue, Action<FusionCacheEntryOptions> setupAction, CancellationToken token = default)
		{
			return cache.GetOrDefault<TValue>(key, defaultValue, cache.CreateEntryOptions(setupAction).SetIsSafeForAdaptiveCaching(), token);
		}

		#endregion

		#region TryGet overloads

		/// <summary>
		/// Try to get the value of type <typeparamref name="TValue"/> in the cache for the specified <paramref name="key"/> and returns a <see cref="MaybeValue{TValue}"/> instance.
		/// </summary>
		/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="setupAction">The setup action used to further configure the newly created <see cref="FusionCacheEntryOptions"/> object, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		public static ValueTask<MaybeValue<TValue>> TryGetAsync<TValue>(this IFusionCache cache, string key, Action<FusionCacheEntryOptions> setupAction, CancellationToken token = default)
		{
			return cache.TryGetAsync<TValue>(key, cache.CreateEntryOptions(setupAction).SetIsSafeForAdaptiveCaching(), token);
		}

		/// <summary>
		/// Try to get the value of type <typeparamref name="TValue"/> in the cache for the specified <paramref name="key"/> and returns a <see cref="MaybeValue{TValue}"/> instance.
		/// </summary>
		/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="setupAction">The setup action used to further configure the newly created <see cref="FusionCacheEntryOptions"/> object, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		public static MaybeValue<TValue> TryGet<TValue>(this IFusionCache cache, string key, Action<FusionCacheEntryOptions> setupAction, CancellationToken token = default)
		{
			return cache.TryGet<TValue>(key, cache.CreateEntryOptions(setupAction).SetIsSafeForAdaptiveCaching(), token);
		}

		#endregion

		#region Set overloads

		/// <summary>
		/// Put the <paramref name="value"/> in the cache for the specified <paramref name="key"/> with the provided <paramref name="duration"/>. If a value is already there it will be overwritten.
		/// </summary>
		/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="value">The value to put in the cache.</param>
		/// <param name="duration">The value for the newly created <see cref="FusionCacheEntryOptions.Duration"/> property, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		/// <returns>A <see cref="Task"/> to await the completion of the operation.</returns>
		public static ValueTask SetAsync<TValue>(this IFusionCache cache, string key, TValue value, TimeSpan duration, CancellationToken token = default)
		{
			return cache.SetAsync<TValue>(key, value, cache.DefaultEntryOptions.Duplicate(duration).SetIsSafeForAdaptiveCaching(), token);
		}

		/// <summary>
		/// Put the <paramref name="value"/> in the cache for the specified <paramref name="key"/> with the provided <paramref name="duration"/>. If a value is already there it will be overwritten.
		/// </summary>
		/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="value">The value to put in the cache.</param>
		/// <param name="duration">The value for the newly created <see cref="FusionCacheEntryOptions.Duration"/> property, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		public static void Set<TValue>(this IFusionCache cache, string key, TValue value, TimeSpan duration, CancellationToken token = default)
		{
			cache.Set<TValue>(key, value, cache.DefaultEntryOptions.Duplicate(duration).SetIsSafeForAdaptiveCaching(), token);
		}

		/// <summary>
		/// Put the <paramref name="value"/> in the cache for the specified <paramref name="key"/> with the <see cref="FusionCacheEntryOptions"/> resulting by calling the provided <paramref name="setupAction"/> lambda. If a value is already there it will be overwritten.
		/// </summary>
		/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="value">The value to put in the cache.</param>
		/// <param name="setupAction">The setup action used to further configure the newly created <see cref="FusionCacheEntryOptions"/> object, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		/// <returns>A <see cref="Task"/> to await the completion of the operation.</returns>
		public static ValueTask SetAsync<TValue>(this IFusionCache cache, string key, TValue value, Action<FusionCacheEntryOptions> setupAction, CancellationToken token = default)
		{
			return cache.SetAsync<TValue>(key, value, cache.CreateEntryOptions(setupAction).SetIsSafeForAdaptiveCaching(), token);
		}

		/// <summary>
		/// Put the <paramref name="value"/> in the cache for the specified <paramref name="key"/> with the <see cref="FusionCacheEntryOptions"/> resulting by calling the provided <paramref name="setupAction"/> lambda. If a value is already there it will be overwritten.
		/// </summary>
		/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="value">The value to put in the cache.</param>
		/// <param name="setupAction">The setup action used to further configure the newly created <see cref="FusionCacheEntryOptions"/> object, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		public static void Set<TValue>(this IFusionCache cache, string key, TValue value, Action<FusionCacheEntryOptions> setupAction, CancellationToken token = default)
		{
			cache.Set<TValue>(key, value, cache.CreateEntryOptions(setupAction).SetIsSafeForAdaptiveCaching(), token);
		}

		#endregion

		#region Remove overloads

		/// <summary>
		/// Removes the value in the cache for the specified <paramref name="key"/>.
		/// </summary>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="setupAction">The setup action used to further configure the newly created <see cref="FusionCacheEntryOptions"/> object, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		/// <returns>A <see cref="Task"/> to await the completion of the operation.</returns>
		public static ValueTask RemoveAsync(this IFusionCache cache, string key, Action<FusionCacheEntryOptions> setupAction, CancellationToken token = default)
		{
			return cache.RemoveAsync(key, cache.CreateEntryOptions(setupAction).SetIsSafeForAdaptiveCaching(), token);
		}

		/// <summary>
		/// Removes the value in the cache for the specified <paramref name="key"/>.
		/// </summary>
		/// <param name="cache">The <see cref="IFusionCache"/> instance.</param>
		/// <param name="key">The cache key which identifies the entry in the cache.</param>
		/// <param name="setupAction">The setup action used to further configure the newly created <see cref="FusionCacheEntryOptions"/> object, automatically created by duplicating <see cref="IFusionCache.DefaultEntryOptions"/>.</param>
		/// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
		public static void Remove(this IFusionCache cache, string key, Action<FusionCacheEntryOptions> setupAction, CancellationToken token = default)
		{
			cache.Remove(key, cache.CreateEntryOptions(setupAction).SetIsSafeForAdaptiveCaching(), token);
		}

		#endregion
	}
}
