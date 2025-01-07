using Assets.Scripts.Infrastructure.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BarsContaining : MonoBehaviour
{
    private Dictionary<int, int> _barIdPriorityDict;
    private List<GameObject> _barsList;
    private Transform _barsContainer;

    // Start is called before the first frame update
    void Awake()
    {
        _barIdPriorityDict = new Dictionary<int, int>();
        _barsList = new List<GameObject>();

        foreach (Transform transform in gameObject.transform)
        {
            if (transform.CompareTag(Tag.BarCanvas.ToString()))
            {
                _barsContainer = transform;
            }
        }
    }

    void Update()
    {
        if (!_barsContainer.gameObject.activeSelf)
        {
            return;
        }

        if (_barsList.Count == 0 || _barsList.All(b => !b.activeSelf))
        {
            return;
        }

        _barsContainer.LookAt(transform.position + Camera.main.transform.forward);
        ReOrderBars();
    }

    public void ReOrderBars()
    {
        var shift = 0f;
        foreach (var bar in _barsList.Where(b => b.activeSelf))
        {
            var rectTransform = (RectTransform)bar.transform;
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, -shift, rectTransform.localPosition.z);
            shift += rectTransform.rect.height;
        }
    }

    public GameObject AddBarToContainer(GameObject barTempalte, int priority)
    {
        var bar = Instantiate(barTempalte, _barsContainer.transform);
        bar.transform.parent = _barsContainer.transform;
        _barsList.Add(bar);
        _barIdPriorityDict.Add(bar.GetInstanceID(), priority);

        _barsList = _barsList
            .OrderByDescending(b => _barIdPriorityDict[b.GetInstanceID()])
            .ToList();

        return bar;
    }
}
