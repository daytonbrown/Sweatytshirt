using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sweaty_T_Shirt.Models;
using System.Data.Objects;
using System.Data.Entity.Infrastructure;

namespace Sweaty_T_Shirt.DAL
{
    public class BaseRepository : IDisposable
    {
        protected SweatyTShirtContext _context = null;
        private bool _isLocalContext = false;
        
        /// <summary>
        /// Base class will create SweatyTShirtContext and will call dispose on it
        /// when this object is disposed.
        /// </summary>
        public BaseRepository() 
        {
            _context = new SweatyTShirtContext();
            _isLocalContext = true;
        }

        /// <summary>
        /// Base class will NOT create SweatyTShirtContext and will NOT 
        /// call Dispose on sweatyTShirtContext when this object is disposed.
        /// </summary>
        /// <param name="sweatyTShirtContext"></param>
        public BaseRepository(SweatyTShirtContext sweatyTShirtContext)
        {
            _context = sweatyTShirtContext;
            _isLocalContext = false;
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_isLocalContext && !this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}