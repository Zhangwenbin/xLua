using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LWARS
{
    public class EventParam : ScriptableObject
    {

        
        [SerializeField] private EventTrack.ECategory   _category;
        [SerializeField] private bool                   _loop;
        [SerializeField] private float                  _loopstart;
        [SerializeField] private float                  _length;
        [SerializeField] private EventTrack[]           _events = new EventTrack[0];
        
        
        public EventTrack.ECategory                     Category            { get { return _category;   } set { _category = value;  } }
        public bool                                     Loop                { get { return _loop;       } set { _loop = value;      } }
        public float                                    LoopStart           { get { return _loopstart;  } set { _loopstart = value; } }
        public float                                    Length              { get { return _length;     } set { _length = value;    } }
        public EventTrack[]                             Events              { get { return _events;     } set { _events = value;    } }

        
        static EventParam _default_event_param;
        
        public static EventParam DefaultEventParam
        {
            get
            {
                if( _default_event_param == null )
                {
                    _default_event_param = ScriptableObject.CreateInstance<EventParam>( );
                }
                return _default_event_param;
            }
        }
        
        
        public float CalcLength( )
        {
            float end = 0.0f;
            // 
            if (_events == null || _events.Length == 0)
                return end;

            foreach( var value in _events )
            {
                if( value.End > end )
                {
                    end = value.End;
                }
            }
            
            return end;
        }
        

        public float GetEndTime()
        {
            if( Length != 0 )
                return Length;
            
            return CalcLength();
        }
        
        
        public T[] GetEventTrack<T>() where T: EventTrack
        {
            List<T> ret = new List<T>( _events.Length );
            for( int i = 0; i < _events.Length; ++i )
            {
                if( _events[ i ] is T )
                {
                    ret.Add( (T)_events[ i ] );
                }
            }
            return ret.ToArray( );
        }
        

        public AnimationClip GetAnimationClip(string curve_name)
        {
            var list = GetEventTrack<EventTrackAnimation>();
            AnimationClip clip = null;
            
            foreach (var i in list)
            {
                foreach (var j in i.CurveNames)
                {
                    if (j == curve_name)
                    {
                        clip = i.Animation;
                        break;
                    }
                }
            }
            
            return clip;
        }
        
        
        public bool CheckPreLoad()
        {
            if( _events == null || _events.Length == 0 )
                return false;
            
            for( int i = 0, max = _events.Length; i < max; ++i )
            {
                if( _events[i] == null )
                    continue;
                
                if( _events[i].CheckPreLoad() )
                    return true;
            }
            return false;
        }
        

        public bool IsDonePreLoad()
        {
            if( _events == null || _events.Length == 0 )
                return true;
            
            for( int i = 0, max = _events.Length; i < max; ++i )
            {
                if( _events[i] == null )
                    continue;
                
                if( _events[i].IsDonePreLoad() == false )
                    return false;
            }
            return true;
        }
        

        public void StartPreLoad()
        {
            if( _events == null || _events.Length == 0 )
                return;
            
            for( int i = 0, max = _events.Length; i < max; ++i )
            {
                if( _events[i] == null )
                    continue;
                
                _events[i].StartPreLoad();
            }
        }
        

        public void UnloadPreLoad()
        {
            if( _events == null || _events.Length == 0 )
                return;
            
            for( int i = 0, max = _events.Length; i < max; ++i )
            {
                if( _events[i] == null )
                    continue;
                
                _events[i].UnloadPreLoad();
            }
        }
        
        #if UNITY_EDITOR
        
        public virtual void EditorStartPreLoad()
        {
            if( _events == null || _events.Length == 0 )
                return;
            
            for( int i = 0, max = _events.Length; i < max; ++i )
            {
                if( _events[i] == null )
                    continue;
                
                _events[i].EditorStartPreLoad();
            }
        }
        
        public virtual void EditorUnloadPreLoad()
        {
            if( _events == null || _events.Length == 0 )
                return;
            
            for( int i = 0, max = _events.Length; i < max; ++i )
            {
                if( _events[i] == null )
                    continue;
                
                _events[i].EditorUnloadPreLoad();
            }
        }
        
        #endif
        
        
        #if UNITY_EDITOR
        

        public string[] GetStreamingAssets( )
        {
            return null;
        }

        public bool HasUnpackedSubAsset
        {
            get
            {
                if (_events == null) { return false; }

                for( int i = 0; i < _events.Length; ++i )
                {
                    EventTrack e = _events[ i ];
                    if( e == null )
                    {
                        continue;
                    }
                    
                    if( AssetDatabase.IsMainAsset( e ) )
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        private void DeleteUnusedEvents()
        {
            if( !AssetDatabase.Contains( this ) )
            {
                return;
            }
            
            string path = AssetDatabase.GetAssetPath( this );
            
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath( path );
            for( int i = 0; i < assets.Length; ++i )
            {
                if( assets[ i ] == null || !(assets[ i ] is EventTrack) )
                {
                    continue;
                }
                
                EventTrack e = (EventTrack)assets[ i ];
                
                if( System.Array.LastIndexOf<EventTrack>( _events, e ) >= 0 )
                {
                    continue;
                }
                
                DestroyImmediate( e, true );
                
                EditorUtility.SetDirty( this );
            }
            
            ClearEmptyEventSlots( );
        }


        public void PackSubAssets()
        {
            for( int i = 0; i < _events.Length; ++i )
            {
                EventTrack e = _events[ i ];
                if( e == null )
                {
                    continue;
                }
                
                float t = (float)i / _events.Length;
                
                EventTrack duplicatedEvent = Instantiate( e ) as EventTrack;
                duplicatedEvent.name = System.Guid.NewGuid( ).ToString( );
                duplicatedEvent.hideFlags = HideFlags.HideInHierarchy;
                _events[ i ] = duplicatedEvent;
                
                if( AssetDatabase.IsMainAsset( e ) )
                {
                    string assetPath = AssetDatabase.GetAssetPath( e );
                    if( string.IsNullOrEmpty( assetPath ) )
                    {
                        continue;
                    }
                    
                    EditorUtility.DisplayProgressBar( "PackSubAssets", "Deleting " + assetPath, t );
                    AssetDatabase.DeleteAsset( assetPath );
                }
                
                EditorUtility.DisplayProgressBar( "PackSubAssets", "Moving " + duplicatedEvent.name, t );
                AssetDatabase.AddObjectToAsset( duplicatedEvent, this );
                
                // 
                AssetDatabase.ImportAsset( AssetDatabase.GetAssetPath( duplicatedEvent ) );
                
                EditorUtility.SetDirty( this );
            }
            
            ClearEmptyEventSlots( );
            
            EditorUtility.ClearProgressBar( );
        }
        

        public void ClearEmptyEventSlots()
        {
            bool dirty = false;
            
            if( _events == null )
                return;
            
            List<EventTrack> newEvents = new List<EventTrack>();
            for( int i = 0, max = _events.Length; i < max; ++i )
            {
                if( _events[i] == null )
                {
                    dirty = true;
                    continue;
                }
                
                newEvents.Add( _events[i] );
            }
            
            
            if( dirty == false )
                return;
            
            _events = newEvents.ToArray( );
            EditorUtility.SetDirty( this );
        }
        
        #endif
    }
}
