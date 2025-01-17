﻿//Copyright WID Virtual Environments Group 2018-Present
//Authors:Simon Smith, Ross Tredinnick
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WIDVE.IO
{
	public abstract class DataFile : ScriptableObject
	{
		protected const string DEFAULT_FILENAME = "NewDataFile";

		protected const string FALLBACK_FOLDER_NAME = "OutputFiles";

		static string _applicationDataPath = string.Empty;
		/// <summary>
		/// Stores Application.dataPath so it can be accessed from threads.
		/// </summary>
		public static string ApplicationDataPath
		{
			get
			{
				if (string.IsNullOrEmpty(_applicationDataPath))
				{
					try
					{
						_applicationDataPath = Application.dataPath;
					}
					catch(System.ArgumentException)
					{
						//Application.dataPath can't be accessed outside of the main thread
						_applicationDataPath = string.Empty;
					}
				}

				return _applicationDataPath;
			}
		}

		static string _persistentDataPath = string.Empty;
		/// <summary>
		/// Stores Application.persistentDataPath so it can be accessed from threads.
		/// </summary>
		public static string PersistentDataPath
		{
			get
			{
				if (string.IsNullOrEmpty(_persistentDataPath))
				{
					try
					{
						_persistentDataPath = Application.persistentDataPath;
					}
					catch (System.ArgumentException)
					{
						_persistentDataPath = string.Empty;
					}
				}

				return _persistentDataPath;
			}
		}

		static string _streamingAssetsPath = string.Empty;
		/// <summary>
		/// Stores Application.streamingAssetsPath so it can be accessed from threads.
		/// </summary>
		public static string StreamingAssetsPath
		{
			get
			{
				if(string.IsNullOrEmpty(_streamingAssetsPath))
				{
					try
					{
						_streamingAssetsPath = Application.streamingAssetsPath;
					}
					catch(System.ArgumentException)
					{
						_streamingAssetsPath = string.Empty;
					}
				}

				return _streamingAssetsPath;
			}
		}

		static string _fallbackFolderPath = string.Empty;
		/// <summary>
		/// This is where DataFiles will be written to if their specified path does not exist.
		/// </summary>
		protected static string FallbackFolderPath
		{
			get
			{
				if(string.IsNullOrEmpty(_fallbackFolderPath))
				{
					_fallbackFolderPath = System.IO.Path.Combine(ApplicationDataPath, FALLBACK_FOLDER_NAME);
				}
				return _fallbackFolderPath;
			}
		}

		public enum FolderTypes { Absolute, RelativeToApplicationDataPath, RelativeToPersistentDataPath, RelativeToStreamingAssetsPath }

		[SerializeField]
		[HideInInspector]
		[Tooltip("Specifies the root of the folder path.")]
		FolderTypes _folderType = FolderTypes.RelativeToApplicationDataPath;
		/// <summary>
		/// Specifies the root of the folder path.
		/// </summary>
		public FolderTypes FolderType
		{
			get => _folderType;

			private set
			{
				_folderType = value;
				_pathIsValid = false;
			}
		}

		[SerializeField]
		[HideInInspector]
		[Tooltip("Folder where the file is located.")]
		string _folder = string.Empty;
		/// <summary>
		/// Folder where the file is located.
		/// </summary>
		public string Folder
		{
			get
			{
				//don't allow writing to a blank folder...
				if(FolderType == FolderTypes.Absolute && string.IsNullOrEmpty(_folder))
				{
					_folder = ApplicationDataPath;
				}
				return _folder;
			}

			set
			{
				_folder = value;
				_pathIsValid = false;
			}
		}

		[SerializeField]
		[HideInInspector]
		[Tooltip("Create the folder if it does not exist?")]
		bool _createFolder = true;
		bool CreateFolder => _createFolder;

		[SerializeField]
		[HideInInspector]
		[Tooltip("Filename, with or without extension.")]
		string _filename = DEFAULT_FILENAME;
		/// <summary>
		/// Filename the DataFile will be written to.
		/// </summary>
		public string Filename
		{
			get => _filename;

			set
			{
				_filename = value;
				_pathIsValid = false;
			}
		}
		string FilenameNoExt => System.IO.Path.GetFileNameWithoutExtension(Filename);

		bool _pathIsValid = false;
		bool PathIsValid => _pathIsValid;

		string _path;
		/// <summary>
		/// Full filepath the DataFile will be written to.
		/// </summary>
		public string Path
		{
			get
			{   
				//construct and validate the full filepath
				if (!PathIsValid)
				{	
					//get full path to folder
					string folderPath = GetFolderPath(Folder, FolderType);

					//check that this folder exists
					if (!Directory.Exists(folderPath))
					{
						if (CreateFolder)
						{
							//create the folder
							Directory.CreateDirectory(folderPath);
						}
						else
						{
							//write to the default folder
							folderPath = FallbackFolderPath;

							//if the default folder does not exist, create it
							if(!Directory.Exists(folderPath))
							{
								Directory.CreateDirectory(folderPath);
							}
						}
					}

					//check that filename ends in the correct extension
					string filename = Filename;
					if (!string.IsNullOrEmpty(Extension) && 
						!filename.EndsWith(Extension, ignoreCase: true, System.Globalization.CultureInfo.CurrentCulture))
					{
						filename += '.' + Extension.Trim('.');
					}

					//combine folder and filename together to get the full path
					_path = System.IO.Path.Combine(folderPath, filename);

					_pathIsValid = true;
				}

				return _path;
			}
		}

		/// <summary>
		/// Returns the path relative to the root folder specified by FolderType.
		/// </summary>
		public string RelativePath
		{
			get
			{
				int i;
				switch(FolderType)
				{
					default:
					case FolderTypes.Absolute:
						return Path;

					case FolderTypes.RelativeToApplicationDataPath:
						i = Path.IndexOf(ApplicationDataPath);
						return Path.Remove(i, ApplicationDataPath.Length);

					case FolderTypes.RelativeToPersistentDataPath:
						i = Path.IndexOf(PersistentDataPath);
						return Path.Remove(i, PersistentDataPath.Length);

					case FolderTypes.RelativeToStreamingAssetsPath:
						i = Path.IndexOf(StreamingAssetsPath);
						return "Assets/StreamingAssets" + Path.Remove(i, StreamingAssetsPath.Length);
				}
			}
		}

		/// <summary>
		/// Returns true if the file exists on disk.
		/// </summary>
		public bool FileExists => File.Exists(Path);

		public enum FileFormats { Binary, Text }

		/// <summary>
		/// Specifies whether this is a binary or a text file.
		/// </summary>
		public abstract FileFormats FileFormat { get; }

		/// <summary>
		/// File extension used by this DataFile.
		/// </summary>
		public abstract string Extension { get; }

		public enum WriteModes { AlwaysOverwrite, OverwriteThenAppend, Append, ReadOnly, Increment }

		[SerializeField]
		[HideInInspector]
		[Tooltip("AlwaysOverwrite: File will be overwritten each write.\n" +
				 "OverwriteThenAppend: File will be overwritten on first write, then appended afterwards.\n" +
				 "Append: File will be appended each write.\n" +
				 "ReadOnly: Trying to write to this file will do nothing.\n" +
				 "Increment: Filename will be incremented if the file already exists.")]
		WriteModes _writeMode = WriteModes.Append;
		/// <summary>
		/// Specifies how this DataFile will be written to.
		/// </summary>
		public WriteModes WriteMode => _writeMode;

		/// <summary>
		/// Tracks how many times the file has been opened for writing during this run of the program.
		/// </summary>
		protected uint TimesOpened = 0;

		/// <summary>
		/// Call this before any DataFiles are used outside of the main thread.
		/// <para>Need to do this since Application data paths can't be accessed directly in a separate thread.</para>
		/// </summary>
		public static void StoreApplicationDataPaths()
		{
			_applicationDataPath = ApplicationDataPath;
			_persistentDataPath = PersistentDataPath;
			_streamingAssetsPath = StreamingAssetsPath;
		}

		/// <summary>
		/// Returns the folder's full path according to the given FolderType.
		/// </summary>
		/// <param name="folder">Name of folder.</param>
		/// <param name="folderType">Type of folder path.</param>
		/// <returns>Absolute path to folder.</returns>
		public static string GetFolderPath(string folder, FolderTypes folderType)
		{
			switch (folderType)
			{
				case FolderTypes.Absolute:
					return folder;

				default:
				case FolderTypes.RelativeToApplicationDataPath:
					return System.IO.Path.Combine(ApplicationDataPath, folder);

				case FolderTypes.RelativeToPersistentDataPath:
					return System.IO.Path.Combine(PersistentDataPath, folder);

				case FolderTypes.RelativeToStreamingAssetsPath:
					return System.IO.Path.Combine(StreamingAssetsPath, folder);
			}
		}

		/// <summary>
		/// Returns true if the DataFile should overwrite, false otherwise.
		/// </summary>
		/// <param name="writeMode">Specified writing mode.</param>
		/// <param name="timesOpened">Number of times the DataFile has been opened.</param>
		protected static bool ShouldOverwrite(WriteModes writeMode, uint timesOpened)
		{
			switch (writeMode)
			{
				default:
				case WriteModes.ReadOnly:
					//no writing
					return false;

				case WriteModes.Append:
					//don't overwrite, just append
					return false;

				case WriteModes.AlwaysOverwrite:
					//overwrite the entire file every time something is written
					return true;

				case WriteModes.OverwriteThenAppend:
					//overwrite if the file has not been opened before for this run of the program, otherwise append
					return timesOpened == 0;
			}
		}

		/// <summary>
		/// Writes the given DataContainers to the file.
		/// <para>Called by the DataWriter class. Leave this blank if not using DataContainers.</para>
		/// </summary>
		/// <param name="buffer">Array of ordered arrays of DataContainers.</param>
		public virtual void WriteData(DataContainer[][] buffer) { }

		/// <summary>
		/// If WriteMode is Increment, increments the filename until it will not overwrite an existing file.
		/// </summary>
		protected virtual void IncrementFilename()
		{
			if(WriteMode != WriteModes.Increment) return;

			while(FileExists)
			{
				//increment filename if file already exists
				int i;
				Filename = FilenameNoExt;
				string[] filenameSplit = Filename.Split('_');
				if(int.TryParse(filenameSplit[filenameSplit.Length - 1], out i))
				{
					//replace number at end of filename
					i++;
					filenameSplit[filenameSplit.Length - 1] = $"{i}";
					Filename = string.Join("_", filenameSplit);
				}
				else
				{
					//no number in filename already
					i = 1;
					Filename += $"_{i}";
				}
			}
		}

#if UNITY_EDITOR
		protected virtual void DrawExtension(SerializedObject serializedObject)
		{
			GUI.enabled = false;
			EditorGUILayout.TextField(label: nameof(Extension), text: Extension);
			GUI.enabled = true;
		}

		[CanEditMultipleObjects]
		[CustomEditor(typeof(DataFile), true)]
		public class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				DataFile dataFile = target as DataFile;

				serializedObject.Update();

				SerializedProperty writeMode = serializedObject.FindProperty(nameof(_writeMode));

				EditorGUI.BeginChangeCheck();

				EditorGUILayout.LabelField(target.GetType().Name, EditorStyles.boldLabel);
				
				//draw any non-hidden fields from child classes
				DrawDefaultInspector();

				//these fields are hidden by default so they don't get draw twice
				SerializedProperty folderType = serializedObject.FindProperty(nameof(_folderType));
				EditorGUILayout.PropertyField(folderType);

				//don't draw create folder if file is read only...
				if(writeMode.enumValueIndex != (int)WriteModes.ReadOnly)
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_createFolder)));
				}

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_folder)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_filename)));

				//draw the extension - this may be handled differently by child classes
				dataFile.DrawExtension(serializedObject);

				EditorGUILayout.PropertyField(writeMode);

				/*
				//show the asset if it is part of the project
				bool fileFound = false;
				if(folderType.enumValueIndex == (int)FolderTypes.RelativeToApplicationDataPath ||
				   folderType.enumValueIndex == (int)FolderTypes.RelativeToStreamingAssetsPath)
				{
					GUI.enabled = false;

					TextAsset file = AssetDatabase.LoadAssetAtPath<TextAsset>(System.IO.Path.GetFileNameWithoutExtension(dataFile.RelativePath));
					if(file)
					{
						EditorGUILayout.ObjectField(file, typeof(TextAsset), allowSceneObjects: false);
						fileFound = true;
					}
					else
					{
						EditorGUILayout.LabelField("No file found at specified path!");
						fileFound = false;
					}

					GUI.enabled = true;
				}

				//otherwise, show the full filepath
				if(!fileFound)
				{
					GUI.enabled = false;

					EditorGUILayout.LabelField($"Path: {dataFile.Path}", new GUIStyle() { wordWrap = true });

					GUI.enabled = true;
				}
				*/

				bool changed = EditorGUI.EndChangeCheck();

				serializedObject.ApplyModifiedProperties();

				if (changed)
				{  
					foreach(DataFile df in targets)
					{
						//no extensions in filenames!
						df._filename = System.IO.Path.GetFileNameWithoutExtension(df._filename);

						//if something changed, probably need to update the path
						df._pathIsValid = false;

						EditorUtility.SetDirty(df);
					}
				}
			}
		}
#endif
	}
}