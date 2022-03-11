using HotChocolate.Fetching;

namespace HotChocolateTest
{
    public class DataLoaderAccessor
    {
        private class FetchBatchDataLoader<TKey, TValue> : BatchDataLoader<TKey, TValue>
            where TKey : notnull
        {
            private readonly FetchBatch<TKey, TValue> _fetch;

            public FetchBatchDataLoader(
                FetchBatch<TKey, TValue> fetch,
                IBatchScheduler batchScheduler,
                DataLoaderOptions options)
                : base(batchScheduler, options)
            {
                _fetch = fetch ?? throw new ArgumentNullException(nameof(fetch));
            }

            protected override Task<IReadOnlyDictionary<TKey, TValue>> LoadBatchAsync(
                IReadOnlyList<TKey> keys,
                CancellationToken cancellationToken) =>
                _fetch(keys, cancellationToken);
        }

        IHttpContextAccessor contextAccessor;
        public DataLoaderAccessor(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public IDataLoader<TKey, TValue> BatchDataLoader<TKey, TValue>(
            FetchBatch<TKey, TValue> fetch,
            string? key = null)
        where TKey : notnull
        {
            var context = contextAccessor.HttpContext;
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (fetch is null)
            {
                throw new ArgumentNullException(nameof(fetch));
            }

            IServiceProvider services = context.RequestServices;
            IDataLoaderRegistry reg = services.GetRequiredService<IDataLoaderRegistry>();
            FetchBatchDataLoader<TKey, TValue> Loader()
                => new(
                    fetch,
                    services.GetRequiredService<IBatchScheduler>(),
                    services.GetRequiredService<DataLoaderOptions>());

            return key is null
                ? reg.GetOrRegister(Loader)
                : reg.GetOrRegister(key, Loader);
        }
    }
}
