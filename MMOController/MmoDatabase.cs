using System;
using FluentNHibernate.Cfg;
using MMOController.Model.Accounts;
using NHibernate.Tool.hbm2ddl;
using NHibernate;
using System.Collections.Generic;

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
		public static void Save<T>(T instance){
			using(var session = sessionFactory.OpenSession()){
				using(var transaction = session.BeginTransaction()){
					transaction.
					transaction.Commit();
				}
			}
		}

		/// <summary>
		/// Commit changes to an array of objects in one transaction.
		/// </summary>
		/// <param name="objects">Objects to save.</param>
		/// <typeparam name="T">Type of object you're committing<c>/typeparam>
		public static void Save<T>(T[] objects){
			using(var session = sessionFactory.OpenSession()){
				using(var transaction = session.BeginTransaction()){
					foreach(var obj in objects){
						transaction.SaveOrUpdate(instance);
					}	
					transaction.Commit();
				}
			}
		}

		/// <summary>
		/// Remove an object from the database.
		/// </summary>
		/// <param name="instance">Instance to remove.</param>
		/// <typeparam name="T">Type you are removing.</typeparam>
		public static void Remove<T>(T instance){
			using(var session = sessionFactory.OpenSession()){
				using(var transaction = session.BeginTransaction()){
					transaction.Delete(instance);
					transaction.Commit();
				}
			}
		}

		/// <summary>
		/// Lists all of the specified type in database.
		/// </summary>
		/// <typeparam name="T">The type to look for.</typeparam>
		public static ICollection<T> List<T>(){
			using(var session = sessionFactory.OpenSession()){
				using(var transaction = session.BeginTransaction()){
					return transaction.CreateCriteria(typeof(T)).List<T>();
				}
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
					.ExposeConfiguration(((NHibernate.Cfg.Configuration obj) => {
						new SchemaUpdate(config).Execute(false, true);
					}))
					.BuildSessionFactory();
		}
	}
}

