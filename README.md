# Unity_IK_HumanoidGaitAnimator

Heavily WIP. Ground work for 2D animated character controller for Unity. Also implements IK for animating character’s legs.  
The work os heavily incomplete and contains debug -functions, duplicate code and code that is commented out("deprecated").  
Generated animation works on variable movement speed, even surfaces and slopes.  

CharacterControl.cs contains code for moving 2D character in 2D environment. It simulates walking, dashing and jumping.  
Contains interfaces for collecting character's state and inputs for changing character’s state.  

IKLegsControl.cs contains code that animates character legs.  
The presumption is that character is rigged and has kinematic model done using Unity’s own rigging tools & 2D inverse kinematics solvers such as CDD or Limb.   

IKCharacterAdapter.cs serves as a bridge between these components.  

[Youtube video of the character in action](https://youtu.be/jqgxD_3iY04)
