using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SensorCloud.datamodel;

namespace SensorCloud.modules
{
	public abstract class DbModule : Module
	{
		public SensorCloudContext context { get { return new SensorCloudContext(this); } }
		public abstract void OnConfiguring(DbContextOptionsBuilder optionsBuilder);
	}
}
