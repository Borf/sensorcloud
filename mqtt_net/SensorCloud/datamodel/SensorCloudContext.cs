using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Threading.Tasks;

namespace SensorCloud.datamodel
{
	public class SensorCloudContext : DbContext
	{
        //public static readonly LoggerFactory MyLoggerFactory = new LoggerFactory(new[] { new ConsoleLoggerProvider((_, __) => true, true) });

        private Config config;

        public DbSet<Sensor> sensors { get; set; }
		public DbSet<Node> nodes { get; set; }
		public DbSet<Room> rooms { get; set; }
        public DbSet<Ping> pings { get; set; }
        public DbSet<Rule> rules { get; set; }
        public DbSet<SensorData> sensordata { get; set; }
        public DbSet<DashboardItem> dashboardItems { get; set; }
        public DbSet<DashboardCard> dashboardCards { get; set; }

        //spotnet service
        public DbSet<Spot> spots { get; set; }
        public DbSet<SpotNzb> spotNzbs { get; set; }

        //HG659 hosts
        public DbSet<HG659_host> HG569_hosts { get; set; }

        public SensorCloudContext(IConfiguration configuration)
		{
            config = new Config();
            configuration.GetSection("Db").Bind(config);
		}


		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
            if(config.mysql != null)
                optionsBuilder
                    .UseMySQL(config.mysql.configstring)
/*                    .UseLoggerFactory(MyLoggerFactory)
                      .EnableSensitiveDataLogging()
                      .EnableDetailedErrors()*/
                    ;
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

            modelBuilder.Entity<Rule>(entity =>
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

            modelBuilder.Entity<Spot>(entity =>
            {
                entity.HasKey(e => e.article);
                entity.HasMany(e => e.nzbs).WithOne(n => n.spot);
            });
            modelBuilder.Entity<SpotNzb>(entity =>
            {
                entity.HasKey(e => new { e.article, e.articleid, e.segment });
                entity.HasOne(e => e.spot).WithMany(s => s.nzbs);
            });

            modelBuilder.Entity<HG659_host>(entity =>
            {
                entity.HasKey(e => new { e.mac });
            });
        }
    }
}
