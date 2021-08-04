
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Playables;
#endif

namespace LWARS
{

    public class EventPlayer : MonoBehaviour
    {

        class LoadRequest
        {
            public string                       _id;
            public string                       _path;
            public LoaderRequest                _request;
        }
        
        
        public bool                                 UnscaleTime     = false;

        public float                                Speed           = 1.0f;
        

        public float                                MaxDeltaTime    = 0.3333f;
        
        // 
        private List<LoadRequest>                   _load_requests  = new List<LoadRequest>();
        private Dictionary<string, EventParam>      _loaded_events  = new Dictionary<string, EventParam>();
        
        // 
        private List<EventPlayerStatus>             _play_events    = new List<EventPlayerStatus>();
        private AnimationPlayer                     _animation      = null;
        
        
        public AnimationPlayer                      AnimationPlayer { get { return _animation; } }

        public bool isError;
        
        public bool                                 IsEventLoading  { get { return _load_requests.Count > 0; } }
        

        

        

        public  void Initialize( )
        {
            if( MaxDeltaTime == 0 )
            {
                MaxDeltaTime = Time.maximumDeltaTime;
            }
        }
        
        public  void Release( )
        {
            Clear( );
            
            if( _animation != null )
            {
                _animation.Destroy( );
                _animation = null;
            }
            

        }
        

        public void Clear( )
        {
            for (int i = _play_events.Count - 1; i >= 0; --i)
            {
                _play_events[i].Release();
                InstancePool<EventPlayerStatus>.Shared.Return( _play_events[i] );
            }
            _play_events.Clear( );
            
            for( int i = 0; i < _load_requests.Count; ++i )
            {
                if( _load_requests[ i ]._request != null )
                {
                    _load_requests[ i ]._request.Unload( );
                }
            }
            _load_requests.Clear( );
            
            _loaded_events.Clear();
        }
        

        public virtual AnimationPlayer CreateAnimation( Animator animator, int layerNum = 1 )
        {
            if( _animation != null )
            {
                Clear( );
                _animation.Destroy( );
                _animation = null;
            }
            if( animator != null )
            {
                _animation = new AnimationPlayer( );
                _animation.Create( animator, layerNum );
                return _animation;
            }
            return null;
        }
        


        protected virtual void Start()
        {
        }
        

        protected virtual void Update()
        {

            float dt = ( UnscaleTime ? TimerManager.UnscaledDeltaTime: TimerManager.DeltaTime ) * Speed;
            if( dt > MaxDeltaTime ) dt = MaxDeltaTime;
            UpdateEvent(dt);
        }
        

        public void UpdateEvent(float dt)
        {
            for (int i = _play_events.Count - 1; i >= 0; --i)
            {
                _play_events[i].UpdateEvent(dt);
            }
            
            if( _animation != null )
            {
                _animation.SetSpeed( Speed );
                _animation.Update( dt );
            }
        }
        

        protected virtual void LateUpdate()
        {
            // 
            for (int i = _play_events.Count - 1; i >= 0; --i)
            {
                if (!_play_events[i].IsPlay())
                {
                    StopEvent( _play_events[i] );
                }
            }
        }
        

        public void LoadEventAsync( string id, string path )
        {
            LoaderRequest loadReq = AssetManager.LoadAsync<EventParam>( path );
            if( loadReq != null )
            {
                LoadRequest req = new LoadRequest();
                
                req._id = id;
                req._path = path;
                req._request = loadReq;
                
                _load_requests.Add( req );
                
                if( _load_requests.Count == 1 )
                {
                    StartCoroutine( AsyncLoad( ) );
                }
            }
        }
        

        private System.Collections.IEnumerator AsyncLoad( )
        {
            do
            {
                for( int i = _load_requests.Count - 1; i >= 0; --i )
                {
                    if( !_load_requests[ i ]._request.isDone )
                    {
                        continue;
                    }
                    
                    EventParam asset = _load_requests[ i ]._request.asset as EventParam;
                    
                    // 
                    if( asset == null )
                    {
                        asset = EventParam.DefaultEventParam;
                    }
                    

                    _loaded_events[ _load_requests[ i ]._id ] = asset;
                    
                    _load_requests[ i ]._request.Unload( );
                    
                    _load_requests.RemoveAt( i );
                }
                
                yield return new WaitForEndOfFrame( );
            }
            while ( _load_requests.Count > 0 );
            
            yield return null;
        }
        

