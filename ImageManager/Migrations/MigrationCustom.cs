using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageManager.Migrations
{
    public abstract class MigrationCustom<T> : Migration where T : DbContext
    {
        public virtual void PreUp(T context) { }
        public virtual void PostUp(T context) { }
        public virtual void PreDown(T context) { }
        public virtual void PostDown(T context) { }
    }
}
