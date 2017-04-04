using UnityEngine;
using UnityEngine.UI;

public class EdgeIndicator : MonoBehaviour
{
    public RectTransform CanvasRect;
    public Image icon;
    public Image offscreen_icon;
    public Camera mGameCamera;
    public Transform mainPlayer;
    public Transform target;

    private float localSize = 30;
    private Vector3 heightOffset = new Vector3(0, 1, 0);
    private float width;
    private float height;
    // Use this for initialization
    private void Start()
    {
        localSize = icon.minWidth;
        mGameCamera = Camera.main;

        width = CanvasRect.rect.width;
        height = CanvasRect.rect.height;
    }

    public void SetData(Transform m_target)
    {
        target = m_target;
        mGameCamera = Camera.main;
        
    }

    private void Update()
    {
        var pos = mGameCamera.WorldToViewportPoint(target.position + heightOffset);
        // Determine the visibility and the target alpha
        var isVisible = pos.z > 0f && pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f;

        if (isVisible)
        {
            icon.gameObject.SetActive(true);
            offscreen_icon.gameObject.SetActive(false);

            var WorldObject_ScreenPosition = GetScreenPosition(pos, CanvasRect);
            icon.rectTransform.anchoredPosition = WorldObject_ScreenPosition;
            icon.rectTransform.eulerAngles = Vector3.zero;
        }
        else //Ofscreen Indicator
        {
            //            UpdateOfScreenIndicator();
            icon.gameObject.SetActive(false);
            offscreen_icon.gameObject.SetActive(true);

            UpdateOfScreenNew();
        }
    }

    private Vector2 intersect(int edgeLine, Vector2 line2point1, Vector2 line2point2)
    {
        float[] A1 = { -height, 0, height, 0 };
        float[] B1 = { 0, -width, 0, width };
        float[] C1 = { -width * height, -width * height, 0, 0 };

        var A2 = line2point2.y - line2point1.y;
        var B2 = line2point1.x - line2point2.x;
        var C2 = A2 * line2point1.x + B2 * line2point1.y;

        var det = A1[edgeLine] * B2 - A2 * B1[edgeLine];

        return new Vector2((B2 * C1[edgeLine] - B1[edgeLine] * C2) / det, (A1[edgeLine] * C2 - A2 * C1[edgeLine]) / det);
    }



    private Vector3 PointOnCircle(float _radius, float angleInDegrees, Vector3 origin)
    {
        // Convert from degrees to radians via multiplication by PI/180        
        //        float x = (float)(radius * Mathf.Cos(angleInDegrees * Mathf.PI / 180F)) + origin.x;
        //        float y = (float)(radius * Mathf.Sin(angleInDegrees * Mathf.PI / 180F)) + origin.y;

        // [21/10/2016 01:38:41 PhuongND] -  Trong truong hop nay, vi truc toa do la x huong len tren, y huong sang phai, nen 
        //cach tinh x,y nguoc so voi truong hop truc toa do thong thuong
        var x = _radius * Mathf.Sin(angleInDegrees * Mathf.PI / 180F) + origin.x;
        var y = _radius * Mathf.Cos(angleInDegrees * Mathf.PI / 180F) + origin.y;
        //End PhuongND

        return new Vector3(x, y, 0);
    }

    private float AngleBetween2Vector(Vector3 vec1, Vector3 vec2)
    {
        var angle = Vector3.Angle(vec1, vec2); // calculate angle
        return angle * Mathf.Sign(Vector3.Cross(vec1, vec2).y);
    }

    Vector2 GetScreenPosition(Vector3 worldPos, RectTransform canvasRec)
    {
        // 0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. 
        // Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.
        Vector2 WorldObject_ScreenPosition = new Vector2(
                ((worldPos.x * width) - (width * 0.5f)),
                ((worldPos.y * height) - (height * 0.5f)));
        return WorldObject_ScreenPosition;
    }


