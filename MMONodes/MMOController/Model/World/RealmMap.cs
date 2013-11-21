using System;
using FluentNHibernate.Mapping;

namespace MMOController
{
	public class RealmMap : ClassMap<Realm>
	{
		public RealmMap ()
		{
			Id(e=>e.Id);
			Map(e=>e.LevelName);
			Map(e=>e.WorldOriginX);
			Map(e=>e.WorldOriginY);
			Map(e=>e.WorldOriginZ);
			Map(x=>x.WorldSizeX);
			Map(x=>x.WorldSizeY);
		}
	}
}

