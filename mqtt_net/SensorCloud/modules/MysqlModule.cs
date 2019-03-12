using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SensorCloud.modules
{
	public class MysqlModule : DbModule
	{
		private string configString;

		public MysqlModule(string host, int port, string user, string pass, string daba)
		{
			configString = $"server={host};database={daba};user={user};password={pass}";
		}
		public override void Start()
		{
		}

		public override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseMySQL(configString);
		}
	}





}
