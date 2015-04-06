using UnityEngine;
using System.Collections;

public class SimpleTimeSource : ITimeSource {

	// Use this for initialization

    private double _time = 0;
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        _time += Time.deltaTime;
	}

    public override double GetTime()
    {
        return _time;
    }

    public void Reset() 
    {
        _time = 0;
    }
}
