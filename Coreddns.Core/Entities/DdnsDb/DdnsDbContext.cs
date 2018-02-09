using Microsoft.EntityFrameworkCore;

namespace Coreddns.Core.Entities.DdnsDb
{
    // dotnet core CLI によるデータベース更新のためのコード生成
    //   dotnet ef migrations add <MigrationName>
    // データベースへの反映
    //   dotnet ef database update
    // SQLを生成する方法、あるいは プログラムの実行時に更新する方法もある
    public class DdnsDbContext : DbContext
    {
        public DdnsDbContext(DbContextOptions<DdnsDbContext> options)
            : base(options)
        { }

        public DbSet<ddnshost> ddnshost { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ddnshost>()
                .HasIndex(x => new { x.name, x.isvalid });

            modelBuilder.Entity<ddnshost>()
                .HasIndex(x => new { x.hash, x.isvalid })
                .IsUnique();
        }
    }
}
