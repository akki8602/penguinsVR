﻿//Copyright WID Virtual Environments Group 2018-Present
//Authors:Simon Smith, Ross Tredinnick
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.Graphics
{
	[ExecuteAlways]
	public class Fader : RendererController
	{
		enum Modes { SingleMaterial, SeparateMaterials }

		[SerializeField]
		[HideInInspector]
		Modes _fadeMode = Modes.SingleMaterial;
		Modes FadeMode => _fadeMode;

		[SerializeField]
		[HideInInspector]
		Material _opaqueMaterial;
		Material OpaqueMaterial => _opaqueMaterial;

		[SerializeField]
		[HideInInspector]
		Material _transparentMaterial;
		Material TransparentMaterial => _transparentMaterial;

		[SerializeField]
		[HideInInspector]
		[Range(0, 1)]
		float _minAlpha = 0;
		float MinAlpha => _minAlpha;

		[SerializeField]
		[HideInInspector]
		[Range(0, 1)]
		float _maxAlpha = 1;
		float MaxAlpha => _maxAlpha;

		Material CurrentMaterial;

		public void SetAlpha(float alpha, bool trackValue=true)
		{
			if(!ShaderProperties) return;

			if(FadeMode == Modes.SeparateMaterials)
			{
				if(!OpaqueMaterial || !TransparentMaterial) return;

				//set opaque or transparent material
				Material m = Mathf.Approximately(alpha, 1) ? OpaqueMaterial : TransparentMaterial;
				if(CurrentMaterial != m)
				{
					SetMaterial(m);
					CurrentMaterial = m;
				}
			}

			for(int i = 0; i < Renderers.Length; i++)
			{
				//for each renderer:
				Renderer r = Renderers[i];

				//just skip null/missing renderers for now - not sure how to keep the list up to date in every situation yet
				if(!r) continue;

				//turn renderer on or off
				if(Mathf.Approximately(alpha, 0f))
				{
					if(r.enabled) r.enabled = false;
				}
				else if(!r.enabled) r.enabled = true;

				//initialize property block with default material values
				ShaderProperties.SetProperties(MPB, r.sharedMaterial, clear: true);

				//add per-renderer colors back in
				PerRendererColor prc = r.GetComponent<PerRendererColor>();
				if(prc && prc.enabled) MPB.SetColor(prc.ColorName, prc.Color);

				//scale alpha based on min/max alpha
				float scaledAlpha = Mathf.Lerp(MinAlpha, MaxAlpha, alpha);

				//set alpha value of all colors
				ShaderProperties.SetAlpha(MPB, scaledAlpha);

				//apply property block
				r.SetPropertyBlock(MPB);
			}

			if(trackValue) CurrentValue = alpha;
		}

		public override void SetValue(float value)
		{
			SetAlpha(value);
		}

		void OnEnable()
		{
			//refresh fading
			SetAlpha(CurrentValue);
		}

		void OnDisable()
		{
			//remove any fading
			SetAlpha(1, false);
			RemoveMaterialPropertyBlock();
		}

#if UNITY_EDITOR
		[CanEditMultipleObjects]
		[CustomEditor(typeof(Fader), true)]
		new class Editor : RendererController.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				serializedObject.Update();

				SerializedProperty fadeMode = serializedObject.FindProperty(nameof(_fadeMode));
				EditorGUILayout.PropertyField(fadeMode);

				if(fadeMode.enumValueIndex == (int)Modes.SeparateMaterials)
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_opaqueMaterial)));
					EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_transparentMaterial)));
				}

				EditorGUI.BeginChangeCheck();

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_minAlpha)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_maxAlpha)));

				if(EditorGUI.EndChangeCheck())
				{
					//refresh fading
					foreach(Fader f in targets)
					{
						f.SetAlpha(f.CurrentValue);
					}
				}

				serializedObject.ApplyModifiedProperties();
			}
		}
#endif
	}
}