        public virtual EventParam FindEvent( string id )
        {
            if( !_loaded_events.ContainsKey( id ) )
            {
                return null;
            }
            
            EventParam anim = _loaded_events[ id ];
            if( anim == null )
            {
                return null;
            }
            
            return anim;
        }

        public string IsContained( EventParam evParam )
        {
            if( evParam == null )
                return "";
            
            foreach( var pair in _loaded_events )
            {
                if( pair.Value == evParam )
                    return pair.Key;
            }
            
            return "";
        }
        

        public void AddEvent( string id, EventParam anim )
        {
            _loaded_events[ id ] = anim;
        }
        

        public virtual void RemoveEvent( string id )
        {
            _loaded_events.Remove( id );
        }
        

        private EventPlayerStatus FindPlayStatus( string id )
        {
            for( int i = _play_events.Count - 1; i >= 0; --i )
            {
                if( _play_events[i].Name == id )
                {
                    return _play_events[i];
                }
            }
            return null;
        }
        

        public bool PlayEvent( string id, EventParam param, float time=0 )
        {

            EventPlayerStatus status = FindPlayStatus( id );
            if( status == null )
            {
                status = InstancePool<EventPlayerStatus>.Shared.Rent( ) ?? new EventPlayerStatus( );
                _play_events.Add( status );
            }
            
            // 
            status.Initialize( id, param, this );
            status.UpdateEventImmidiate( time );
            
            return true;
        }
        

        public virtual bool PlayEvent( string id, float time=0 )
        {
  
            if(!_loaded_events.ContainsKey(id))
            {
                return false;
            }
            
            EventParam param = _loaded_events[id];
            if(param == null)
            {
                return false;
            }
            
            // 
            EventPlayerStatus status = FindPlayStatus( id );
            if( status == null )
            {
                status = InstancePool<EventPlayerStatus>.Shared.Rent( ) ?? new EventPlayerStatus( );
                _play_events.Add( status );
            }
            
            // 
            status.Initialize( id, param, this );
            status.UpdateEventImmidiate( time );
            
            return true;
        }
        
        public void UpdateEventImmediate( string id, float time )
        {
            EventPlayerStatus status = FindPlayStatus(id);
            if( status != null )
            {
                status.UpdateEventImmidiate( time );
            }
        }
        
        public void StopEvent( EventPlayerStatus status )
        {
            if( status != null )
            {
                status.Release( );
                _play_events.Remove( status );
                InstancePool<EventPlayerStatus>.Shared.Return( status );
            }
        }
        public void StopEvent( string id )
        {
            StopEvent( FindPlayStatus( id ) );
        }
        

        public void StopEvent( )
        {
            for( int i = _play_events.Count - 1; i >= 0; --i )
            {
                StopEvent( _play_events[i] );
            }
        }
        

        public void SetSpeed( float speed )
        {
            Speed = speed;
        }
        

        public float GetLength( string id )
        {
            EventPlayerStatus status = FindPlayStatus( id );
            if( status == null )
            {
                return 0;
            }
            return status.Length;
        }
        
        public float GetElapseTime( string id )
        {
            EventPlayerStatus status = FindPlayStatus( id );
            if( status == null )
            {
                return 0;
            }
            return status.GetElapseTime( );
        }
        
        public float GetNormalizedTime( string id )
        {
            EventPlayerStatus status = FindPlayStatus( id );
            if( status == null )
            {
                return 0;
            }
            return status.GetNormalizedTime( );
        }
        

        public float GetRemainingTime( string id )
        {
            EventPlayerStatus status = FindPlayStatus( id );
            if( status == null )
            {
                return 0;
            }
            return status.GetRemainingTime( );
        }
        
        
        public GameObject GetObject<T>( string id, string name ) where T : EventTrack
        {
            EventPlayerStatus status = FindPlayStatus( id );
            if( status != null )
            {
                return status.GetObject<T>( name );
            }
            return null;
        }
        

        public virtual bool HasEvent( string id )
        {
            return _loaded_events.ContainsKey( id );
        }
        

        public bool IsEventPlaying( string id )
        {
            EventPlayerStatus status = FindPlayStatus( id );
            if( status != null )
            {
                return status.IsPlay( );
            }
            return false;
        }
        
