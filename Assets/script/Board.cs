using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private bool draggingFllowOption = false;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    //Logic
    private Character[,] characters;
    private Character currentDragging;
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector2Int tmpHover;
    private Vector3 bounds;


    private bool isMouseInUse = true;
    private bool isKeyboardInUse = false;



    private void Awake()
    {
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllCharacters();
        PositionAllCharacters();
        tmpHover.Set(0, 0);
    }
    private void Update()
    {
        Debug.Log("currentHover: " + currentHover);
        Debug.Log("tmpHover: " + tmpHover);

        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);


        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover")))
        {
            //get the indexes of the tile i hit (clicked)
            if (isMouseInUse)
            {
                currentHover = LookupTileIndex(info.transform.gameObject);
            }

            if (currentHover == -Vector2Int.one)
            {
                tiles[tmpHover.x, tmpHover.y].layer = LayerMask.NameToLayer("Tile");
                //currentHover = -Vector2Int.one;
            }

            else
            {
                if (!currentHover.Equals(tmpHover))
                {

                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Hover");

                    tiles[tmpHover.x, tmpHover.y].layer = LayerMask.NameToLayer("Tile");
                    tmpHover.Set(currentHover.x, currentHover.y);

                }
            }


            //mouse clicked
            if (Input.GetMouseButtonDown(1))
            {
                //check mouse hover data have character
                if (characters[currentHover.x, currentHover.y] != null)
                {
                    //Is it our turn?
                    if (true)
                    {
                        //set current dragging character's x y pos
                        currentDragging = characters[currentHover.x, currentHover.y];
                    }
                }
            }

            //mouse releasing
            if (currentDragging != null && Input.GetMouseButtonUp(1))
            {
                Vector2Int previousPositon = new Vector2Int(currentDragging.currentX, currentDragging.currentY);

                bool validMove = MoveTo(currentDragging, currentHover.x, currentHover.y);

                if (!validMove)
                {
                    //���� ���������� �ٽ� �ٲ��ְ� ����
                    currentDragging.SetPosition(GetTileCenter(previousPositon.x, previousPositon.y));
                    currentDragging = null;
                }
                else
                {
                    currentDragging = null;
                }



            }
        }


        else
        {
            tiles[tmpHover.x, tmpHover.y].layer = LayerMask.NameToLayer("Tile");

        }

        //when camera moving 
        if (Input.GetMouseButton(2))
        {
            for (int x = 0; x < TILE_COUNT_X; x++)
                for (int y = 0; y < TILE_COUNT_Y; y++)
                    tiles[x, y].layer = LayerMask.NameToLayer("Tile");

        }

        //while draging character fllows
        if (currentDragging && draggingFllowOption == true)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * (yOffset + 0.5f));
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
                currentDragging.SetPosition(ray.GetPoint(distance));

        }


    }

    // Generate board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("x: {0} , y: {1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;


        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    //Spawning of the characters
    private void SpawnAllCharacters()
    {
        characters = new Character[TILE_COUNT_X, TILE_COUNT_Y];
        int blueTeam = 0, redTeam = 1, none = 3;

        //blueTeam
        characters[0, 0] = SpawnSingleCharacter(CharacterType.Lion, blueTeam);
        characters[2, 0] = SpawnSingleCharacter(CharacterType.Rose, blueTeam);


        //redTeam
        characters[1, 0] = SpawnSingleCharacter(CharacterType.Rose, redTeam);
    }

    private Character SpawnSingleCharacter(CharacterType type, int team)
    {
        Character character = Instantiate(prefabs[(int)type - 1], transform).GetComponent<Character>();

        character.type = type;
        character.team = team;
        character.GetComponent<SpriteRenderer>().material = teamMaterials[team];

        return character;
    }


    //position characters
    private void PositionAllCharacters()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (characters[x, y] != null)
                    PositionSingleCharacter(x, y, true);
    }

    private void PositionSingleCharacter(int x, int y, bool force = false)
    {
        characters[x, y].currentX = x;
        characters[x, y].currentY = y;
        characters[x, y].SetPosition(GetTileCenter(x, y), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0.5f, tileSize / 2 + 0.2f);
    }


    //Operations
    private bool MoveTo(Character character, int x, int y)
    {
        Vector2Int previousPosition = new Vector2Int(character.currentX, character.currentY);

        //Is there another character on the target position?
        if (characters[x, y] != null)
        {
            Character otherCharacter = characters[x, y];

            if (character.team == otherCharacter.team)
            {
                Debug.Log("Same_team");
                return false;
            }

            if (character.team != otherCharacter.team)
            {
                otherCharacter.SetPosition(new Vector3(8, 0, 0), true);
                Debug.Log("other_team");
            }
        }

        characters[x, y] = character;
        characters[previousPosition.x, previousPosition.y] = null;

        PositionSingleCharacter(x, y);
        return true;
    }


    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);
            }
        }

        return -Vector2Int.one; // Invalid
    }

}
