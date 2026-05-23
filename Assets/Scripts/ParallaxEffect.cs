using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BackgroundLayer
{
    public Sprite sprite;
    public float parallaxFactor;
    public int spriteCount;
    public Vector2 startPosition;
    public Vector2 sizeScale;
    public int zPosition;
    public float repositionThresholdMultiplier = 1;
    public Material material;
}

public class ParallaxEffect : MonoBehaviour
{
    public Transform cameraTransform;
    public float intensity;

    [SerializeField] private List<BackgroundLayer> backgroundLayers;
    [SerializeField] private CameraFollow cameraFollow;

    private List<BackgroundLayerController> layerControllers = new List<BackgroundLayerController>();

    private float CamerasLastXPosition;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        InitializeLayers();
    }

    private void InitializeLayers()
    {
        for (int i = 0; i < backgroundLayers.Count; i++)
        {
            var layer = backgroundLayers[i];
            var layerController =
                new BackgroundLayerController(layer, i, backgroundLayers[i].repositionThresholdMultiplier,
                    cameraFollow, backgroundLayers[i].material);
            layerControllers.Add(layerController);
        }
    }

    //private void LateUpdate()
    //{
    //    if (!Player.Instance.gameObject.activeInHierarchy) return;
    //    if (Player.Instance.MoveDirection == 0) return;
    //    if (Player.Instance.BossActive) return;
    //    if (Player.Instance.OnStarting) return;

    //    float CameraXDelta = cameraTransform.position.x - CamerasLastXPosition;

    //    foreach (var controller in layerControllers)
    //    {
    //        controller.UpdateLayer(CameraXDelta * intensity, Time.deltaTime);
    //    }

    //    CamerasLastXPosition = cameraTransform.position.x;
    //}
}

public class BackgroundLayerController
{
    private BackgroundLayer layerData;
    private GameObject layerObject;
    private List<GameObject> backgroundSprites = new List<GameObject>();
    private int sortingOrder;
    private float repositionThresholdMultiplier;
    private CameraFollow cameraFollow;
    private Material mat;

    public BackgroundLayerController(BackgroundLayer data, int order, float thresholdMultiplier,
        CameraFollow cameraFollow, Material mat)
    {
        layerData = data;
        sortingOrder = -order - 1;
        repositionThresholdMultiplier = thresholdMultiplier;
        this.cameraFollow = cameraFollow;
        this.mat = mat;
        InitializeLayer();
    }

    private void InitializeLayer()
    {
        layerObject = new GameObject($"BackgroundLayer_{sortingOrder}");
        layerObject.transform.position = new Vector3(layerData.startPosition.x, layerData.startPosition.y, 0);

        for (int i = 0; i < layerData.spriteCount; i++)
        {
            CreateBackgroundSprite(i);
        }
    }

    private void CreateBackgroundSprite(int index)
    {
        GameObject background = new GameObject($"Background_{index}");
        float xPosition = index * layerData.sprite.bounds.size.x * layerData.sizeScale.x;
        background.transform.position = new Vector3(Camera.main.transform.position.x, layerObject.transform.position.y, layerData.zPosition);
        background.transform.localScale = new Vector3(layerData.sizeScale.x, layerData.sizeScale.y, 1);
        background.transform.SetParent(layerObject.transform);

        SpriteRenderer spriteRenderer = background.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = layerData.sprite;
        spriteRenderer.sortingOrder = sortingOrder - 1;
        spriteRenderer.sortingLayerName = "Background";
        spriteRenderer.material = mat;

        mat.SetTexture("_TextureSprite", layerData.sprite.texture);

        CameraTransformMatch match = background.AddComponent<CameraTransformMatch>();
        match.offset = new Vector3(0f, 0f, 10);

        backgroundSprites.Add(background);
    }

    public void UpdateLayer(float moveDirection, float deltaTime)
    {
        if (!cameraFollow.IsMoving) return;
        MoveLayer(moveDirection, deltaTime);
        RepositionSprites(moveDirection);
    }

    private void MoveLayer(float moveDirection, float deltaTime)
    {
        layerObject.transform.Translate(new Vector2(-moveDirection, 0) * (layerData.parallaxFactor * deltaTime));

        // Restore player delta
       // Vector2 playerMovementDelta = Player.Instance.MovementDelta;
       // layerObject.transform.Translate(new Vector2(playerMovementDelta.x * -moveDirection * deltaTime, 0));
    }

    private void RepositionSprites(float moveDirection)
    {
        float cameraWidth = Camera.main.orthographicSize * Camera.main.aspect * 2;
        float leftEdge = Camera.main.transform.position.x - (cameraWidth * repositionThresholdMultiplier / 2);
        float rightEdge = Camera.main.transform.position.x + (cameraWidth * repositionThresholdMultiplier / 2);

        if (moveDirection > 0) // Moving right
        {
            RepositionSpritesForRightMovement(leftEdge);
        }
        else if (moveDirection < 0) // Moving left
        {
            RepositionSpritesForLeftMovement(rightEdge);
        }
    }

    private void RepositionSpritesForRightMovement(float leftEdge)
    {
        GameObject lastSprite = backgroundSprites[backgroundSprites.Count - 1];

        for (int i = 0; i < backgroundSprites.Count; i++)
        {
            GameObject sprite = backgroundSprites[i];
            float spriteRightEdge = sprite.transform.position.x +
                                    (layerData.sprite.bounds.size.x / 2 * layerData.sizeScale.x);

            if (spriteRightEdge < leftEdge)
            {
                float newX = lastSprite.transform.position.x + (layerData.sprite.bounds.size.x * layerData.sizeScale.x);
                sprite.transform.position = new Vector3(newX, sprite.transform.position.y, sprite.transform.position.z);

                backgroundSprites.RemoveAt(i);
                backgroundSprites.Add(sprite);
                lastSprite = sprite;
                i--;
            }
        }
    }

    private void RepositionSpritesForLeftMovement(float rightEdge)
    {
        GameObject firstSprite = backgroundSprites[0];

        for (int i = backgroundSprites.Count - 1; i >= 0; i--)
        {
            GameObject sprite = backgroundSprites[i];
            float spriteLeftEdge = sprite.transform.position.x -
                                   (layerData.sprite.bounds.size.x / 2 * layerData.sizeScale.x);

            if (spriteLeftEdge > rightEdge)
            {
                float newX = firstSprite.transform.position.x -
                             (layerData.sprite.bounds.size.x * layerData.sizeScale.x);
                sprite.transform.position = new Vector3(newX, sprite.transform.position.y, sprite.transform.position.z);

                backgroundSprites.RemoveAt(i);
                backgroundSprites.Insert(0, sprite);
                firstSprite = sprite;
            }
        }
    }
}