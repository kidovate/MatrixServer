using System;
using FluentNHibernate.Cfg;
using MMOController.Model.Accounts;

namespace MMOController
{
	/// <summary>
	/// Manages the transactions with the database through Fluent NHibernate.
	/// </summary>
	public static class MmoDatabase
	{
		private static ISessionFactory sessionFactory;

		static MmoDatabase ()
		{
			sessionFactory = CreateSessionFactory();
		}

		/// <summary>
		/// Saves a mapped type T in the database.
		/// </summary>
		/// <typeparam name="T">The type you are serializing.</typeparam>
		public void Save<T>(T instance){
			using(var session = sessionFactory.CreateSession(){
			}
		}

		private static ISessionFactory CreateSessionFactory()
		{
			return Fluently.Configure()
					.Database(
						MySqlConfiguration.Standard.ConnectionString(
							c => c.FromConnectionStringWithKey("MoyaAws1")
						)
					)
					.Mappings(m =>
						m.FluentMappings.AddFromAssemblyOf<User>()
					)
					.BuildSessionFactory();
		}
	}
}

