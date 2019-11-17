﻿using System.Collections.Generic;
using System.Linq;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper
{
	internal sealed class GameProcessorContext
	{
		public GameProcessorContext(GameCollection collection)
		{
			Collection = collection;
		}

		public void AddSerializedFile(SerializedFile file, SerializedFileScheme scheme)
		{
			m_files.Add(file, scheme);
		}

		public void ReadSerializedFiles()
		{
			while (m_files.Count > 0)
			{
				SerializedFile file = m_files.First().Key;
				ReadFile(file);
			}
		}

		private void ReadFile(SerializedFile file)
		{
			foreach (FileIdentifier dependency in file.Metadata.Dependencies)
			{
				if (!m_knownFiles.Contains(dependency.FilePath))
				{
					if (Collection.GameFiles.TryGetValue(dependency.FilePath, out SerializedFile dependencyFile))
					{
						ReadFile(dependencyFile);
					}
					else
					{
						m_knownFiles.Add(dependency.FilePath);
					}
				}
			}

			SerializedFileScheme scheme = m_files[file];
			file.ReadData(scheme.Stream);
			scheme.Dispose();
			m_files.Remove(file);
			m_knownFiles.Add(file.Name);
		}

		public GameCollection Collection { get; }

		private Dictionary<SerializedFile, SerializedFileScheme> m_files = new Dictionary<SerializedFile, SerializedFileScheme>();
		private readonly HashSet<string> m_knownFiles = new HashSet<string>();
	}
}