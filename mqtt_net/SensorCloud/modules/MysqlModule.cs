using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SensorCloud.modules
{
	public class MysqlModule : Module
	{
        public string host { get; private set; }
        public int port { get; private set; }
        public string user { get; private set; }
        public string pass { get; private set; }
        public string daba { get; private set; }

        public MysqlModule(string host, int port, string user, string pass, string daba)
        {
            this.host = host;
            this.port = port;
            this.user = user;
            this.pass = pass;
            this.daba = daba;
        }

        public SensorCloudContext context { get { return new SensorCloudContext(this); } }
		public override void Start()
		{
/*			context = new SensorCloudContext();
			foreach(var node in context.nodes.Include(node => node.Room).Include(node => node.sensors))
			{
				Log(node.ToString());
			}*/
		}
	}


	public class Node
	{
		public int id { get; set; }
		public string hwid { get; set; }
		public string name { get; set; }
		public string topic { get; set; }

		[Column("room")]
		public int roomId { get; set; }
		[ForeignKey("roomId")]
		public Room Room { get; set; }

		public string config { get; set; }
		public virtual ICollection<Sensor> sensors { get; set; }

		public override string ToString()
		{
			return $"Node({id}, {hwid}, {name}, {Room?.name}, {sensors?.Count} sensors";
		}
	}

	public class Sensor
	{
		public int id { get; set; }
		public int nodeid { get; set; }

		[ForeignKey("nodeid")]
		public Node node { get; set; }
		public int type { get; set; }
		public string config { get; set; }
	}
	public class Room
	{
		public int id { get; set; }
		public string name { get; set; }
		public string topic { get; set; }
		public virtual ICollection<Node> nodes { get; set; }
	}

    public class Ping
    {
        public int id { get; set; }
        public DateTime stamp { get; set; }
        public int nodeid { get; set; }
        public string ip { get; set; }
        public int heapspace { get; set; }
        public int rssi { get; set; }
    }

    public class SensorData
    {
        public int id { get; set; }
        public DateTime stamp { get; set; }
        public int nodeid { get; set; }
        public string type { get; set; }
        public double value { get; set; }
    }

    [Table("dashboard_items")]
    public class DashboardItem
    {
        public int id { get; set; }
        public int cardid { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string parameter { get; set; }
        public DateTime time { get; set; }
        public string value { get; set; }
    }

    public class SensorCloudContext : DbContext
	{
        private MysqlModule module;

		public DbSet<Sensor> sensors { get; set; }
		public DbSet<Node> nodes { get; set; }
        public DbSet<Room> rooms { get; set; }
        public DbSet<Ping> pings { get; set; }
        public DbSet<SensorData> sensordata { get; set; }
        public DbSet<DashboardItem> dashboardItems { get; set; }

        public SensorCloudContext(MysqlModule module)
        {
            this.module = module;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseMySQL($"server={module.host};database={module.daba};user={module.user};password={module.pass}");
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

            modelBuilder.Entity<DashboardItem>(entity =>
            {
                entity.HasKey(e => e.id);
            });
        }
    }
}
