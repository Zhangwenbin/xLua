
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LWARS
{

    public class EventTrack : ScriptableObject
    {
        public static EventTrackStatus  CurrentStatus   = null;
        public static float             DeltaTime       = 0;
        
  
        public enum ECategory
        {
            None    = 0,    // 
            Common  ,       // 
            Unit    ,       // 
            UI      ,       // UI
        }
        

        public float        Start       = 0;
        public float        End         = 0;
        

        public virtual ECategory Category
        {
            get { return ECategory.None; }
        }
        

        
        public float GetTimeScale( float time )
        {
            float range = ( End - Start );
            if( range > 0 )
            {
                time = Mathf.Clamp( time, 0, End );
                return ( time - Start ) / range;
            }
            return 1;
        }
        
        public virtual EventTrackStatus CreateStatus( EventPlayerStatus owner )        { return new EventTrackStatus( owner ); }
        public virtual void UpdatePreview( GameObject gobj, float time )               { }
        public virtual void OnStart( MonoBehaviour behaviour )                   { }
        public virtual void OnUpdate( MonoBehaviour behaviour, float time )      { }
        public virtual void OnEnd( MonoBehaviour behaviour )                     { }
        public virtual void OnBackground( MonoBehaviour behaviour, float time )  { }
        

        public virtual bool CheckPreLoad()
        {
            return false;
        }
        

        public virtual bool IsDonePreLoad()
        {
            return true;
        }
        

        public virtual void StartPreLoad()
        {
        }
        

        public virtual void UnloadPreLoad()
        {
        }
        
        #if UNITY_EDITOR
        

        public virtual void EditorStartPreLoad()
        {
        }
        

        public virtual void EditorUnloadPreLoad()
        {
        }
        
        #endif
        

        #if UNITY_EDITOR
        
        bool          m_IsReqTrackStatusUpdate    = false;
        
        public virtual bool     isCustomInspector   { get { return true;                } }
        public virtual Color    TrackColor          { get { return Color.blue;          } }
        public virtual void     OnSceneGUI( SceneView sceneView, GameObject go, float time ){ }
        public virtual void     OnInspectorGUI( Rect position, SerializedObject serializeObject, float width )
        {
            // CustomFieldAttribute.OnInspectorGUI( position, this.GetType(), serializeObject, width );
        }


        public virtual void EditorRelease()
        {
        }
        

        public virtual void EditorPreProcess( MonoBehaviour behaviour, float time, float dt, bool isLooped, bool isEnded )
        {
        }
        

        public virtual void EditorPostProcess( MonoBehaviour behaviour, float time, float dt, bool isLooped, bool isEnded )
        {
        }
        

        protected void RequestTrackStatusUpdate()
        {
            m_IsReqTrackStatusUpdate = true;
        }
        
        public void ResetTrackStatusUpdate()
        {
            m_IsReqTrackStatusUpdate = false;
        }
        
        public bool IsRequestTrackStatusUpdate()
        {
            return m_IsReqTrackStatusUpdate;
        }
        
        static readonly string[] CATEGORY_TEXT_TBL = new string[]
        {
            "none",
            "common",
            "unit",
            "UI",
        };
        
        public static string GetCategoryText( EventTrack evTrack )
        {
            ECategory category = ECategory.None;
            if( evTrack != null )
            {
                category = evTrack.Category;
            }
            return CATEGORY_TEXT_TBL[ (int)category ];
        }
        

        public static bool IsDispPopupList( ECategory own, ECategory target )
        {
            if( own == ECategory.None
            ||  target == ECategory.None )
                return true;
            
            if( own == ECategory.Common
            ||  target == ECategory.Common )
                return true;
            
            return own == target;
        }
        
        #endif
    }
}
