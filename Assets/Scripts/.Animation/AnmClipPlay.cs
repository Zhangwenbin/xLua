using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace LWARS
{
    public class AnmClipPlay : MonoBehaviour
    {
        [System.Serializable]
        public class AnmInfo
        {
            public string           Key     = "";
            public AnimationClip    Clip    = null;
        }
        
        
        private AnimationPlayer     m_Player;
        
        [SerializeField]
        private AnmInfo[]           m_ClipInfos = null;
        
        private AnmInfo[]           m_PlayingInfos  = null;
        private int                 m_MixerLayerNum = 1;
        
        protected void Awake()
        {
        }
        
        public void Setup( int mixerLayerNum )
        {
            var animator = GetComponent<Animator>();
            if( animator == null)
                return;
            m_MixerLayerNum = Mathf.Max( 1, mixerLayerNum );
            m_Player = new AnimationPlayer();
            m_Player.Create( animator, 2, DirectorUpdateMode.Manual );
            m_PlayingInfos = new AnmInfo[ m_MixerLayerNum ];
        }
        
        protected  void OnDestroy()
        {
            if( m_Player != null )
            {
                m_Player.Destroy();
                m_Player = null;
            }
        }
        

        public void UpdateManual( float elapse, int layerId )
        {
            if( m_PlayingInfos == null )
                return;
            
            if( layerId >= m_MixerLayerNum )
                return;
            
            if( m_Player != null )
            {
                m_Player.SetTime( elapse, layerId );
                m_Player.Update( 0 );
            }
        }
        
        
        public void Play( string key, int layerId )
        {
            if( layerId >= m_MixerLayerNum )
                return;
            
            if( string.IsNullOrEmpty( key ) )
                return;
            
            AnmInfo info = m_PlayingInfos[layerId];
            if( info != null && info.Key == key )
                return;
            
            if( m_ClipInfos == null || m_ClipInfos.Length == 0 )
                return;
            
            for( int i = 0, max = m_ClipInfos.Length; i < max; ++i )
            {
                if( m_ClipInfos[i].Key == key )
                {
                    AnmInfo playInfo = m_ClipInfos[i];
                    m_PlayingInfos[layerId] = playInfo;
                    m_Player.Play( playInfo.Clip, 0, layerId );
                    m_Player.Update( 0 );
                    break;
                }
            }
        }
        
  
        public bool IsPlay()
        {
            if( m_Player == null || m_PlayingInfos == null || m_PlayingInfos.Length == 0 )
                return false;
            
            for( int i = 0; i < m_MixerLayerNum; ++i )
            {
                AnmInfo info = m_PlayingInfos[i];
                if( info == null )
                    continue;
                
                if( m_Player.GetRemainingTime( info.Clip.name ) > 0.0f )
                    return true;
            }
            
            return false;
        }
        
    }
}
