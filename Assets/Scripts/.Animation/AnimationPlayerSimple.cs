using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace LWARS
{

    public class AnimationPlayerSimple : MonoBehaviour
    {

        private AnimationPlayer                     _player;
        [SerializeField]private AnimationClip       _clip   = null;
        

        public AnimationClip Clip
        {
            get { return _clip; }
        }
        
        
        protected void Awake()
        {
            if (_clip == null) { return; }

            if (_player == null)
            {
                var animator = GetComponent<Animator>();
                if (animator == null) { return; }

                _player = new AnimationPlayer();
                _player.Create(animator);
                _player.Play(_clip);
                _player.Update(0.0f);
            }
        }
        
        protected  void OnDestroy()
        {
            if (_player != null)
            {
                _player.Destroy();
                _player = null;
            }
        }
        
        private void Update()
        {
            if (_player != null)
            {
                float dt = Time.deltaTime;
                _player.Update(dt);
            }
        }
        

        public bool IsPlay()
        {
            if (_player == null) { return false; }

            return (_player.GetRemainingTime(_clip.name) > 0.0f);
        }
        
        
        #if UNITY_EDITOR
        
        public void EditorAwake()
        {
            Awake();
            UpdateManual( 0, 0 );
        }
        
        public void UpdateManual( float t, float dt )
        {
            if( _player != null )
            {
                SetTimeToPlayable( _player.Graph, t );
                
                if( _player != null && _player.Graph.IsValid() )
                {
                    _player.Graph.Evaluate( dt );
                }
                
                _player.Update( dt );
            }
        }
        
        void SetTimeToPlayable( PlayableGraph graph, float time )
        {
            int outputCnt = graph.GetOutputCount();
            if( outputCnt == 0 )
                return;

            for( int i = 0; i < outputCnt; ++i )
            {
                UnityEngine.Playables.PlayableOutput output = graph.GetOutput( i );
                if( output.IsOutputValid() == false )
                    continue;

                _SetTimeToPlayable( output.GetSourcePlayable(), time );
            }
        }
        
        void _SetTimeToPlayable( Playable playable, float time )
        {
            if( playable.IsValid() == false )
                return;

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
        
        #endif
    }
}
