using DG.Tweening;
using UnityEngine;

// DoTween анимации для башни кубов.
public static class TowerAnimations
{
    public static Tween AnimateDrop(CubeView cube, Vector2 targetPos, float jumpHeight, float duration)
    {
        bool isUnlocked = false;
        cube.DraggableCube.LockInteraction();
        cube.Rect.anchoredPosition = new Vector2(targetPos.x, targetPos.y + jumpHeight);
        return cube.Rect.DOAnchorPos(targetPos, duration).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            cube.DraggableCube.UnlockInteraction();
            isUnlocked = true;
        })
        .OnKill(() =>
        {
            if (!isUnlocked)
                cube.DraggableCube.UnlockInteraction();
        });
    }

    public static void FadeAndDestroy(CubeView cube)
    {
        cube.DraggableCube.LockInteraction();
        cube.Rect.DOScale(0f, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Object.Destroy(cube.gameObject);
        });
    }

    public static void AnimateFallIntoHole(CubeView cube, float fallDuration)
    {
        bool isUnlocked = false;
        Sequence seq = DOTween.Sequence();

        cube.DraggableCube.LockInteraction();
        var rect = cube.Rect;

        seq.Join(rect.DOAnchorPos(new Vector2(rect.anchoredPosition.x, -500f), fallDuration).SetEase(Ease.InQuad));

        seq.Join(rect.DORotate(new Vector3(0, 0, Random.Range(-360f, 360f)), fallDuration, RotateMode.FastBeyond360));

        seq.OnComplete(() =>
        {
            cube.DraggableCube.UnlockInteraction();
            isUnlocked = true;
            Object.Destroy(rect.gameObject);
        })
        .OnKill(() =>
        {
            if (!isUnlocked)
                cube.DraggableCube.UnlockInteraction();
        });
    }
}
