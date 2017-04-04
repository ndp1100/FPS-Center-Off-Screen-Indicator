using UnityEngine;
using UnityEngine.UI;

public class CenterCycleIndicator : MonoBehaviour
{
    private Vector3 heightOffset = new Vector3(0, 1, 0);
    public Image icon;
    public Image iconOfScreen;

    public RectTransform CanvasRect;
    public Transform mainPlayer;
    public Transform target;

    private Camera mGameCamera;

    const float radius = 160f;

    // Use this for initialization
    private void Start()
    {
        mGameCamera = Camera.main;
    }

    public void SetData(Transform m_target, float m_heightOffset = 1)
    {
        target = m_target;
        heightOffset.y = m_heightOffset;
    }

    private Vector2 PointOnCircle(float _radius, float angleInDegrees, Vector2 origin)
    {
        // Convert from degrees to radians via multiplication by PI/180        
        //        float x = (float)(radius * Mathf.Cos(angleInDegrees * Mathf.PI / 180F)) + origin.x;
        //        float y = (float)(radius * Mathf.Sin(angleInDegrees * Mathf.PI / 180F)) + origin.y;

        // [21/10/2016 01:38:41 PhuongND] -  Trong truong hop nay, vi truc toa do la x huong len tren, y huong sang phai, nen 
        //cach tinh x,y nguoc so voi truong hop truc toa do thong thuong
        var x = _radius * Mathf.Sin(angleInDegrees * Mathf.PI / 180F) + origin.x;
        var y = _radius * Mathf.Cos(angleInDegrees * Mathf.PI / 180F) + origin.y;
        //End PhuongND

        return new Vector2(x, y);
    }


    private float AngleBetween2Vector(Vector3 vec1, Vector3 vec2)
    {
        var angle = Vector3.Angle(vec1, vec2); // calculate angle
        return angle * Mathf.Sign(Vector3.Cross(vec1, vec2).y);
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (!target.gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            return;
        }

        UpdateIndicator();
    }

    Vector2 GetScreenPosition(Vector3 worldPos, RectTransform canvasRec)
    {
        // 0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. 
        // Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.
        Vector2 WorldObject_ScreenPosition = new Vector2(
                ((worldPos.x * canvasRec.sizeDelta.x) - (canvasRec.sizeDelta.x * 0.5f)),
                ((worldPos.y * canvasRec.sizeDelta.y) - (canvasRec.sizeDelta.y * 0.5f)));
        return WorldObject_ScreenPosition;
    }

    private void UpdateIndicator()
    {
        var pos = mGameCamera.WorldToViewportPoint(target.position + heightOffset);

        // Determine the visibility and the target alpha
        var isVisible = pos.z > 0f && pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f;

        if (isVisible)
        {
            icon.gameObject.SetActive(true);
            iconOfScreen.gameObject.SetActive(false);
            
            Vector2 WorldObject_ScreenPosition = GetScreenPosition(pos, CanvasRect);
            icon.rectTransform.anchoredPosition = WorldObject_ScreenPosition;
            icon.rectTransform.eulerAngles = Vector3.zero;
        }
        else //Off-screen Indicator
        {
            icon.gameObject.SetActive(false);
            iconOfScreen.gameObject.SetActive(true);
            UpdateOfscreen();
        }
    }

    private void UpdateOfscreen()
    {
        if (target == null) return;
        //calculate icon position
        var targetPos = new Vector3(target.position.x, 1, target.position.z);
        var playerPos = new Vector3(mainPlayer.position.x, 1, mainPlayer.position.z);
        var direcPlayer2Target = targetPos - playerPos;
        var angleInWorldSpace = AngleBetween2Vector(mainPlayer.forward, direcPlayer2Target);

        if (angleInWorldSpace < 0) angleInWorldSpace = angleInWorldSpace + 360;
        iconOfScreen.rectTransform.anchoredPosition = PointOnCircle(radius, angleInWorldSpace, Vector2.zero);

        //calculate icon rotation
        iconOfScreen.rectTransform.localEulerAngles = new Vector3(0, 0, -angleInWorldSpace);
    }
}