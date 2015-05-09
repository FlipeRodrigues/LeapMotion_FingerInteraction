/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * 																			 *
 * Project: Leap Motion Interaction											 *
 * File: HighlightBehaviour.cs												 *
 * Description: Defines how an object reacts to being selected				 *
 * (or deselected)															 *
 * Author: Filipe Miguel Sobreira Rodrigues									 *													 
 * Revision Hystory: - 3/2015 -> first release  							 *                                                     
 * 																			 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using UnityEngine;
using System.Collections;

public class HighlightBehaviour : MonoBehaviour {

// PUBLIC VARIABLES

	// user specifications regarding what highlighting an object looks like
	public Color m_highlightColor;
	public float m_highlightScaleFactor;
	public float m_selectionDelay;

// PRIVATE VARIABLES

	// varaibles related with color affectations	
	private Renderer m_renderer;
	private Color m_defaultColor;

	// stores this object's default scale (can be changed via the scale functionality)
	private Vector3 m_defaultScale;

	// stores the audio that will play on a successful selection
	private AudioSource m_selectionAudio;

	// these are necessary in order to have smooth transitions (in terms of color and scale)
	private float m_selectionTimer;
	private float m_lerp;

	// for efficiency's sake, most of this script only runs if this object's selection status is changing
	private bool m_isChangingSelection;

	// we have to know wheter this object is being selected or deselected to highlight or fade it (respectively)
	private bool m_isSelecting;

	// Use this for initialization
	void Start () {

		m_renderer = this.GetComponent<Renderer> ();

		m_defaultColor = m_renderer.material.color;

		m_defaultScale = this.transform.localScale;
			
		m_selectionAudio = this.GetComponent<AudioSource> ();

		// affect this object's selection audio based on how long it took to select it
		m_selectionAudio.pitch = 1 / (m_selectionDelay * 3);

		m_selectionTimer = m_selectionDelay;
	}

	// This is called via "SendMessage()" from FingerInteraction.cs when this object is being targeted
	void Select (bool isSelecting) {

		// compensate for incomplete selections (prevents abrupt changes in color and scale)
		m_selectionTimer = m_selectionDelay - m_selectionTimer;

		m_isChangingSelection = true;

		m_isSelecting = isSelecting;
	}

	// This is called via "SendMessage()" from FingerInteraction.cs when this object is being scaled
	void SetDefaultScale () {

		m_defaultScale = this.transform.localScale / m_highlightScaleFactor;
	}

	// Update is called once per frame
	void Update () {

		// checks if the specified selection time delay has elapsed
		if (m_selectionTimer > m_selectionDelay) {

			// we don't need to select this object if it is already selected
			if (FingerInteraction.s_currentSelection != this.gameObject) {
			
				if (m_isSelecting) {

					m_selectionAudio.Play ();

					// actual selection happens here
					FingerInteraction.s_currentSelection = this.gameObject;

					FingerInteraction.s_selectedRigidBody = this.GetComponent<Rigidbody> ();

					FingerInteraction.s_selectedRigidBody.isKinematic = false;
				}
				else
					this.GetComponent<Rigidbody> ().isKinematic = this.GetComponent<Rigidbody> ().HasStopped ();
			}

			// makes sure they are exactly the same
			m_selectionTimer = m_selectionDelay;

			m_isChangingSelection = false;
		}
		else
			m_selectionTimer += Time.deltaTime;

		// no need to affect this object if he's not being selected or deselected
		if (!m_isChangingSelection)
			return;

		// allows for smooth transition between colors and scales
		m_lerp = Mathf.PingPong (m_selectionTimer, m_selectionDelay) / m_selectionDelay;
		m_lerp = m_isSelecting ? m_lerp : 1 - m_lerp;
		
		ColorAffectations ();
		
		ScaleAffectations ();
	}

	// Affect this object's color
	void ColorAffectations () {
		
		m_renderer.material.color = Color.Lerp (m_defaultColor, m_highlightColor, m_lerp);
	}

	// Affect this object's scale
	void ScaleAffectations () {

		this.transform.localScale = Vector3.Lerp (m_defaultScale, m_defaultScale * m_highlightScaleFactor, m_lerp);
	}
}