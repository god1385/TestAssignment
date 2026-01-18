using UnityEngine;
using UnityEngine.UI;
using Zenject;


public class CubeView : MonoBehaviour
{
    [SerializeField] private Image mainImage;
    [SerializeField] private CubeToDrag cubeToDrag;
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private RectTransform rectTransform;

    public LayoutElement LayoutElement => layoutElement;
    public RectTransform Rect => rectTransform;
    public Sprite CubeSprite => mainImage.sprite;
    public CubeToDrag CubeToDrag => cubeToDrag;

    [Inject]
    public void Construct(GameEventsHandler eventsHandler)
    {
        cubeToDrag.Construct(eventsHandler);
    }

    public void Initialize(Sprite sprite, RectTransform dragparent, ScrollRect mainScroll = null)
    {
        mainImage.sprite = sprite;
        cubeToDrag.Initialize(sprite, this.transform, dragparent, mainScroll);
    }
}
