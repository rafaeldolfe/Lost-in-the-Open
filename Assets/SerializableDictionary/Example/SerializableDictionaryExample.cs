using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializableDictionaryExample : MonoBehaviour {
	// The dictionaries can be accessed throught a property
	[SerializeField]
	public ObjectColorDictionary m_objectColorDictionary;
	public StringColorArrayDictionary m_objectColorArrayDictionary;

	void Reset ()
	{
		// access by property
		m_objectColorDictionary = new ObjectColorDictionary() { {gameObject, Color.blue}, {this, Color.red} };
	}
}
