using UnityEngine;
using System.Collections.Generic;
// using System.Buffers;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Playables;
#endif

namespace LWARS
{

    public class EventPlayerStatus
    {


        
        private string                      m_Name;
        private EventParam                  m_EventParam;
        private MonoBehaviour      m_Behaviour;
        private EventTrackStatus[]          m_TrackStatus;
        private int                         m_TrackStatusCount;
        
        private float                       m_Length;
        private float                       m_LoopStart;
        private float                       m_Time;
        private float                       m_TimePrev;
        
        private bool                        m_Immidiate;
        
        
        public string                       Name                { get { return m_Name;          } }
        public EventParam                   Param               { get { return m_EventParam;    } }
        
        public float                        Length              { get { return m_Length;        } }
        public float                        Time                { get { return m_Time;          } }
        public float                        TimePrev            { get { return m_TimePrev;      } }
        
        public bool                         isImmidiate         { get { return m_Immidiate;     } }
        
        public EventPlayerStatus( )
        {
        }
        

        public void Initialize( string name, EventParam param, MonoBehaviour behaviour )
        {
   
            Release( );
            

            m_Name          = name;
            m_EventParam    = param;
            m_Behaviour     = behaviour;
            m_TrackStatus   = null;
  
            m_TrackStatusCount = m_EventParam.Events.Length;
            m_TrackStatus = ArrayPool<EventTrackStatus>.Shared.Rent(m_TrackStatusCount);
            for( int i = 0; i < m_TrackStatusCount; ++i )
            {
                EventTrack track = m_EventParam.Events[i];
                if( track != null )
                {
                    m_TrackStatus[i] = track.CreateStatus( this );
                    m_TrackStatus[i].Initialize( m_Behaviour, track );
                }
            }
            

            m_Time = m_TimePrev = 0.0f;
            

            m_Length = m_EventParam.GetEndTime();
            

            m_LoopStart = m_EventParam.LoopStart;
        }
        

        public void Release( )
        {

            if( m_TrackStatus != null )
            {

                for( int index = 0; index < m_TrackStatusCount; ++index )
                {
                    EventTrackStatus status = m_TrackStatus[ index ];
                    if( status == null )
                    {
                        continue;
                    }
                    
               
                    status.Release( m_Behaviour );
                }
                
 
                if (m_TrackStatus != null)
                {
                    ArrayPool<EventTrackStatus>.Shared.Return(m_TrackStatus, true);
                    m_TrackStatus = null;
                }
                m_TrackStatusCount = 0;
            }
            

            m_Name          = null;
            m_EventParam    = null;
            m_Behaviour     = null;
            m_TrackStatus   = null;
            m_TrackStatusCount = 0;
        }
        
        
        public void UpdateEvent( float dt )
        {
         
            if( m_TrackStatus == null )
            {
                return;
            }
            
            m_TimePrev = m_Time;
            m_Time += dt;
            //DebugUtility.Log( m_Time );
            
            for( int index = 0; index < m_TrackStatusCount; ++index )
            {
                EventTrackStatus status = m_TrackStatus[ index ];
                if( status == null )
                {
                    continue;
                }
                
                if( status.UpdateEvent( m_Behaviour, m_Time, dt ) == false )
                {
                    return;
                }
            }
            
            if( m_EventParam.Loop )
            {
                if( GetRemainingTime( ) <= 0.0f )
                {
                    m_Time = m_LoopStart;
                    m_TimePrev = m_LoopStart;

                    for( int index = 0; index < m_TrackStatusCount; ++index )
                    {
                        EventTrackStatus status = m_TrackStatus[ index ];
                        if( status == null )
                        {
                            continue;
                        }
                        status.Clear( m_Behaviour );
                        status.LoopStart( m_Behaviour, m_Time );
                    }
                }
            }
        }
        

        public void UpdateEventImmidiate( float time )
        {
            m_TimePrev = m_Time = time;
            m_Immidiate = true;
            UpdateEvent( 0 );
            m_Immidiate = false;
        }
        


        public float GetElapseTime( )
        {
            return m_Time;
        }
        

