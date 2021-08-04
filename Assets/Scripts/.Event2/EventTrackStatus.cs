
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LWARS
{

    public class EventTrackStatus
    {
        public enum PlayState
        {
            Wait    ,
            Active  ,
            Finish  ,
        }
        
        protected struct ObjectCache
        {
            public MonoBehaviour behaviour;
            public GameObject          gameObject;
        }
        
        
        protected EventPlayerStatus     m_Owner;
        protected EventTrack            m_Track;
        protected PlayState             m_State;
        

        
        public EventPlayerStatus        Owner      { get { return m_Owner;  } }
        public EventTrack               Track      { get { return m_Track;  } }
        public PlayState                State      { get { return m_State;  } }
        

        public EventTrackStatus( EventPlayerStatus owner )
        {
            m_Owner = owner;
        }
        

        public virtual void Initialize( MonoBehaviour behaviour, EventTrack track )
        {
            m_Track = track;
            m_State = PlayState.Wait;
        }
        

        public virtual void Release( MonoBehaviour behaviour )
        {
            if( m_Track == null ) return ;
            
            if( m_State == PlayState.Active )
            {

                EventTrack.CurrentStatus = this;

                m_State = PlayState.Finish;
                OnEnd( behaviour );

                EventTrack.CurrentStatus = null;
            }
            
            m_Track = null;
        }
        

        public virtual void Clear( MonoBehaviour behaviour )
        {
            if( m_State == PlayState.Active )
            {

                EventTrack.CurrentStatus = this;

                m_State = PlayState.Finish;
                OnEnd( behaviour );

                EventTrack.CurrentStatus = null;
            }
            m_State = PlayState.Wait;
        }
        

        public virtual void LoopStart( MonoBehaviour behaviour, float time )
        {
            if( m_State == PlayState.Wait )
            {
                if( time >= m_Track.End )
                {
                    m_State = PlayState.Finish;
                }
            }
        }
        

        public virtual GameObject GetObject( string name )
        {
            return null;
        }
        

        protected void CacheTarget( MonoBehaviour behaviour, string targetId, ref ObjectCache cache )
        {
            if( cache.behaviour != behaviour )
            {
                // if( string.IsNullOrEmpty( targetId ) == false )
                // {
                //     SerializeValueBehaviour valueBehaviour = behaviour.GetComponent<SerializeValueBehaviour>( );
                //     if( valueBehaviour != null )
                //     {
                //         cache.gameObject = valueBehaviour.list.GetGameObject( targetId );
                //     }
                //     else
                //     {
                //         cache.gameObject = behaviour.gameObject.FindChildAll( targetId, true );
                //     }
                // }
                // else
                {
                    cache.gameObject = behaviour.gameObject;
                }
                cache.behaviour = behaviour;
            }
        }
        

        public bool UpdateEvent( MonoBehaviour behaviour, float time, float deltaTime )
        {
            #if UNITY_EDITOR

            if( Application.isPlaying == false )
            {
                if( deltaTime == 0 )
                    return false;
            }
            #endif
            
            EventTrack track = m_Track;
            if( track == null )
            {
                return false;
            }
            
            EventTrack.CurrentStatus = this;
            EventTrack.DeltaTime = deltaTime;
            

            if( m_State == PlayState.Wait )
            {
                if( track.Start <= time )
                {
                    m_State = PlayState.Active;
                    OnStart( behaviour );
                }
            }

            if( m_State == PlayState.Active )
            {
                OnUpdate( behaviour, time );
                
                if( track.End <= time )
                {
                    m_State = PlayState.Finish;
                    OnEnd( behaviour );
                }
            }
            
            OnBackground( behaviour, time );
            
            EventTrack.CurrentStatus = null;
            EventTrack.DeltaTime = 0;
            
            return m_Track != null;
        }
        

        protected virtual void OnStart( MonoBehaviour behaviour )
        {
            m_Track.OnStart( behaviour );
        }
        

        protected virtual void OnUpdate( MonoBehaviour behaviour, float time )
        {
            m_Track.OnUpdate( behaviour, time );
        }
        

        protected virtual void OnEnd( MonoBehaviour behaviour )
        {
            m_Track.OnEnd( behaviour );
        }
        

        protected virtual void OnBackground( MonoBehaviour behaviour, float time )
        {
            m_Track.OnBackground( behaviour, time );
        }
        
        
        #if UNITY_EDITOR
        
        public bool EditorUpdateEvent( MonoBehaviour behaviour, float time, float deltaTime, bool isSeek )
        {
            #if UNITY_EDITOR
            if( Application.isPlaying == false )
            {
                if( deltaTime == 0 && isSeek == false )
                    return false;
            }
            #endif
            
            EventTrack track = m_Track;
            if( track == null )
            {
                return false;
            }
            
            EventTrack.CurrentStatus = this;
            EventTrack.DeltaTime = deltaTime;
            

            if( m_State == PlayState.Wait )
            {
                if( track.Start <= time )
                {
                    m_State = PlayState.Active;
                    OnStart( behaviour );
                }
            }

            if( m_State == PlayState.Active )
            {

                OnUpdate( behaviour, time );
                
                if( track.End <= time )
                {
                    m_State = PlayState.Finish;
                    OnEnd( behaviour );
                }
            }
            
            OnBackground( behaviour, time );
            
            EventTrack.CurrentStatus = null;
            EventTrack.DeltaTime = 0;
            
            return m_Track != null;
        }
        

        static public GameObject CacheTargetOnEditor( MonoBehaviour behaviour, string targetId )
        {
            GameObject gobj = null;

            // if( string.IsNullOrEmpty( targetId ) == false )
            // {
            //     SerializeValueBehaviour valueBehaviour = behaviour.GetComponent<SerializeValueBehaviour>( );
            //     if( valueBehaviour != null )
            //     {
            //         gobj = valueBehaviour.list.GetGameObject( targetId );
            //     }
            //     else
            //     {
            //         gobj = behaviour.gameObject.FindChildAll( targetId, true );
            //     }
            // }
            // else
            {
                gobj = behaviour.gameObject;
            }
            
            return gobj;
        }
        

        protected virtual void ReCacheTarget( MonoBehaviour behaviour )
        {
        }
        

        public virtual void EditorRelease()
        {
            m_Track.EditorRelease();
        }
        

        public void EditorPreProcess( MonoBehaviour behaviour, float time, float dt, bool isLooped, bool isEnded )
        {
            EventTrack.CurrentStatus = this;
            
            
            if( m_State == PlayState.Finish )
            {
                if( time < m_Track.End )
                {
                    m_State = PlayState.Active;
                }
            }
            
            if( m_State == PlayState.Active )
            {
                if( time < m_Track.Start )
                {
                    m_State = PlayState.Wait;
                }
            }
            
            if( m_Track.IsRequestTrackStatusUpdate() )
            {
                ReCacheTarget( behaviour );
                m_Track.ResetTrackStatusUpdate();
            }
            
            m_Track.EditorPreProcess( behaviour, time, dt, isLooped, isEnded );
            
            EventTrack.CurrentStatus = null;
        }
        
        public void EditorPostProcess( MonoBehaviour behaviour, float time, float dt, bool isLooped, bool isEnded )
        {
            EventTrack.CurrentStatus = this;
            m_Track.EditorPostProcess( behaviour, time, dt, isLooped, isEnded );
            EventTrack.CurrentStatus = null;
        }
        

        // public static bool OnCustomProperty_TargetField( CustomFieldAttribute attr, UnityEditor.SerializedProperty prop, float width )
        // {
        //     EditorGUI.BeginChangeCheck();
        //     {
        //         EditorGUILayout.BeginHorizontal();
        //         {
        //             EditorGUILayout.LabelField( attr.text, GUILayout.Width( width * 0.35f ) );
        //             prop.stringValue = EditorGUILayout.TextField( prop.stringValue );
        //             SerializeValueBehaviour beheviour = null;
        //             EventPlayer player = EventPlayer.CurrentEditorPlayer;
        //             if( player != null ) beheviour = player.GetComponentInChildren<SerializeValueBehaviour>();
        //             if( beheviour != null )
        //             {
        //                 SerializeValueList valueList = beheviour.list;
        //                 string selectName = prop.stringValue;
        //                 SerializeValue[] gobjs = valueList.GetFields<GameObject>();
        //                 List<string> names = new List<string>();
        //                 for( int i = 0; i < gobjs.Length; ++i ) names.Add( gobjs[ i ].key );
        //                 int value = names.FindIndex( ( p ) => p == selectName );
        //                 GUI.contentColor = value == -1 ? Color.red : Color.green;
        //                 int nextValue = UnityEditor.EditorGUILayout.Popup( "", value, names.ToArray(), GUILayout.Width( 15f ) );
        //                 if( value != nextValue )
        //                 {
        //                     prop.stringValue = names[ nextValue ];
        //                 }
        //                 GameObject gobj = valueList.GetGameObject( selectName );
        //                 GUIStyle style = new GUIStyle( "toolbarButton" );
        //                 style.alignment = TextAnchor.MiddleCenter;
        //                 GUI.enabled = gobj != null ? true : false;
        //                 if( GUILayout.Button( "sel", style, GUILayout.Width( 30f ) ) )
        //                 {
        //                     UnityEditor.Selection.activeGameObject = gobj;
        //                 }
        //                 GUI.enabled = true;
        //                 GUI.contentColor = Color.white;
        //             }
        //         }
        //         EditorGUILayout.EndHorizontal();
        //     }
        //     return EditorGUI.EndChangeCheck();
        // }
        #endif
    }
}
