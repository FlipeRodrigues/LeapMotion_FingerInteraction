# LeapMotion_FingerInteracton
Leap Motion oriented UI based on how many fingers the user has extended

 INSTRUCTIONS:
- When the scene loads all interactable objects are selected by default;  
- Depending on how many fingers the user has exetended, different actions become available;
- 0 fingers extended (closed fist): Purposely does nothing (left empty by design, gives the user a stance in which he can reposition his/her hand without affecting the current selection);							 
- 1 finger extended (any finger except thumb): Select individual interactable objects;
- 2 fingers extended (any 2 fingers): Translate the current selection	based on the velocity of the tips of the extended fingers;
- 3 fingers extended (any 2 fingers): Rotate the current selection based on the velocity of the tips of the extended fingers;
- 4 fingers extended: Currently does nothing. Ideas? It's not a very confortable stance, plus leap doesn't recognize it very well;
- 5 fingers extended (open hand): Scale the current selection based on the velocity of the tips of the extended fingers;	
