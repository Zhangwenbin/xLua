
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LWARS;

using UnityEngine.Playables;
using UnityEngine.Animations;

namespace LWARS
{
#if UNITY_EDITOR
    public class EditorAnimationPlayer : EventPlayer
    {
        
        public void _SetPlayableTime( float time )
        {
            if (AnimationPlayer.Graph.IsValid() == false)
                return;

            int outputCnt = AnimationPlayer.Graph.GetOutputCount();
            if (outputCnt == 0)
                return;

            for (int i = 0; i < outputCnt; ++i)
            {
                PlayableOutput output = AnimationPlayer.Graph.GetOutput(i);
                if (output.IsOutputValid() == false)
                    continue;

                _SetTimeToPlayable(output.GetSourcePlayable(), time);
            }
        }
        
        void _SetTimeToPlayable( Playable playable, float time )
        {
            if( playable.IsValid() == false )
                return ;
            
            playable.SetTime( time );
            
            int inputCnt = playable.GetInputCount();
            if( inputCnt > 0 )
            {
                for( int i = 0; i < inputCnt; ++i )
                {
                    _SetTimeToPlayable( playable.GetInput( i ), time );
                }
            }
        }
        
        [UnityEditor.CustomEditor(typeof(EditorAnimationPlayer))]
        public class EditorInspector_EditorAnimationPlayer: EditorInspector_EventPlayer
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI( );
            }
        }
    }
#endif
    
    public class EditorFaceAnmController
    {

        
        SkinnedMeshRenderer m_SkinnedMesh       = null;
        int                 m_BlendShapeCnt     = 0;
        string[]            m_NameTbl           = new string[0];
        
        
        
        public int BlendShapeCount
        {
            get { return m_BlendShapeCnt; }
        }
        
        
        
        public EditorFaceAnmController()
        {
        }


        public void Initialize( SkinnedMeshRenderer skinRen )
        {
            if( skinRen == null )
                return;
            
            m_SkinnedMesh = skinRen;
            Mesh mesh = skinRen.sharedMesh;
            if( mesh == null || mesh.blendShapeCount == 0 )
                return;
            
            m_BlendShapeCnt = mesh.blendShapeCount;
            m_NameTbl = new string[ m_BlendShapeCnt ];
            for( int i = 0; i < m_BlendShapeCnt; ++i )
            {
                m_NameTbl[i] = mesh.GetBlendShapeName( i );
            }
        }
        

        public void Release()
        {
            m_NameTbl = null;
            m_SkinnedMesh = null;
        }
        

        public void SetWeight( int idx, float weight )
        {
            if( m_SkinnedMesh == null )
                return;
            
            if( idx < 0 || m_NameTbl.Length <= idx )
                return;
            
            m_SkinnedMesh.SetBlendShapeWeight( idx, weight );
        }
        
        public void SetWeight( string key, float weight )
        {
            if( string.IsNullOrEmpty( key ) )
                return;
            
            if( m_SkinnedMesh == null )
                return;
            
            for( int i = 0, max = m_NameTbl.Length; i < max; ++i )
            {
                if( m_NameTbl[i] == key )
                {
                    m_SkinnedMesh.SetBlendShapeWeight( i, weight );
                }
            }
        }
        
        #if UNITY_EDITOR
        

        public void DrawGUI()
        {
            if( m_SkinnedMesh == null )
            {
                GUILayout.Label( "SkinnedMesh" );
                return;
            }
            
            if( m_BlendShapeCnt == 0 )
            {
                GUILayout.Label( "BlendShape" );
                return;
            }
            
            for( int i = 0; i < m_BlendShapeCnt; ++i )
            {
                float weight = m_SkinnedMesh.GetBlendShapeWeight( i );
                
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label( m_NameTbl[i], GUILayout.Width( 60 ) );
                    int postW = UnityEditor.EditorGUILayout.IntSlider( (int)weight, 0, 100 );
                    if( postW != weight )
                    {
                        m_SkinnedMesh.SetBlendShapeWeight( i, (float)postW );
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
        
        #endif
    }
}
