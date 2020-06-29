#if !UNITY_EDITOR
#define NOT_UNITY_EDITOR
#endif

namespace Mapbox.Unity.Location
{
	using UnityEngine;
	using Mapbox.Unity.Map;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Singleton factory to allow easy access to various LocationProviders.
	/// This is meant to be attached to a game object.
	/// </summary>
	public class LocationProviderFactory : MonoBehaviour
	{
		[SerializeField]
		public AbstractMap mapManager;

		[SerializeField]
		AbstractLocationProvider _editorLocationProvider;

		[SerializeField]
		AbstractLocationProvider _transformLocationProvider;

		[SerializeField]
		bool _dontDestroyOnLoad;

        public static LocationProviderFactory Instance { get; private set; }

        /// <summary>
        /// The default location provider. 
        /// Outside of the editor, this will be a <see cref="T:Mapbox.Unity.Location.DeviceLocationProvider"/>.
        /// In the Unity editor, this will be an <see cref="T:Mapbox.Unity.Location.EditorLocationProvider"/>
        /// </summary>
        /// <example>
        /// Fetch location to set a transform's position:
        /// <code>
        /// void Update()
        /// {
        ///     var locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        ///     transform.position = Conversions.GeoToWorldPosition(locationProvider.Location,
        ///                                                         MapController.ReferenceTileRect.Center,
        ///                                                         MapController.WorldScaleFactor).ToVector3xz();
        /// }
        /// </code>
        /// </example>
        public ILocationProvider DefaultLocationProvider { get; set; }

        /// <summary>
        /// Returns the serialized <see cref="T:Mapbox.Unity.Location.TransformLocationProvider"/>.
        /// </summary>
        public ILocationProvider TransformLocationProvider
		{
			get
			{
				return _transformLocationProvider;
			}
		}

		/// <summary>
		/// Returns the serialized <see cref="T:Mapbox.Unity.Location.EditorLocationProvider"/>.
		/// </summary>
		public ILocationProvider EditorLocationProvider
		{
			get
			{
				return _editorLocationProvider;
			}
		}

		/// <summary>
		/// Create singleton instance and inject the DefaultLocationProvider upon initialization of this component. 
		/// </summary>
		protected virtual void Awake()
		{
			if (Instance != null)
			{
				DestroyImmediate(gameObject);
				return;
			}
			Instance = this;

			if (_dontDestroyOnLoad)
			{
				DontDestroyOnLoad(gameObject);
			}

			InjectEditorLocationProvider();
		}

		/// <summary>
		/// Injects the editor location provider.
		/// Depending on the platform, this method and calls to it will be stripped during compile.
		/// </summary>
		void InjectEditorLocationProvider()
		{
			Debug.LogFormat("LocationProviderFactory: Injected EDITOR Location Provider - {0}", _editorLocationProvider.GetType());
			DefaultLocationProvider = _editorLocationProvider;
		}
	}
}
