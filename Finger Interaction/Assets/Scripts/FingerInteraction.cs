/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * 																			 *
 * Project: Leap Motion Interaction											 *
 * File: FingerInteraction.cs												 *
 * Description: Defines how the user interacts with the interface using      *
 * the Leap Motion (hand-tracking device)									 *
 * Author: Filipe Miguel Sobreira Rodrigues									 *													 
 * Revision Hystory: - 3/2015 -> first release  							 *                                                     
 * 																			 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * INSTRUCTIONS:  															 *
 *																			 *
 * - When the scene loads all interactable objects are selected by default;  *
 * - Depending on how many fingers the user has exetended, different         *
 * actions become available;												 *
 * - 0 fingers extended (closed fist): Purposely does nothing (left empty	 *
 * by design);																 *
 * - 1 finger extended (any finger except thumb): Select individual			 *
 * interactable objects;													 *
 * - 2 fingers extended (any 2 fingers): Translate the current selection	 *
 * based on the velocity of the tips of the extended fingers;				 *
 * - 3 fingers extended (any 2 fingers): Rotate the current selection		 *
 * based on the velocity of the tips of the extended fingers;				 *
 * - 4 fingers extended: Currently does nothing. Ideas? It's not a very 	 *
 * confortable stance, plus leap doesn't recognize it very well;			 *
 * - 5 fingers extended (open hand): Scale the current selection			 *
 * based on the velocity of the tips of the extended fingers;			 	 *
 *                                                       					 *						*
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using UnityEngine;
using System.Collections;
using Leap;

/* FIX / IMPROVE ME:
 * 
 * - RECOMMENT (PHYSX UPDATE)
 * - assign senssable masses at Start()
 * - Confidence use?
 * - Compare with THREE
 * - UI telling how many fingers are extended
 * - tool pointing? (why isn't it working?)
 */

public class FingerInteraction : MonoBehaviour {

// STATIC VARIABLES

	// stores the current selection
	public static GameObject s_currentSelection;

	// we're dealing with physX when we add movement / rotation
	public static Rigidbody s_selectedRigidBody;

// PUBLIC VARIABLES:

	// determines the maximum selection distance
	public float m_selectionRange = 100f;

	// sensitivity related variables
	public float m_translateSensitivity = 50f;
	public float m_rotateSensitivity = 10f;
	public float m_scaleSensitivity = 10f;

// PRIVATE VARIABLES:

	// leap related variables
	private HandController m_handController;
	private HandModel m_handModel;
	private Hand m_leapHand;
	private PointableList m_extendedPointables;

	// variables related with positional corrections
	private Vector3 m_controllerPosition;
	private float m_leapScaleFactor;

	// variables concerning interactable objects (tags & layer)
	private GameObject m_interactables;
	private int m_interactableMask;

	// selection related variables
	private GameObject m_previousTargetSelection;
	private GameObject m_targetSelection;

	// light that aids in the selection process
	private Light m_pointer;

	// variables related with avoiding abrupt (and unintentional) transitions between stances
	private float m_transitionDelay = 0.1f;
	private float m_transitionTimer = 0;

	// counter variables
	private int m_cExtendedPointables;
	private int m_cChildren;

	// Use this for referencing
	void Awake () {

		m_handModel = transform.GetComponent<HandModel> ();

		m_interactables = GameObject.FindGameObjectWithTag ("Interactables");
		m_interactableMask = LayerMask.GetMask ("Interactable");

		m_pointer = GameObject.FindGameObjectWithTag ("Pointer").GetComponent<Light> ();
	}

	// Use this for initialization
	void Start () {

		m_handController = m_handModel.GetController ();

		// needed for future rectifications
		m_controllerPosition = m_handController.transform.position;
		m_leapScaleFactor = (m_handController.transform.localScale.x + m_handController.transform.localScale.y + m_handController.transform.localScale.z) / 3;

		// start with all interactable objects assigned to all target variables (avoid null references)
		m_previousTargetSelection = m_targetSelection = s_currentSelection = m_interactables;

		s_selectedRigidBody = s_currentSelection.GetComponent<Rigidbody> ();
	}

