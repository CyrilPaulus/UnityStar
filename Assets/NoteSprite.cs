using UnityEngine;
using System.Collections;

public class NoteSprite : MonoBehaviour {
    
    private float _width = 1;

    public float Width
    {
        get
        {
            return _width;
        }

        set
        {
            _width = value;
            //0.68 because the main bar is
            var val = (value - 0.32f);
            if (val < 0)
                val = 0;

            UpdateWidth(val/0.16f);
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void UpdateWidth(float width) {
        var center = transform.GetChild(1);
        center.transform.localScale = new Vector3(width, 1, 1);
        center.transform.localPosition =new Vector3(0.16f, 0, 0);

        var end = transform.GetChild(2);
        end.transform.localPosition = new Vector3(0.16f + 0.16f * width, 0, 0);
    }
}
