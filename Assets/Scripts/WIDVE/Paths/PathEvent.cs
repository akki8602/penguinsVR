﻿//Copyright WID Virtual Environments Group 2018-Present
//Authors:Simon Smith, Ross Tredinnick
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Paths
{
	public class PathEvent : MonoBehaviour, IPathEvent
	{
		[SerializeField]
		PathPosition _pathPosition;
		PathPosition PathPosition => _pathPosition;

		[SerializeField]
		UnityEvent _onTrigger;
		public UnityEvent OnTrigger => _onTrigger;

		public float Position => PathPosition ? PathPosition.Position : 0f;

		public void Trigger(PathTrigger trigger)
		{
			OnTrigger?.Invoke();
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(PathEvent))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				if(Application.isPlaying)
				{
					if(GUILayout.Button("Trigger"))
					{
						foreach(PathEvent pe in targets)
						{
							pe.Trigger(null);
						}
					}
				}
			}
		}
#endif
	}
}