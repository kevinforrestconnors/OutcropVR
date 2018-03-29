using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToggleModels : MonoBehaviour {

	private SteamVR_TrackedObject trackedObj;

	private SteamVR_Controller.Device Controller
	{
		get { return SteamVR_Controller.Input((int)trackedObj.index); }
	}

	private static List<string> ignoredModels = new List<string> {"[SteamVR]", "Player", "EventSystem", "Directional Light", "VoiceManager", "Red Laser(Clone)", "Green Laser(Clone)", "Yellow Laser(Clone)", "OutcropVRLine(Clone)", "OutcropVRPlane(Clone)"};

	private static int numModels = 0;
	private static int currentModel = 0;

	private static bool attitudesActive = true;

	void ToggleAllModels() {

		List<GameObject> rootObjects = new List<GameObject>();
		Scene scene = SceneManager.GetActiveScene();
		scene.GetRootGameObjects(rootObjects);

		// sort list to ensure the same order every time, since GetRootGameObjects is not guaranteed to return objects in the same order every time
		List<GameObject> sortedList = rootObjects.OrderBy (go => go.name).ToList ();

		int index = 0;

		for (int i = 0; i < sortedList.Count; i++) {

			GameObject gameObject = sortedList [i];

			// only iterate through models, not gameobjects like the controllers or camera
			if (!ignoredModels.Contains(gameObject.name)) {

				if (index == currentModel) {
					gameObject.SetActive (true);
				} else {
					gameObject.SetActive (false);
				}

				index += 1;
			}
		}

		currentModel += 1;
		if (currentModel == numModels) {
			currentModel = 0;
		}
	}

	void ToggleAttitudes() {

		attitudesActive = !attitudesActive;

		List<GameObject> rootObjects = new List<GameObject>();
		Scene scene = SceneManager.GetActiveScene();
		scene.GetRootGameObjects(rootObjects);

		for (int i = 0; i < rootObjects.Count; i++) {

			GameObject gameObject = rootObjects [i];

			if (gameObject.name == "OutcropVRLine(Clone)" || gameObject.name == "OutcropVRPlane(Clone)") {

				gameObject.SetActive (attitudesActive);
			}
		}
	}

	// Use this for initialization
	void Awake () {
		trackedObj = GetComponent<SteamVR_TrackedObject>();

		List<GameObject> rootObjects = new List<GameObject>();
		Scene scene = SceneManager.GetActiveScene();
		scene.GetRootGameObjects(rootObjects);

		for (int i = 0; i < rootObjects.Count; i++) {
			GameObject gameObject = rootObjects [i];
			if (!ignoredModels.Contains(gameObject.name)) {
				numModels += 1;
			}
		}

		currentModel = numModels - 1;
	}
	
	// Update is called once per frame
	void Update () {
		if (Controller.GetHairTriggerDown ()) {
			ToggleAllModels ();
		}
		if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
		{
			ToggleAttitudes ();
		}
	}
}
