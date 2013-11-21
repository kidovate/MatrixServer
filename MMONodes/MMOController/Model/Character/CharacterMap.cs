using System;
using FluentNHibernate.Mapping;

namespace MMOController.Model.Character
{
	/// <summary>
	/// Mapping the <see cref="Character"/> to the database. 
	/// </summary>
	public class CharacterMap : ClassMap<Character>
	{
		public CharacterMap ()
		{
			Id(x=>x.Id);
		 	Map(x=>x.Name)
        .Unique();
		  Map(x => x.Gender);
			Map(x=>x.XP);
			References(x=>x.User);
		    References(x => x.CurrentRealm);
		}
	}
}

