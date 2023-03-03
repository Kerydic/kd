using UnityEditor.SceneTemplate;
using UnityEngine;

namespace KDGame.Editor.Scene
{
	public class BasicSceneTemplatePipeline : ISceneTemplatePipeline
	{
		public bool IsValidTemplateForInstantiation(SceneTemplateAsset sceneTemplateAsset)
		{
			if (sceneTemplateAsset)
			{
				Debug.Log($"Check template valid: {sceneTemplateAsset.name}");
			}

			return true;
		}

		public void BeforeTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, bool isAdditive,
			string sceneName)
		{
			if (sceneTemplateAsset)
			{
				Debug.Log(
					$"Before Template Pipeline {sceneTemplateAsset.name} isAdditive: {isAdditive} sceneName: {sceneName}");
			}
		}

		public void AfterTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset,
			UnityEngine.SceneManagement.Scene scene, bool isAdditive, string sceneName)
		{
			if (sceneTemplateAsset)
			{
				Debug.Log(
					$"After Template Pipeline {sceneTemplateAsset.name} scene: {scene} isAdditive: {isAdditive} sceneName: {sceneName}");
			}
		}
	}
}