using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private GameObject _gameObjectToFollow;
    [SerializeField]
    private GameObject _background;

    private float _cameraXHalfSize;
    private float _cameraYHalfSize;
    private float _leftSceneEdge; 
    private float _rightSceneEdge;
    private float _topSceneEdge;
    private float _bottomSceneEdge;
     
    void Start()
    {
        SetScreenOrientation();
        SetCameraSizes();
        SetBackgroundSizes();
    }

    private void SetScreenOrientation()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    private void SetCameraSizes()
    {
        Vector3 cameraViewPoint = _camera.ViewportToWorldPoint(transform.position) * -1;
        _cameraXHalfSize = cameraViewPoint.x;
        _cameraYHalfSize = cameraViewPoint.y;
    }

    private void SetBackgroundSizes()
    {
        Vector3 backgroundSize = _background.GetComponent<Renderer>().bounds.size;

        _rightSceneEdge = backgroundSize.x / 2;
        _leftSceneEdge = _rightSceneEdge * (-1);
        _topSceneEdge = backgroundSize.y / 2;
        _bottomSceneEdge = _topSceneEdge * (-1);
    }

    private void FixedUpdate() 
    {
        float newCameraXPosition = transform.position.x;
        float newCameraYPosition = transform.position.y;

        if (IsValidHorizontalPosition())
        {
            newCameraXPosition = _gameObjectToFollow.transform.position.x;
        }
            
        if (IsValidVerticalPosition())
        {
            newCameraYPosition = _gameObjectToFollow.transform.position.y;
        }

        _camera.transform.position = new Vector3(newCameraXPosition, newCameraYPosition, transform.position.z);
    }

    private bool IsValidHorizontalPosition()
    {
        if (_gameObjectToFollow != null)
        {
            return _gameObjectToFollow.transform.position.x > _leftSceneEdge + _cameraXHalfSize && 
                _gameObjectToFollow.transform.position.x < _rightSceneEdge - _cameraXHalfSize;
        } 
        
        return false;
    }

    private bool IsValidVerticalPosition()
    {
        if (_gameObjectToFollow != null)
        {
            return _gameObjectToFollow.transform.position.y > _bottomSceneEdge + _cameraYHalfSize && 
                _gameObjectToFollow.transform.position.y < _topSceneEdge - _cameraYHalfSize;
        }

        return false;
    }

    public void SetGameObjectToFollow(GameObject gameObjectToFollow)
    {
        _gameObjectToFollow = gameObjectToFollow;
    }
}