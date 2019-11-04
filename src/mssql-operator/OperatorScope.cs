using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MSSqlOperator
{
    public interface IOperatorScope : IDisposable
    {
        Task StartAsync(string watchedNamespace, CancellationToken token);
    }

    public class OperatorScope<TOperator> : IOperatorScope where TOperator : OperatorSharp.Operator
    {
        private readonly IServiceScope scope;
        public OperatorScope(IServiceProvider provider)
        {
            this.scope = provider.CreateScope();
        }

        public async Task StartAsync(string watchedNamespace, CancellationToken token)
        {
            var instance = scope.ServiceProvider.GetService<TOperator>();
            await instance.WatchAsync(token, watchedNamespace);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    scope.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

    }
}