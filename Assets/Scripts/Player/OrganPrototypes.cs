using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OrganPrototypes : MonoBehaviour {
	public List<CreatureBodySegment> segmentPrototypes = new List<CreatureBodySegment>();
	public List<CreatureLimb> limbPrototypes = new List<CreatureLimb>();
	public List<CreatureAppendage> appendagePrototypes = new List<CreatureAppendage>();
	public static bool isInitialized = false;
	
	public static OrganPrototypes Instance { get; private set; }

	public void Awake(){
		if(Instance != null && Instance != this){
			Destroy(gameObject);
		}
		Instance = this;
		DontDestroyOnLoad(transform.gameObject);

		RegisterOrgans();
		isInitialized = true;
	}

	public CreatureBodySegment LoadSegment(int index){
		CreatureBodySegment segment = new CreatureBodySegment();
		CreatureBodySegment source = segmentPrototypes[index];

		segment.name = source.name;
		segment.prototypeIndex = source.prototypeIndex;
		segment.segmentType = source.segmentType;
		segment.animationControllerName = source.animationControllerName;
		segment.animations = source.animations;
		segment.segmentOffsets = source.segmentOffsets;
		
		return segment;
	}

	public void AttachSegment(Creature root, CreatureBodySegment segment, Vector3 offset){
		Transform parentTransform;
		GameObject obj;
		parentTransform = root.display;
		obj = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/CreatureDisplayNode"));
		obj.transform.parent = parentTransform.transform;
		segment.basePosition = offset;
		obj.GetComponent<CreatureDisplayNode>().rootSegment = segment;
		obj.GetComponent<SpriteRenderer>().transform.localPosition = segment.basePosition;
		obj.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(segment.animationControllerName);
		if(root.segments.Count > 0){
			CreatureBodySegment previous = root.segments[root.segments.Count -1];
			obj.transform.parent = previous.obj.transform;
			segment.previousSegment = previous;
			previous.nextSegment = segment;
			segment.basePosition = obj.transform.localPosition;
		}
		segment.obj = obj;
		segment.root = root;
		root.segments.Add(segment);
	}

	public CreatureLimb LoadLimb(int index){
		CreatureLimb organ = new CreatureLimb();
		CreatureLimb source = limbPrototypes[index];
		CombatAction act;

		organ.name = source.name;
		organ.prototypeIndex = source.prototypeIndex;
		organ.limbType = source.limbType;
		organ.animationControllerName = source.animationControllerName;
		organ.appendageOffsets = source.appendageOffsets;
		organ.animations = source.animations;
		
		//create combat action for the limb
		for(int i=0;i<source.combatActions.Count;i++){
			act = new CombatAction();
			act.limb = organ;
			act.name = source.combatActions[i].name;
			act.range = source.combatActions[i].range;
			act.damage = source.combatActions[i].damage;
			act.windupDuration = source.combatActions[i].windupDuration;
			act.attackDuration = source.combatActions[i].attackDuration;
			act.cooldownDuration = source.combatActions[i].cooldownDuration;
			act.windupAnimation = source.combatActions[i].windupAnimation;
			act.attackAnimation = source.combatActions[i].attackAnimation;
			act.backswingAnimation = source.combatActions[i].backswingAnimation;
			organ.combatActions.Add(act);
		}
		return organ;
	}

	public void AttachLimb(CreatureBodySegment segment, CreatureLimb limb, Vector3 offset){
		limb.offset = offset;
		GameObject limbObject = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/CreatureDisplayNode"));
		limbObject.transform.parent = segment.obj.transform;
		limbObject.GetComponent<CreatureDisplayNode>().root = limb;
		limbObject.GetComponent<SpriteRenderer>().transform.localPosition = offset;
		limbObject.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(limb.animationControllerName);
		limb.obj = limbObject;
		limb.root = segment.root;
		segment.limbs.Add(limb);
	}

	public CreatureAppendage LoadAppendage(int index){
		CreatureAppendage appendage = new CreatureAppendage();
		CreatureAppendage source = appendagePrototypes[index];
		CombatAction act;

		appendage.name = source.name;
		appendage.prototypeIndex = source.prototypeIndex;
		appendage.animationControllerName = source.animationControllerName;
		appendage.animations = source.animations;
		
		
		//create combat action for the limb
		for(int i=0;i<source.combatActions.Count;i++){
			act = new CombatAction();
			act.name = source.combatActions[i].name;
			act.range = source.combatActions[i].range;
			act.damage = source.combatActions[i].damage;
			act.windupDuration = source.combatActions[i].windupDuration;
			act.attackDuration = source.combatActions[i].attackDuration;
			act.cooldownDuration = source.combatActions[i].cooldownDuration;
			act.windupAnimation = source.combatActions[i].windupAnimation;
			act.attackAnimation = source.combatActions[i].attackAnimation;
			act.backswingAnimation = source.combatActions[i].backswingAnimation;
			appendage.combatActions.Add(act);
		}
		return appendage;
	}

	public void AttachAppendage(CreatureLimb organ, CreatureAppendage appendage){
		GameObject obj;
		Transform display = organ.root.display;
		obj = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/CreatureDisplayNode"));
		obj.transform.parent = organ.obj.transform;
		obj.GetComponent<SpriteRenderer>().transform.position = new Vector3(display.transform.position.x + organ.offset.x, display.transform.position.y + organ.offset.y, organ.offset.z);
		obj.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(appendage.animationControllerName);
		for(int i=0;i<appendage.combatActions.Count;i++){
			appendage.combatActions[i].limb = organ;
		}
		appendage.obj = obj;
		appendage.root = organ.root;
		appendage.offset = organ.appendageOffsets[0];
		organ.appendage = appendage;
	}

	private void RegisterOrgans(){
		XmlProcessor xml = new XmlProcessor("Text/OrganList");
		string node;
		while(!xml.IsDone()){
			node = xml.getNextNode();
			switch(node){
			case "OrganSegmentNode":
				readOrganSegmentNode(xml);
				break;
			case "OrganLimbNode":
				readOrganLimbNode(xml);
				break;
			case "OrganAppendageNode":
				readOrganAppendageNode(xml);
				break;
			default:
				break;
			}
		}
	}

	private void readOrganSegmentNode(XmlProcessor xml){
		string node;
		CreatureBodySegment segment = new CreatureBodySegment();
		//Get Attributes
		segment.name = "default";
		segment.segmentType = "none";
		segment.animationControllerName = "none";

		XmlAttribute attribute = xml.getNextAttribute();

		while(attribute.name != ""){
			switch(attribute.name){
				case "name":
					segment.name = attribute.value;
					break;
				case "segmentType":
					segment.segmentType = attribute.value;
					break;
				case "animationControllerName":
					segment.animationControllerName = attribute.value;
					break;
			}
			attribute = xml.getNextAttribute();
		}

		//Get Subnodes
		node = xml.getNextNode();
		while(node != "/OrganSegmentNode"){
			switch(node){
			case "OffsetNode":
				segment.segmentOffsets.Add(readOffsetNode(xml));
				break;
			case "AnimationNode":
				segment.animations.Add(readAnimationNode(xml));
				break;
			}
			node = xml.getNextNode();
		}
		segment.prototypeIndex = segmentPrototypes.Count;
		segmentPrototypes.Add(segment);
	}

	private void readOrganLimbNode(XmlProcessor xml){
		string node;
		CreatureLimb limb = new CreatureLimb();
		//Get Attributes
		limb.name = "default";
		limb.limbType = "none";
		limb.animationControllerName = "none";

		XmlAttribute attribute = xml.getNextAttribute();

		while(attribute.name != ""){
			switch(attribute.name){
				case "name":
					limb.name = attribute.value;
					break;
				case "limbType":
					limb.limbType = attribute.value;
					break;
				case "animationControllerName":
					limb.animationControllerName = attribute.value;
					break;
			}
			attribute = xml.getNextAttribute();
		}

		//Get Subnodes
		node = xml.getNextNode();
		while(node != "/OrganLimbNode"){
			switch(node){
			case "OffsetNode":
				limb.appendageOffsets.Add(readOffsetNode(xml));
				break;
			case "AnimationNode":
				limb.animations.Add(readAnimationNode(xml));
				break;
			case "ActionNode":
				limb.combatActions.Add(readActionNode(xml));
				break;
			}
			node = xml.getNextNode();
		}
		limb.prototypeIndex = limbPrototypes.Count;
		limbPrototypes.Add(limb);
	}

	private void readOrganAppendageNode(XmlProcessor xml){
		string node;
		CreatureAppendage appendage = new CreatureAppendage();
		//Get Tags
		appendage.name = "default";
		appendage.animationControllerName = "none";

		XmlAttribute attribute = xml.getNextAttribute();

		while(attribute.name != ""){
			switch(attribute.name){
				case "name":
					appendage.name = attribute.value;
					break;
				case "animationControllerName":
					appendage.animationControllerName = attribute.value;
					break;
			}
			attribute = xml.getNextAttribute();
		}

		//Get Subnodes
		node = xml.getNextNode();
		while(node != "/OrganAppendageNode"){
			switch(node){
			case "AnimationNode":
				appendage.animations.Add(readAnimationNode(xml));
				break;
			case "ActionNode":
				appendage.combatActions.Add(readActionNode(xml));
				break;
			}
			node = xml.getNextNode();
		}
		appendage.prototypeIndex = appendagePrototypes.Count;
		appendagePrototypes.Add(appendage);
	}

	private Vector3 readOffsetNode(XmlProcessor xml){
		float x = 0f;
		float y = 0f; 
		float z = 0f;

		XmlAttribute attribute = xml.getNextAttribute();

		while(attribute.name != ""){
			switch(attribute.name){
				case "x":
					x = float.Parse(attribute.value)/128;
					break;
				case "y":
					y = float.Parse(attribute.value)/128;
					break;
				case "z":
					z = float.Parse(attribute.value)/128;
					break;
			}
			attribute = xml.getNextAttribute();
		}

		return new Vector3(x,y,z);
	}

	private OrganAnimation readAnimationNode(XmlProcessor xml){
		string node;
		OrganAnimation anim = new OrganAnimation();
		XmlAttribute attribute = xml.getNextAttribute();

		while(attribute.name != ""){
			switch(attribute.name){
				case "name":
					anim.name = attribute.value;
					break;
			}
			attribute = xml.getNextAttribute();
		}

		node = xml.getNextNode();
		while(node != "/AnimationNode"){
			switch(node){
			case "TagNode":
				anim.tags.Add(readTagNode(xml));
				break;
			}
			node = xml.getNextNode();
		}
		return anim;
	}

	private string readTagNode(XmlProcessor xml){
		return xml.getNextAttribute().value;
	}

	private CombatAction readActionNode(XmlProcessor xml){
		CombatAction act = new CombatAction();
		XmlAttribute attribute = xml.getNextAttribute();

		while(attribute.name != ""){
			switch(attribute.name){
				case "name":
					act.name = attribute.value;
					break;
				case "range":
					act.range = float.Parse(attribute.value);
					break;
				case "damage":
					act.damage = int.Parse(attribute.value);
					break;
				case "windupDuration":
					act.windupDuration = float.Parse(attribute.value);
					break;
				case "attackDuration":
					act.attackDuration = float.Parse(attribute.value);
					break;
				case "cooldownDuration":
					act.cooldownDuration = float.Parse(attribute.value);
					break;
				case "windupAnimation":
					act.windupAnimation = attribute.value;
					break;
				case "attackAnimation":
					act.attackAnimation = attribute.value;
					break;
				case "backswingAnimation":
					act.backswingAnimation = attribute.value;
					break;
			}
			attribute = xml.getNextAttribute();
		}
		return act;
	}

}