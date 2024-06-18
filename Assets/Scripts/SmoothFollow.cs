using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour {

    // The target we are following
    public Transform target;
    // The distance in the x-z plane to the target
    public float distance = 10.0f;
    // the height we want the camera to be above the target
    public float height = 5.0f;
    // How much we 
    public float heightDamping = 2.0f;
    public float rotationDamping = 3.0f;

    //当前帧数值,用来检测是否有重复更新 
    private int curFrame = -1;

    //是否优化动态建筑显示
    public bool useOptimizeDynamicBuild = true;

    //是否优化静态特效显示
    public bool useOptimizeStaticEffect = true;

    //是否优化动态模型显示
    public bool useOptimizeDynamicModel = true;

    //相机视截体
    public Plane[] planes;
    private Camera mainCamera = null;

    public bool CameraMoving;
    public Transform mTransform = null;
    public float slideSpeed = 10f; // 滑动速度  
    private Vector3 startPosition; // 滑动起始位置（世界空间或屏幕空间）  
    private Vector3 lastPosition; // 上一个位置（用于计算滑动向量）  
    private bool isSliding = false; // 是否正在滑动  

    // Use this for initialization
    void Start ()
    {
        mainCamera = Camera.main;

        //Debug.Log("start camera name" + mainCamera.name);
	}

    //[yaz]不需要每帧更新
//     void FixedUpdate()
//     {
//         //FixedUpdatePosition();
//     }

    void FixedUpdate()
    {
        if (Input.touchCount > 0) // 处理触摸输入  
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (IsTouchInSlideArea(touch.position))
                {
                    isSliding = true;
                    startPosition = Input.mousePosition;
                    lastPosition = startPosition;
                }
            }
            else if (touch.phase == TouchPhase.Moved && isSliding)
            {
                CameraMoving = true;
                Vector3 currentPosition = Input.mousePosition;
                Vector3 slideDelta = (currentPosition - lastPosition) * slideSpeed * Time.deltaTime;
                SlideCamera(slideDelta);
                lastPosition = currentPosition;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isSliding = false;
                CameraMoving = false;

            }
        }
        else if (Input.GetMouseButtonDown(0)) // 处理鼠标输入  
        {
            if (IsMouseInSlideArea(Input.mousePosition))
            {
                isSliding = true;
                startPosition = Input.mousePosition;
                lastPosition = startPosition;
            }
        }
        else if (Input.GetMouseButton(0) && isSliding) // 鼠标按下并移动  
        {
            CameraMoving = true;
            Vector3 currentPosition = Input.mousePosition;
            Vector3 slideDelta = (currentPosition - lastPosition) * slideSpeed * Time.deltaTime;
            SlideCamera(slideDelta);
            lastPosition = currentPosition;
        }
        else if (Input.GetMouseButtonUp(0)) // 鼠标释放  
        {
            isSliding = false;
            CameraMoving = false;
        }
    }
    // 根据滑动向量滑动相机  
    private void SlideCamera(Vector3 slideDelta)
    {
        // 注意：你可能需要根据你的场景和相机设置来调整这个逻辑  
        // 例如，你可能需要限制相机在x和z轴上移动，而不是y轴  
        Vector3 newPosition = new Vector3(transform.position.x + slideDelta.x,transform.position.y, transform.position.z + slideDelta.y);
        transform.position = newPosition;
    }
    // 将屏幕位置转换为世界空间位置（对于3D滑动区域）  
    private Vector3 GetWorldPositionFromScreen(Vector3 screenPosition)
    {
        return Camera.main.ScreenToWorldPoint(screenPosition); // 替换为实际转换逻辑  
    }

    // 检查鼠标位置是否在滑动区域内（对于UI或3D滑动区域）  
    private bool IsMouseInSlideArea(Vector3 mousePosition)
    {
        // 这里你需要根据你的滑动区域类型来实现检查逻辑  
        // 如果是UI元素，使用RectTransform  
        // 如果是3D对象，使用射线投射（Raycasting）  
        // 这里只是一个示例，需要替换为实际检查逻辑  
        return true; // 替换为实际检查逻辑  
    }
    //获取Transform
    public Transform GetTransform()
    {
        if (mTransform == null)
        {
            mTransform = gameObject.transform;
        }
        return mTransform;
    }


    //创建视平截体
    public void RefreshFrustumPlanes()
    {
        if (!useOptimizeDynamicBuild)
            return;

        if (mainCamera == null)
            return;

        planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);        
    }
  
    //检测包围盒是否可见
    public bool IsInFrustum(Plane[] planes,Bounds bound)
    {
        return GeometryUtility.TestPlanesAABB(planes, bound);
    }
	
	// Update is called once per frame
    public void FixedUpdatePosition()
    {
        // Early out if we don't have a target
        if (target == null || CameraMoving)
        {
            return;
        }

        //相机位置不随玩家位置高度，高度60固定
        Vector3 targetPos = new Vector3(target.position.x, 60, target.position.z);


        // Calculate the current rotation angles
        float wantedRotationAngle = target.eulerAngles.y;
        float wantedHeight = targetPos.y + height;

        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        // Damp the rotation around the y-axis
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

        // Damp the height
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        // Convert the angle into a rotation

        Quaternion wantRotation = Quaternion.Euler(0, currentRotationAngle, 0);
        //Debug.Log("wantRotation  " + wantRotation);
        //Quaternion nowRotation = this.transform.rotation;
        //Quaternion midQua = wantRotation;
        ////Debug.Log("Quaternion.Angle(wantRotation, nowRotation)  " + Quaternion.Angle(wantRotation, nowRotation));
        //if (Quaternion.Angle(wantRotation, nowRotation) >= 10.0f)
        //{
        //    //this.transform.rotation = wantRotation;
        //}
        //else {
        //    midQua = Quaternion.Lerp(nowRotation, wantRotation, 2.0f * Time.deltaTime);
        //}
        //Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        Vector3 wantPos = targetPos - wantRotation * Vector3.forward * distance;
        wantPos.y = currentHeight;
        Vector3 nowPos = GetTransform().position;
        float mtoDis = Vector3.Distance(wantPos, nowPos);
        GetTransform().position = wantPos;
        Vector3 dir = targetPos - GetTransform().position;
        dir.Normalize();
        GetTransform().rotation = Quaternion.LookRotation(dir);
        //transform.LookAt(target);      

        RefreshFrustumPlanes();

        //检测是否有重复更新
        if (curFrame == Time.frameCount)
        {
            Debug.LogError("update more than once in one frame!");
        }
        curFrame = Time.frameCount;
    }

    private bool IsTouchInSlideArea(Vector2 touchPosition)
    {
        // 如果slideArea是UI元素，使用RectTransform的Rect属性来检查  
        // 如果slideArea是3D对象，你需要将其屏幕坐标转换为世界坐标，然后进行检查  
        // 这里假设slideArea是UI元素  
        //Rect screenRect = new Rect(slideArea.position.x, slideArea.position.y, slideArea.sizeDelta.x, slideArea.sizeDelta.y);
        //return screenRect.Contains(touchPosition);
        return true;
    }
}