        public bool IsEventLoop( string id )
        {
            EventPlayerStatus status = FindPlayStatus( id );
            if( status != null )
            {
                return status.IsLoop();
            }
            return false;
        }
        
        #if UNITY_EDITOR
        
        public static EventPlayer CurrentEditorPlayer = null;
        
        bool        m_IsManual            = false;      // 
        bool        m_IsOnSkillSeq        = true;       // 

        public void SetOnSkillSeq()
        {
            m_IsOnSkillSeq = false;
        }
        
        public bool IsManualUpdate
        {
            get { return m_IsManual; }
        }
        
        public Dictionary<string, EventParam> LoadedEvents
        {
            get { return _loaded_events; }
        }
        
        [UnityEditor.CustomEditor(typeof(EventPlayer))]
        public class EditorInspector_EventPlayer : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                CustomFieldAttribute.OnInspectorGUI( target.GetType(), serializedObject );
                
                EventPlayer player = target as EventPlayer;
                
                GUIStyle itemStyle = new GUIStyle( GUI.skin.label );
                itemStyle.padding.left = 10;
                
                UnityEditor.EditorGUILayout.LabelField( "Load Requests:" );
                for( int i = 0; i < player._load_requests.Count; ++i )
                {
                    UnityEditor.EditorGUILayout.BeginHorizontal( );
                    GUILayout.Space( 10 );
                    UnityEditor.EditorGUILayout.LabelField( player._load_requests[ i ]._path );
                    UnityEditor.EditorGUILayout.EndHorizontal( );
                }
                
                GUILayout.Space( 10 );
                
                UnityEditor.EditorGUILayout.LabelField( "Loaded Events:" );
                foreach( string key in player._loaded_events.Keys )
                {
                    UnityEditor.EditorGUILayout.BeginHorizontal( );
                    GUILayout.Space( 10 );
                    UnityEditor.EditorGUILayout.LabelField( player._loaded_events[ key ].name );
                    UnityEditor.EditorGUILayout.EndHorizontal( );
                }
                
                GUILayout.Space( 10 );
                
                
                GUILayout.Space( 10 );
            }
        }
        
        public void SetManualFlag( bool flag )
        {
            m_IsManual = flag;
        }
        

        public void SetPlayableTime( float time )
        {
            UpdateManual( time, 0, false, false );
        }
        

        public void UpdateManual( float time, float dt, bool isLooped, bool isEnded, bool isSeek = false )
        {
            SetManualFlag( true );
            
            // 
            for( int i = _play_events.Count - 1; i >= 0; --i )
            {
                _play_events[ i ].EditorPreProcess( time, dt, isLooped, isEnded );
            }
            
            if( _animation != null && _animation.Graph.IsValid() )
            {
                AnimationPlayer.Graph.Evaluate( dt );
            }
            
            EditorUpdateEvent( dt, isSeek );
            
            // 
            for( int i = _play_events.Count - 1; i >= 0; --i )
            {
                _play_events[ i ].EditorPostProcess( time, dt, m_IsOnSkillSeq, isLooped, isEnded );
            }
            
            SetManualFlag( false );
        }
        

        public void EditorUpdateEvent(float dt, bool isSeek )
        {
            for (int i = _play_events.Count - 1; i >= 0; --i)
            {
                _play_events[i].EditorUpdateEvent(dt, isSeek);
            }
            
            if( _animation != null )
            {
                _animation.SetSpeed( Speed );
                _animation.Update( dt );
            }
        }
        
        public void ReleaseObjectOnPlayTrack()
        {
            for( int i = _play_events.Count - 1; i >= 0; --i )
            {
                _play_events[ i ].ReleaseObjectOnPlayTrack();
            }
        }
        
        public void DrawGUI()
        {
            if( _play_events.Count > 0 )
            {
                for( int i = 0, max = _play_events.Count; i < max; ++i )
                {
                    _play_events[i].DrawGUI();
                }
            }
        }
        

        public EventPlayerStatus GetPlayingEventStatus()
        {
            if( _play_events.Count == 0 )
                return null;
            
            EventPlayerStatus status = null;
            for( int i = 0, max = _play_events.Count; i < max; ++i )
            {
                status = _play_events[0];
                if( status.IsPlay() )
                {
                    break;
                }
            }
            
            return status;
        }
        
        public virtual void ReleaseOnEditor()
        {
            ReleaseObjectOnPlayTrack();
            
            Release();
        }
        
        #endif
    }
}
