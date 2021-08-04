using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace LWARS
{

    public class AnimationPlayer
    {
        private static readonly int MIXER_COUNT = 3;
        
        public enum EFlag
        {
            None                = 0,
            SamePlay            = (1<<0),       // 
            BlendTimeOverride   = (1<<1),       // 
        }
        
        public class PlayStatus
        {
            public enum State
            {
                Keep,
                BlendUp,
                BlendDown,
                //NoClip,
            }
            
            public State                        _state;
            public AnimationClip                _clip;
            public AnimationClipPlayable        _clip_playable;
            public float                        _weight;
            public short                        _mixer_layer = -1;
            public short                        _mixer_id = -1;
            
            public bool IsActive()
            {
                return (_state == State.Keep || _state == State.BlendUp);
            }
            
            public bool IsLoop()
            {
                return (_clip != null && _clip.isLooping);
            }
        }
        

        Animator                                _animator;
        PlayableGraph                           _graph;
        AnimationLayerMixerPlayable             _mixer_layer;
        AnimationMixerPlayable[]                _mixer;
        float                                   _blend_time;
        public float                            _speed = 1.0f;

        List<PlayStatus>                        _play_statuss;
        
        private BitFlag<EFlag>                  m_Flag          = new BitFlag<EFlag>( );
        private float                           m_BlendTime     = 0;
        
        
        public Animator Animator { get { return _animator; } }
        public PlayableGraph Graph { get { return _graph; } }


        public void Create(Animator animator, int layer_num = 1, DirectorUpdateMode updateMode = DirectorUpdateMode.GameTime)
        {
            Destroy();

            _animator = animator;
            if (_animator == null)
            {
                return;
            }
            
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }

            _graph = PlayableGraph.Create();
            _graph.SetTimeUpdateMode(updateMode);

            _mixer = new AnimationMixerPlayable[layer_num];

            _mixer_layer = AnimationLayerMixerPlayable.Create(_graph, layer_num);
            for (int i = 0; i < _mixer.Length; ++i)
            {
                if (!_mixer[i].IsValid())
                {
                    _mixer[i] = AnimationMixerPlayable.Create(_graph, MIXER_COUNT, true);
                    _mixer_layer.AddInput(_mixer[i], 0, 1);
                }
            }

            var output = AnimationPlayableOutput.Create(_graph, "output", _animator);
            output.SetSourcePlayable(_mixer_layer);
           _graph.Play();
           
            if (_play_statuss == null)
            {
                _play_statuss = new List<PlayStatus>();
            }
        }
        

        public void Destroy()
        {
            _play_statuss = null;

            if (_graph.IsValid())
            {
                _graph.Destroy();
            }
        }

      
        public virtual void Update(float dt)
        {
            float weight_base = (_blend_time > 0.0f) ? dt / _blend_time : 1.0f;
            
            for (int i = _play_statuss.Count - 1; i >= 0; --i)
            {
                PlayStatus status = _play_statuss[i];

                float weight = weight_base * _speed;
                switch (status._state)
                {
                    case PlayStatus.State.Keep:
                        break;

                    case PlayStatus.State.BlendUp:
                        status._weight += weight;
                        if (status._weight >= 1.0f)
                        {
                            status._weight = 1.0f;
                            status._state = PlayStatus.State.Keep;
                        }
                        _mixer[status._mixer_layer].SetInputWeight((int)status._mixer_id, status._weight);
                        break;

                    case PlayStatus.State.BlendDown:
                        status._weight -= weight;
                        if (status._weight <= 0.0f)
                        {
                            Stop(status._clip);
                            continue;
                        }
                        _mixer[status._mixer_layer].SetInputWeight((int)status._mixer_id, status._weight);
                        break;
                }
            }
        }
        
        public void ResetFlag( )
        {
            m_Flag.flag = 0;
        }
        
        public void SetSamePlay( )
        {
            m_Flag.SetValue( EFlag.SamePlay, true );
        }
        

        public void ResetSamePlay( )
        {
            m_Flag.SetValue( EFlag.SamePlay, false );
        }

        public void SetBlendTime( float time )
        {
            m_Flag.SetValue( EFlag.BlendTimeOverride, true );
            m_BlendTime = time;
        }
        

        public void ResetBlendTime( )
        {
            m_Flag.SetValue( EFlag.BlendTimeOverride, false );
        }
        

        public bool Play(AnimationClip clip, float blend_time = 0.0f, int mixer_layer = 0)
        {
            if(clip == null)
            {
                ResetFlag( );
                return false;
            }
            

            bool playing = (_play_statuss.Count != 0);
            
            if( m_Flag.HasValue( EFlag.BlendTimeOverride ) )
            {
                blend_time = m_BlendTime;
            }
            
            if (playing)
            {
                _blend_time = blend_time;
            }
            else
            {
                _blend_time = 0.0f;
            }

            PlayStatus status = null;
            for (int i = _play_statuss.Count - 1; i >= 0; --i)
            {
                if (_play_statuss[i]._clip.GetHashCode( ) == clip.GetHashCode( ) )
                {
                    if (_play_statuss[i].IsActive())
                    {
                        if( m_Flag.HasValue( EFlag.SamePlay ) == false )
                        {
                            ResetFlag( );
                            return false;
                        }
                    }
                    
                    status = _play_statuss[i];
                    break;
                }
            }

            if (status == null)
            {
                status = new PlayStatus();
                status._clip = clip;
                status._state = PlayStatus.State.BlendUp;
                status._clip_playable = AnimationClipPlayable.Create(_graph, status._clip);
                status._clip_playable.SetSpeed(_speed);
                status._weight = (_blend_time > 0.0f) ? 0.0f : 1.0f; // 

                if (mixer_layer >= _mixer.Length)
                {
                    ResetFlag( );
                    return false;
                }
                status._mixer_layer = (short)mixer_layer;

                // 
                for (int i = 0; i < _mixer[status._mixer_layer].GetInputCount(); ++i)
                {
                    if (!_mixer[status._mixer_layer].GetInput(i).IsValid())
                    {
                        _mixer[status._mixer_layer].ConnectInput(i, status._clip_playable, 0);
                        status._mixer_id = (short)i;
                        break;
                    }
                }

                // 
                if (status._mixer_id < 0)
                {
                    int remove = -1;
                    float weight = 1;
                    for( int i = _play_statuss.Count - 1; i >= 0; --i )
                    {
                        if( _play_statuss[i] == null )
                        {
                            _play_statuss.RemoveAt( i );
                            continue;
                        }
                        if( _play_statuss[i]._mixer_layer != mixer_layer ) continue;
                        
                        if( _play_statuss[i]._weight < weight )
                        {
                            remove = i;
                            weight = _play_statuss[i]._weight;
                        }
                    }
                    if( remove != -1 )
                    {
                        _graph.Disconnect( _mixer[status._mixer_layer], _play_statuss[ remove ]._mixer_id );
                        _play_statuss.RemoveAt( remove );
                        _mixer[status._mixer_layer].ConnectInput(remove, status._clip_playable, 0);
                        status._mixer_id = (short)remove;
                    }
                    else
                    {
                        StopAll();
                        _mixer[status._mixer_layer].ConnectInput(0, status._clip_playable, 0);
                        status._mixer_id = 0;
                    }
                }

                _play_statuss.Add(status);
            }
            else
            {
                status._state = PlayStatus.State.BlendUp;

                if (status.IsLoop())
                {
                    // 
                    status._weight = (_blend_time > 0.0f) ? status._weight : 1.0f;
                }
                else
                {
                    // 
                    status._clip_playable.SetTime(0.0f);
                    status._weight = (_blend_time > 0.0f) ? 0.0f : 1.0f;
                }
            }


            // 
            for (int i = _play_statuss.Count - 1; i >= 0; --i)
            {
                if (_play_statuss[i] != status    // 
                    && _play_statuss[i]._mixer_layer == status._mixer_layer  // 
                )
                {

                    {
                        _play_statuss[i]._state = PlayStatus.State.BlendDown;
                        if (_blend_time <= 0.0f)
                        {
                            _play_statuss[i]._weight = 0.0f;
                        }
                    }
                }
            }
            
            if (!playing)
            {
                Update(0.0f);
            }

            ResetFlag( );
            
            return true;
        }
        
        public void Stop(AnimationClip clip, int mixer_layer = 0)
        {

            PlayStatus status = FindStatus(clip);
            if( status != null )
            {
                if (status._mixer_id >= 0)
                {
                    if (_mixer[status._mixer_layer].GetInput(status._mixer_id).IsValid())
                    {
                        _graph.Disconnect(_mixer[status._mixer_layer], status._mixer_id);
                    }
                    status._mixer_id = -1;
                }
                _play_statuss.Remove(status);
            }
        }
        

        public void StopAll()
        {
            if (_play_statuss == null) { return; }

            for (int i = _play_statuss.Count - 1; i >= 0; --i)
            {
                Stop(_play_statuss[i]._clip);
            }
        }
        

        public PlayStatus FindStatus(AnimationClip clip)
        {
            PlayStatus status = null;
            foreach (var i in _play_statuss)
            {
                if (i._clip == clip)
                {
                    status = i;
                    break;
                }
            }

            return status;
        }
        

        public void SetTime( float time )
        {
            for( int i = _play_statuss.Count - 1; i >= 0; --i )
            {
                _play_statuss[ i ]._clip_playable.SetTime( time );
            }
            _graph.Evaluate( 0.0f );
        }
        
        public void SetTime( float time, int mixer_layer )
        {
            for( int i = _play_statuss.Count - 1; i >= 0; --i )
            {
                PlayStatus status = _play_statuss[i];
                if( status._mixer_layer != mixer_layer )
                    continue;
                
                status._clip_playable.SetTime( time );
            }
            _graph.Evaluate( 0.0f );
        }
        
        public void SetTimeToLastAnim( float time )
        {
            if( _play_statuss.Count > 0 )
            {
                _play_statuss[ _play_statuss.Count-1 ]._clip_playable.SetTime( time );
                _graph.Evaluate( 0.0f );
            }
        }
        
        public void SetSpeed( float speed )
        {
            foreach( var i in _play_statuss )
            {
                i._clip_playable.SetSpeed( speed );
            }

            _speed = speed;
        }
        
        public float GetLength( )
        {
            float length = 0;
            foreach( var i in _play_statuss )
            {
                length = i._clip_playable.GetAnimationClip( ).length;
            }
            return length;
        }
        

        public float GetElapseTime( string id )
        {

            return 0.0f;
        }
        

        public float GetRemainingTime( string clip_name )
        {
            foreach (var i in _play_statuss)
            {
                if (i._clip.name == clip_name)
                {
                    return Mathf.Max(i._clip.length - (float)i._clip_playable.GetTime(), 0.0f);
                }
            }

            return 0.0f;
        }
        

        public float GetNormalizedTime( string id )
        {

            return 0.0f;
        }

        
        public bool IsPlayToLastAnim( string name, bool isVague = true  )
        {
            if( _play_statuss.Count > 0 )
            {
                if( isVague )
                {
                    return _play_statuss[ _play_statuss.Count-1 ]._clip.name.IndexOf( name ) != -1;
                }
                else
                {
                    return _play_statuss[ _play_statuss.Count-1 ]._clip.name == name;
                }
            }
            return false;
        }
        
        
        #if UNITY_EDITOR
        

        public void ChangeTimeUpdateMode( DirectorUpdateMode updateMode )
        {
            if( _graph.IsValid() )
            {
                _graph.SetTimeUpdateMode( updateMode );
            }
        }
        
      
        #endif
    }
}
