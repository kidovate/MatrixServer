using System;
using MMOCommon;

namespace MMOController
{
	/// <summary>
	/// A realm is a single level in the game with a set of interest zones.
	/// 
	/// Interest zones are identified by RealmID+ZoneXIndex+ZoneYIndex
	/// </summary>
	public class Realm
	{
		/// <summary>
		/// The ID of this realm.
		/// </summary>
		/// <value>The identifier.</value>
		public virtual int Id {get;set;}

		/// <summary>
		/// The CryEngine level name corresponding to the realm.
		/// </summary>
		/// <value>The name of the level.</value>
		public virtual string LevelName {get;set;}

		/// <summary>
		/// The origin (0,0,0) of this realm in global coordinates
		/// </summary>
		/// <value>The world origin x.</value>
		public virtual int WorldOriginX {get; set;}

		/// <summary>
		/// The origin (0,0,0) of this realm in global coordinates
		/// </summary>
		/// <value>The world origin y.</value>
		public virtual int WorldOriginY {get;set;}

		/// <summary>
		/// The origin (0,0,0) of this realm in global coordinates
		/// </summary>
		/// <value>The world origin Z.</value>
		public virtual int WorldOriginZ {get;set;}

		/// <summary>
		/// Converts to or from a Vector3D with WorldOrigin
		/// </summary>
		/// <value>The world origin.</value>
		public Vector3D WorldOrigin {
			get{
				return new Vector3D(){ XPosition = this.WorldOriginX, YPosition = this.WorldOriginY, ZPosition = this.WorldOriginZ};
			}
			set{
				this.WorldOriginX = value.XPosition;
				this.WorldOriginY = value.YPosition;
				this.WorldOriginZ = value.ZPosition;
			}
		}

		/// <summary>
		/// The realm size in world coordinates (X).
		/// </summary>
		/// <value>The world size X.</value>
		public virtual int WorldSizeX {get;set;}

		/// <summary>
		/// The realm size in world coordinates (Y).
		/// </summary>
		/// <value>The world size y.</value>
		public virtual int WorldSizeY {get;set;}

		/// <summary>
		/// The realm tile size in X coordinates.
		/// </summary>
		/// <value>The tile count x.</value>
		public virtual int TileCountX {get;set;}

	}
}