        public float GetNormalizedTime( )
        {
            return m_Time / m_Length;
        }

        public float GetRemainingTime( )
        {
            float time = m_Length - m_Time;
            return ( time > 0.0f ) ? time : 0.0f;
        }
        

        public virtual GameObject GetObject<T>( string name ) where T : EventTrack
        {
            GameObject result = null;
            for( int i = 0; i < m_TrackStatusCount; ++i )
            {
                EventTrackStatus status = m_TrackStatus[i];
                if( status == null ) continue;
                
                if( status.Track is T )
                {
                    result = status.GetObject( name );
                    if( result != null ) break;
                }
            }
            return result;
        }
        


        public bool IsPlay( )
        {
            return ( GetRemainingTime() > 0.0f );
        }
        

        public bool IsLoop()
        {
            return Param.Loop;
        }
        

        #if UNITY_EDITOR
        

        public void EditorUpdateEvent( float dt, bool isSeek )
        {

            if( m_TrackStatus == null )
            {
                return;
            }
            
            m_TimePrev = m_Time;
            m_Time += dt;
            
            for( int index = 0; index < m_TrackStatusCount; ++index )
            {
                EventTrackStatus status = m_TrackStatus[ index ];
                if( status == null )
                {
                    continue;
                }
                
 
                if( status.EditorUpdateEvent( m_Behaviour, m_Time, dt, isSeek ) == false )
                {

                    return;
                }
            }
            
            if( m_EventParam.Loop )
            {
                if( GetRemainingTime( ) <= 0.0f )
                {
                    
                    m_Time = m_LoopStart;
                    m_TimePrev = m_LoopStart;
    
                    for( int index = 0; index < m_TrackStatusCount; ++index )
                    {
                        EventTrackStatus status = m_TrackStatus[ index ];
                        if( status == null )
                        {
                            continue;
                        }
                        status.Clear( m_Behaviour );
                        status.LoopStart( m_Behaviour, m_Time );
                    }
                }
            }
        }
        

        public void ReleaseObjectOnPlayTrack()
        {
            for( int index = 0; index < m_TrackStatusCount; ++index )
            {
                EventTrackStatus status = m_TrackStatus[ index ];
                if( status == null )
                {
                    continue;
                }
                
                // 
                status.EditorRelease();
            }
        }
        

        public void EditorPreProcess( float time, float dt, bool isLooped, bool isEnded )
        {
            for( int index = 0; index < m_TrackStatusCount; ++index )
            {
                EventTrackStatus status = m_TrackStatus[ index ];
                if( status == null )
                {
                    continue;
                }
                
                m_TimePrev = time - dt;
                m_Time = time;
                
                // 更新
                status.EditorPreProcess( m_Behaviour, time, dt, isLooped, isEnded );
            }
        }
        

        public void EditorPostProcess( float time, float dt, bool isOnSkillSeq, bool isLooped, bool isEnded )
        {
            // 
            if( isOnSkillSeq )
            {
                // 
                if( m_EventParam.Loop == false )
                {
                    if( GetRemainingTime() <= 0.0f )
                    {
                        // 
                        m_Time = m_LoopStart;
                        m_TimePrev = m_LoopStart;
                        // 
                        for( int index = 0; index < m_TrackStatusCount; ++index )
                        {
                            EventTrackStatus status = m_TrackStatus[ index ];
                            if( status == null )
                            {
                                continue;
                            }
                            status.Clear( m_Behaviour );
                            status.LoopStart( m_Behaviour, m_Time );
                        }
                    }
                }
            }
            
            for( int index = 0; index < m_TrackStatusCount; ++index )
            {
                EventTrackStatus status = m_TrackStatus[ index ];
                if( status == null )
                {
                    continue;
                }
                
                // 更新
                status.EditorPostProcess( m_Behaviour, m_Time, dt, isLooped, isEnded );
            }
        }
        

        public void DrawGUI()
        {
            GUILayout.Label( m_Name + "[" + m_Length + "] " + m_Time + " <= " + m_TimePrev );
        }
        
        #endif  

    }
}
