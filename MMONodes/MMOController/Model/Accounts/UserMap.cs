using System;
using FluentNHibernate.Mapping;

namespace MMOController.Model.Accounts
{
	/// <summary>
	/// Mapping the <see cref="User"/> to the database. 
	/// </summary>
	public class UserMap : ClassMap<User>
	{
		public UserMap ()
		{
			Id(x=>x.Id);
			Map(x=>x.Password);
		    Map(x => x.Username)
		        .Unique()
		        .ReadOnly();
			Map(x=>x.Role);
		}
	}
}

