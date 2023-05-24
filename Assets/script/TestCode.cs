using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCode : MonoBehaviour
{
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;


    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    //Logic
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector2Int tmpHover;
    private Character[,] characters;
    private Vector3 bounds;




    private Vector2Int mouseHover; // 마우스 위치 임시 기록 용도

    private int[,] moves = new int[4, 2] { { 0, 1 }, { -1, 0 }, { 0, -1 }, { 1, 0 } };


    private bool isMouseInUse = true;


    private void Awake()
    {
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        tmpHover.Set(0, 0);
        SpawnAllCharacters();
        PositionAllCharacters();
    }
    private void Update()
    {
        if (isMouseInUse) mouseMod();
        else keyboardMod();

        if (!currentHover.Equals(tmpHover))
        {
            if (currentHover != -Vector2Int.one) tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Hover"); // 현재 좌표 노란 타일로 변경
            tiles[tmpHover.x, tmpHover.y].layer = LayerMask.NameToLayer("Tile"); //현재 좌표 흰색 타일로 변경
            tmpHover.Set(currentHover.x, currentHover.y);
        }
    }

    private void mouseMod()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            isMouseInUse = false;
            Debug.Log("키보드 모드가 실행됩니다.");
            return;
        }
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover")))
        {
            currentHover = LookupTileIndex(info.transform.gameObject);
        }
    }

    private void keyboardMod()
    {
        // isMouseInUse = false;
        int idx = -1;
        if (Input.GetKeyDown(KeyCode.W)) idx = 0;
        else if (Input.GetKeyDown(KeyCode.A)) idx = 1;
        else if (Input.GetKeyDown(KeyCode.S)) idx = 2;
        else if (Input.GetKeyDown(KeyCode.D)) idx = 3;
        else return;
        int x = currentHover.x + moves[idx, 0];
        int y = currentHover.y + moves[idx, 1];

        if (x < 0 || y < 0 || x >= TILE_COUNT_X || y >= TILE_COUNT_Y) return;
        currentHover.Set(x, y);
    }

    private void DisableMouseInput()
    {
        // Lock the cursor or confine it within the game window
        Cursor.lockState = CursorLockMode.Locked;
        // Hide the cursor
        Cursor.visible = false;
    }

    private void EnableMouseInput()
    {
        // Restore normal cursor behavior
        Cursor.lockState = CursorLockMode.None;
        // Show the cursor
        Cursor.visible = true;
    }


    // Generate board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
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
        vertices[0] = new Vector3(x * tileSize, 0, y * tileSize);
        vertices[1] = new Vector3(x * tileSize, 0, (y + 1) * tileSize);
        vertices[2] = new Vector3((x + 1) * tileSize, 0, y * tileSize);
        vertices[3] = new Vector3((x + 1) * tileSize, 0, (y + 1) * tileSize);


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
        characters[0, 0] = SpawnSingleCharacter(CharacterType.Lion, none);
        //redTeam
        characters[1, 0] = SpawnSingleCharacter(CharacterType.Rose, none);
    }

    private Character SpawnSingleCharacter(CharacterType type, int team)
    {
        Character character = Instantiate(prefabs[(int)type - 1], transform).GetComponent<Character>();

        character.type = type;
        character.team = team;
        //character.GetComponent<SpriteRenderer>().material = teamMaterials[team];

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
        characters[x, y].transform.position = GetTileCenter(x, y);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0.5f, tileSize / 2 + 0.2f);
    }



    //Operations
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
