using Microsoft.EntityFrameworkCore;
using SensorCloud.modules;
using System.Threading.Tasks;

namespace SensorCloud.datamodel
{
	public class SensorCloudContext : DbContext
	{
		DbModule module;
		public DbSet<Sensor> sensors { get; set; }
		public DbSet<Node> nodes { get; set; }
		public DbSet<Room> rooms { get; set; }
		public DbSet<Ping> pings { get; set; }
		public DbSet<SensorData> sensordata { get; set; }
        public DbSet<DashboardItem> dashboardItems { get; set; }
        public DbSet<DashboardCard> dashboardCards { get; set; }

        public SensorCloudContext(DbModule module)
		{
			this.module = module;
		}


		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			module.OnConfiguring(optionsBuilder);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Node>(node =>
			{
				node.HasKey(e => e.id);
				node.Property(e => e.hwid).IsRequired();
				node.HasMany(e => e.sensors).WithOne(s => s.node);
				node.HasOne(e => e.Room).WithMany(r => r.nodes);
			});

			modelBuilder.Entity<Sensor>(entity =>
			{
				entity.HasKey(e => e.id);
				entity.HasOne(s => s.node)
				  .WithMany(n => n.sensors);
			});

			modelBuilder.Entity<Room>(entity =>
			{
				entity.HasKey(e => e.id);
			});
			modelBuilder.Entity<Ping>(entity =>
			{
				entity.HasKey(e => e.id);
			});
			modelBuilder.Entity<SensorData>(entity =>
			{
				entity.HasKey(e => e.id);
			});

            modelBuilder.Entity<DashboardCard>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.HasMany(e => e.items).WithOne(i => i.card);
            });

            modelBuilder.Entity<DashboardItem>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.HasOne(e => e.card).WithMany(c => c.items);
            });


        }
	}
}