    const float radius = 160f;
    private void UpdateOfScreenNew()
    {
        if (target == null) return;
        //calculate icon position
        var targetPos = new Vector3(target.position.x, 1, target.position.z);
        var playerPos = new Vector3(mainPlayer.position.x, 1, mainPlayer.position.z);
        var direcPlayer2Target = targetPos - playerPos;
        var angleInWorldSpace = AngleBetween2Vector(mainPlayer.forward, direcPlayer2Target);
        if (angleInWorldSpace < 0) angleInWorldSpace = angleInWorldSpace + 360;
        var targetPosOnScreen = PointOnCircle(radius, angleInWorldSpace, Vector3.zero);
        Vector2 _finalPos = targetPosOnScreen.normalized * width * height;
        //        if (targetPosOnScreen.z < 0) targetPosOnScreen *= -1;
        var angle = Mathf.Atan2(targetPosOnScreen.y, targetPosOnScreen.x) * Mathf.Rad2Deg;

//        var targetPos = new Vector3(target.position.x, 1, target.position.z);
//        var playerPos = new Vector3(mainPlayer.position.x, 1, mainPlayer.position.z);
//        var direcPlayer2Target = targetPos - playerPos;
//        var angleInWorldSpace = AngleBetween2Vector(mainPlayer.forward, direcPlayer2Target);
//        if (angleInWorldSpace < 0) angleInWorldSpace = angleInWorldSpace + 360;
//
//        var pos = mGameCamera.WorldToViewportPoint(target.position + heightOffset);
//        var WorldObject_ScreenPosition = GetScreenPosition(pos, CanvasRect);
//        var angle = Mathf.Atan2(WorldObject_ScreenPosition.y, WorldObject_ScreenPosition.x) * Mathf.Rad2Deg;

        float coef;
        
        if (width > height)
            coef = width / height;
        else
            coef = height / width;

        var degreeRange = 360f / (coef + 1);

        if (angle < 0) angle = angle + 360;
        int edgeLine;
        if (angle <= degreeRange / 4f) edgeLine = 0;
        else if (angle <= 180 - degreeRange / 4f) edgeLine = 1;
        else if (angle <= 180 + degreeRange / 4f) edgeLine = 2;
        else if (angle <= 360 - degreeRange / 4f) edgeLine = 3;
        else edgeLine = 0;

        var size = localSize / 2f;

        var posInScreenSpace = intersect(edgeLine, Vector2.zero, _finalPos);
//        posInScreenSpace.x = Mathf.Clamp(posInScreenSpace.x, size, width - size);
//        posInScreenSpace.y = Mathf.Clamp(posInScreenSpace.y, size, height - size);
        //        transform.position = mUICamera.ScreenToWorldPoint(posInScreenSpace);

        offscreen_icon.rectTransform.anchoredPosition = posInScreenSpace;
        offscreen_icon.rectTransform.localEulerAngles = new Vector3(0, 0, -angleInWorldSpace);
    }

    private void UpdateOfScreenIndicator()
    {
        if (mGameCamera == null) return;
        if (target == null) return;

        var targetPosOnScreen = mGameCamera.WorldToScreenPoint(target.position);

        // [14/04/2016 05:42:02 PhuongND] -  
        if (targetPosOnScreen.z < 0)
            targetPosOnScreen *= -1;
        //End PhuongND

        var half_width = 640 * Screen.width / Screen.height * 0.5f;
        var half_height = 640 * 0.5f;

        /*var center = new Vector3(Screen.width/2f, Screen.height/2f, 0);*/
        var center = new Vector3(half_width, half_height, 0);
        var angle = Mathf.Atan2(targetPosOnScreen.y - center.y, targetPosOnScreen.x - center.x) * Mathf.Rad2Deg;

        float coef;
        if (Screen.width > Screen.height)
            coef = Screen.width / (float)Screen.height;
        else
            coef = Screen.height / (float)Screen.width;

        var degreeRange = 360f / (coef + 1);

        if (angle < 0) angle = angle + 360;
        int edgeLine;
        if (angle <= degreeRange / 4f) edgeLine = 0;
        else if (angle <= 180 - degreeRange / 4f) edgeLine = 1;
        else if (angle <= 180 + degreeRange / 4f) edgeLine = 2;
        else if (angle <= 360 - degreeRange / 4f) edgeLine = 3;
        else edgeLine = 0;

        /*            1
         *   -----------------
         *   |                |
         * 2 |                | 0
         *   |                |
         *   -----------------
         *            3
        */

        // [14/04/2016 05:57:05 PhuongND] -  
        var size = localSize / 2f;
        var padding = new Vector3(0, 0, 0);

        //End PhuongND

        //        transform.position = mUICamera.ScreenToWorldPoint(intersect(edgeLine, center, targetPosOnScreen) + padding);
        var pos = transform.localPosition;

        pos.x = Mathf.Clamp(pos.x, -half_width + size, half_width - size);
        pos.y = Mathf.Clamp(pos.y, -half_height + size, half_height - size);
        transform.localPosition = pos;

        icon.gameObject.transform.eulerAngles = new Vector3(0, 0, angle - 90);
        //End PhuongND
    }
}