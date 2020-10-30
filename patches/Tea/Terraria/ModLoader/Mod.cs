using System;
using System.Reflection;

namespace Terraria.ModLoader
{
	/// <summary>
	/// The <c>Mod</c> class. This is for a lot of core stuff.
	/// </summary>
	public abstract class Mod {
		public struct Properties
		{
			public string InternalName;
			public string DisplayName;
			public string Description;
			public Version Version;
			public string[] DLLReferences;
		}

		private string _internalName;
		private string _displayName;
		private string _description;
		private Version _version;
		private string[] _dllReferences;
		private Properties _modProperties;

		internal string file;
		internal Assembly code;

		public Properties ModProperties => _modProperties;

		public string InternalName => _internalName;

		public string DisplayName => _displayName;

		public string Description => _description;

		public Version Version => _version;

		/// <summary>
		/// Allows you to set your mod's display name, description, and version.
		/// </summary>
		/// /// <param name="internalName">Your mod's internal name. Should match your Mod.cs name, namespace name, mod sources folder name, etc.</param>
		/// <param name="displayName">Your mod's display name.</param>
		/// <param name="description">The description of your mod.</param>
		/// <param name="version">The version of your mod.</param>
		public abstract void SetModProperties(out string internalName, out string displayName, out string description, out Version version, out string[] dllReferences);

		/// <summary>
		/// Allows you to do things right before mod content is autoloaded.
		/// <para>Called before <c>Load()</c> and <c>PostAutoload()</c>.</para>
		/// </summary>
		public virtual void PreAutoload() {
		}

		/// <summary>
		/// Allows you to do things right after mod content is autoloaded.
		/// <para>Called before <c>Load()</c> and after <c>PreAutoload()</c>.</para>
		/// </summary>
		public virtual void PostAutoload() {
		}


		/// <summary>
		/// Allows you to do things when your mod loads.
		/// <para>Called after <c>Autoload()</c>.</para>
		/// </summary>
		public virtual void Load() {
		}

		#region Internal Stuff
		internal void Initialize() {
			SetModProperties(out _internalName, out _displayName, out _description, out _version, out _dllReferences);

			_modProperties = new Properties();
			_modProperties.DisplayName = _displayName;
			_modProperties.Description = _description;
			_modProperties.Version = _version;
			_modProperties.DLLReferences = _dllReferences;
		}

		internal void AutoloadInternal() {
			PreAutoload();
			PostAutoload();
		}

		internal void SetupContent() {
		}
		#endregion
	}
}
