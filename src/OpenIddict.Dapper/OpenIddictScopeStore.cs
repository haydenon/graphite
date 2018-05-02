using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenIddict.Core;
using OpenIddict.Models;

namespace OpenIddict.Dapper
{
    public class OpenIddictScopeStore : IOpenIddictScopeStore<OpenIddictScope>
    {
        public Task<long> CountAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<long> CountAsync<TResult>(Func<IQueryable<OpenIddictScope>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(OpenIddictScope scope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(OpenIddictScope scope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<OpenIddictScope> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<OpenIddictScope> FindByNameAsync(string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ImmutableArray<OpenIddictScope>> FindByNamesAsync(ImmutableArray<string> names, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> GetAsync<TState, TResult>(Func<IQueryable<OpenIddictScope>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDescriptionAsync(OpenIddictScope scope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDisplayNameAsync(OpenIddictScope scope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetIdAsync(OpenIddictScope scope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNameAsync(OpenIddictScope scope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<JObject> GetPropertiesAsync(OpenIddictScope scope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ImmutableArray<string>> GetResourcesAsync(OpenIddictScope scope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<OpenIddictScope> InstantiateAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ImmutableArray<OpenIddictScope>> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ImmutableArray<TResult>> ListAsync<TState, TResult>(Func<IQueryable<OpenIddictScope>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetDescriptionAsync(OpenIddictScope scope, string description, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetDisplayNameAsync(OpenIddictScope scope, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNameAsync(OpenIddictScope scope, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetPropertiesAsync(OpenIddictScope scope, JObject properties, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetResourcesAsync(OpenIddictScope scope, ImmutableArray<string> resources, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(OpenIddictScope scope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