	// Called every fixed frame rate (deal with physX here)
	void FixedUpdate () {

		m_leapHand = m_handModel.GetLeapHand ();

		// get a list of pointables containing the extended ones
		m_extendedPointables = m_leapHand.Pointables.Extended ();

		m_cChildren = s_currentSelection.transform.childCount;

		// if the number of extended pointables changes, reset our transition timer
		if (m_cExtendedPointables != m_extendedPointables.Count) {
			m_cExtendedPointables = m_extendedPointables.Count;
			m_transitionTimer = 0;
		}

		m_transitionTimer += Time.deltaTime;

		// avoids abrupt transitions between stances (good for when leap tracking goes bananas)
		if (m_transitionTimer < m_transitionDelay)
			return;

		// user actions depend on the amount of pointables / fingers extended
		switch (m_cExtendedPointables) {

			case 0:
				// gives the user the possibility to reposition his/her hand while maintaining the current selection
				return;
				break;

			case 1:
				// select an individual interactable object
				SelectTarget ();
				break;

			case 2:
				// translate the selected interactable/s
				TranslateSelection ();
				break;

			case 3:
				// rotate the selected interactable/s
				RotateSelection ();
				break;
	
			case 4:
				// leap has trouble recognizing this stance
				break;
			
			case 5:
				// scale the selected interactable/s
				ScaleSelection ();
				break;
		}

		// always called if not selecting
		if (m_cExtendedPointables != 1) {

			// turn off the light aid 
			m_pointer.intensity = 0;

			// fade out incomplete selections and return to previous selection
			if (m_targetSelection.transform.childCount == 0) {

				if (m_targetSelection != s_currentSelection) {

					m_targetSelection.SendMessage ("Select", false);

					if (m_cChildren == 0)
						s_currentSelection.SendMessage ("Select", true);

					m_targetSelection = s_currentSelection;
				}
			}
		}
	}

	// Selects the object/s with which to interact (ACTUAL SELECTION OCCURS ON THE OBJECT ITSELF)
	void SelectTarget () {

		// get the 1st and only extended pointable (finger / tool) -> our selector
		Pointable s = m_extendedPointables [0];

		// thumb selections were conflicting with the closed fist stance (0 fingers extended)
		if (s.Equals (m_leapHand.Fingers [0]))
			return;

		Vector3 sTipPosition = s.StabilizedTipPosition.ToUnityScaled () * m_leapScaleFactor + m_controllerPosition;
		Vector3 sDirection = s.Direction.ToUnity ();

		// our ray starts at the tip of our selector and shares its direction
		Ray selectionRay = new Ray (sTipPosition, sDirection);
		RaycastHit selectionHit;

		m_previousTargetSelection = m_targetSelection;

		// if we hit an interactable it becomes our provisional target, otherwise all interactables do
		if (Physics.Raycast (selectionRay, out selectionHit, m_selectionRange, m_interactableMask))
			m_targetSelection = selectionHit.transform.gameObject;

		// if we switch provisional selections, highlight the new one and fade out the precious one
		if (m_previousTargetSelection != m_targetSelection) {

			if (m_previousTargetSelection.transform.childCount == 0)
				m_previousTargetSelection.SendMessage ("Select", false);
			
			if (m_targetSelection.transform.childCount == 0)
				m_targetSelection.SendMessage ("Select", true);
		}

		// update the pointer so that it shows where the user is pointing
		m_pointer.transform.position = sTipPosition;
		m_pointer.transform.rotation = Quaternion.LookRotation (sDirection);
		m_pointer.range = selectionHit.distance + 10;	// minor tweak so that the light doesn't go through objects
		m_pointer.intensity = 8;
	}
	
	// Translates the selected object/s
	void TranslateSelection() {

		// calculate the mean velocity of our translators' tips
		Vector3 meanVelocity = Vector3.zero;
		foreach (Pointable translator in m_extendedPointables)
			meanVelocity += translator.TipVelocity.ToUnityScaled () / m_extendedPointables.Count;

		Vector3 force = meanVelocity * m_translateSensitivity;

		// smooth transition between current position and target one
		s_selectedRigidBody.AddForce (force);
	}

	// Rotate the selected object/s
	void RotateSelection() {

		// calculate the mean velocity of our rotators' tips
		Vector3 meanVelocity = Vector3.zero;
		foreach (Pointable rotator in m_extendedPointables)
			meanVelocity += rotator.TipVelocity.ToUnityScaled () / m_extendedPointables.Count;

		Vector3 torque = Vector3.Cross (meanVelocity, Vector3.down) * m_rotateSensitivity;

		// rotate selection around an axis that is perpendicular to both the down direction and the mean tip velocity (right hand rule)
		s_selectedRigidBody.AddTorque (torque);
	}

	// Scale the selected object/s
	void ScaleSelection() {

		// calculate the mean velocity of our scalers' tips
		Vector3 meanVelocity = Vector3.zero;
		foreach (Pointable scaler in m_extendedPointables)
			meanVelocity += scaler.TipVelocity.ToUnityScaled () / m_extendedPointables.Count;

		Vector3 currentScale = s_currentSelection.transform.localScale;

		// only the z component of velocity is being pondered at the moment
		Vector3 targetScale = currentScale * (1 + meanVelocity.z);

		// smooth transition between current scale and target one
		s_currentSelection.transform.localScale = Vector3.Lerp (currentScale, targetScale, m_scaleSensitivity * Time.deltaTime);

		// tell our selection that its default scale changed (for highlighting purposes)
		if (m_cChildren == 0)
			s_currentSelection.SendMessage ("SetDefaultScale");
	}

	// Called when this behaviour becomes inactive or disabled
	void OnDisable () {

		// make sure our selection doesn't stay highlited if the hand is destroyed
		if (m_targetSelection != null && m_targetSelection.transform.childCount == 0)
			m_targetSelection.SendMessage ("Select", false);
	}
}