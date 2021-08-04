
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
// using CSharpHelpers;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Playables;
#endif

namespace LWARS
{

    [RequireComponent( typeof( EventPlayer ) )]
    public class EventPlayerMachine : MonoBehaviour
    {
        [System.Serializable]
        public class Param
        {

            public string           key;

            public EventParam       param;
        }
        


        public string                   AutoKey;
        

        public Param[]                  Params;
        
        private EventPlayer             m_Player        = null;
        private string                  m_Current       = null;
        private System.Action<object>   m_FinishAction  = null;
        private object                  m_FinishObject  = null;
        private Queue<string>        m_EventNameQueue;
        

        private bool isInitialized;
        public EventPlayer              Player
        {
            get
            {
                if( m_Player == null ) m_Player = gameObject.GetComponent<EventPlayer>( );
                return m_Player;
            }
        }
        
        public string[]                 Keys            { get { return Params.Select( ( prop ) => prop.key ).ToArray( );    } }
        public EventParam[]             Events          { get { return Params.Select( ( prop ) => prop.param ).ToArray( );  } }
        
        public string                   Current         { get { return m_Current; } }
        
        public bool                     SuppressPlay    { get; set; } = false;


        

        private void Awake( )
        {

            Initialize( );
        }
        

        public  void Initialize( )
        {
            if( isInitialized ) return;
            
            
            Player.Initialize( );
            
            if( Params != null )
            {

                for( int i = 0; i < Params.Length; ++i )
                {
                    Param param = Params[i];
                    if( param != null && param.param != null )
                    {
                        Player.AddEvent( param.key, param.param );
                    }
                }
            }
            
            // 
            if( string.IsNullOrEmpty( AutoKey ) == false )
            {
                Play( AutoKey );
            }
        }
        

        public  void Release( )
        {
            if( isInitialized == false ) return;
            
            Player.Release( );
            m_EventNameQueue.Dispose();
            
        }
        


        public void Update( )
        {
            if (m_EventNameQueue.Count > 0)
            {
                if (!IsPlaying())
                {
                    var head = m_EventNameQueue.Pop(0);
                    Play(head);
                }
            }

            if( m_FinishAction != null )
            {
                if( IsPlaying() == false )
                {
                    m_FinishAction( m_FinishObject );
                    m_FinishAction = null;
                }
            }
        }
        

        public void SetOnFinished( System.Action<object> action, object value )
        {
            m_FinishAction = action;
            m_FinishObject = value;
        }
        

        public void Reset( string key, bool lastFrame = false )
        {
            if( Player.HasEvent( key ) )
            {
                Player.PlayEvent( key, lastFrame ? float.MaxValue: 0 );
                Player.StopEvent( key );
            }
            m_Current = null;
        }
        

        public void CheckReset( string key, bool lastFrame = false )
        {
            if( Player.HasEvent( key ) )
            {
                if( Player.IsEventPlaying( key ) == false )
                {
                    Player.PlayEvent( key, lastFrame ? float.MaxValue : 0 );
                }
                else
                {
                    Player.UpdateEventImmediate( key, lastFrame ? float.MaxValue : 0 );
                }
                Player.StopEvent( key );
            }
            m_Current = null;
        }
        

        public void Play( string key )
        {
            if (SuppressPlay) return;

            if( Player.HasEvent( key ) )
            {
                m_Current = key;
                Player.PlayEvent( key );
            }
            else
            {
                m_Current = null;
            }
        }


        public void PlayNext(string key)
        {
            if (SuppressPlay) return;

            if (IsPlaying())
            {
                m_EventNameQueue.Add(key);
            }
            else
            {
                Play(key);
            }
        }
        

        public void Play( string key, float time )
        {
            if (SuppressPlay) return;

            if ( Player.HasEvent( key ) )
            {
                m_Current = key;
                Player.PlayEvent( key, time );
            }
            else
            {
                m_Current = null;
            }
        }
        

        public void Stop( string name )
        {
            if( string.IsNullOrEmpty( name ) == false )
            {
                Player.StopEvent( name );
            }
        }
        

        public void Stop( )
        {
            if( string.IsNullOrEmpty( m_Current ) == false )
            {
                Player.StopEvent( m_Current );
                m_Current = null;
            }
        }
        

        public void StopAll( )
        {
            Player.StopEvent( );
            m_Current = null;
        }
        

        public void SetSpeed( float speed )
        {
            Player.SetSpeed( speed );
        }
        

        public float GetLength( )
        {
            if( string.IsNullOrEmpty( m_Current ) == false )
            {
                return Player.GetLength( m_Current );
            }
            return 0;
        }
        

        public float GetRemainingTime( )
        {
            if( string.IsNullOrEmpty( m_Current ) == false )
            {
                return Player.GetRemainingTime( m_Current );
            }
            return 0;
        }
        

        public float GetNormalizedTime( )
        {
            if( string.IsNullOrEmpty( m_Current ) == false )
            {
                return Player.GetNormalizedTime( m_Current );
            }
            return 0;
        }
        

        public float GetElapseTime( )
        {
            if( string.IsNullOrEmpty( m_Current ) == false )
            {
                return Player.GetElapseTime( m_Current );
            }
            return 0;
        }
        

        public bool HasEvent( string key )
        {
            return Player.HasEvent( key );
        }
        

        public bool IsPlaying( )
        {
            if( string.IsNullOrEmpty( m_Current ) == false )
            {
                return Player.IsEventPlaying( m_Current );
            }
            return false;
        }
        

        public bool IsPlaying( string key )
        {
            return Player.IsEventPlaying( key );
        }
        

        public bool HasParam( string key )
        {
            if( Params != null && Params.Length > 0 )
            {
                for( int i = 0, max = Params.Length; i < max; ++i )
                {
                    if( Params[ i ].key == key )
                        return true;
                }
            }
            
            return false;
        }
        

        #if UNITY_EDITOR

        public void EditorCreateAnim()
        {
            Player.CreateAnimation( GetComponent<Animator>() );
        }
        

        public EventParam DrawGUISelect( EventParam evParam )
        {
            int idx = 0;
            EventParam[] evs = Events;
            for( int i = 0, max = evs.Length; i < max; ++i )
            {
                if( evs[i] == evParam )
                {
                    idx = i;
                    break;
                }
            }
            
            idx = EditorGUILayout.Popup( idx, Keys );
            return Events[idx];
        }
        
        #endif
    }
}
