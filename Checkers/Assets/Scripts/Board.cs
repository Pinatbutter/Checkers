using System.Collections;
using UnityEngine;

public class Board : MonoBehaviour
{

    public Pieces[,] piece = new Pieces[8, 8];
    public Vector3 offSet = new Vector3(-4.0f, 0, -4.0f);
    public Vector3 otherOffSet = new Vector3(0.5f, 0, 0.5f);

    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    private Pieces selectedPiece;
    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector3 endDrag;

    private void Start()
    {
        GeneratePieces();
    }

    private void Update()
    {
        UpdateMouseOver();
        Debug.Log(mouseOver);
        {
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;
            if (Input.GetMouseButtonDown(0))
                SelectPiece(x, y);
            if (Input.GetMouseButtonUp(0))
            {
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);
            }
        }
    }
    private void UpdateMouseOver()
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mouseOver.x = (int)(hit.point.x - offSet.x);
            mouseOver.y = (int)(hit.point.z - offSet.z);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }
    private void SelectPiece(int x, int y)
    {
        if (x < 0 || x >= piece.Length || y < 0 || y >= piece.Length)
            return;

        Pieces p = piece[x, y];
        if (p != null)
        {
            selectedPiece = p;
            startDrag = mouseOver;
            Debug.Log(selectedPiece.name);
        }
    }
    private void TryMove(int x1, int y1, int x2, int y2)
    {
        startDrag = new Vector2(x1, y1); 
        endDrag = new Vector2(x2, y2);
        selectedPiece = piece[x1, y1]; 

        PlacePiece(selectedPiece, x2, y2);
    }
    private void GeneratePieces()
    {
        //white pieces
        for (int y = 0; y < 3; y++)
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                if (oddRow)
                    CreatePiece(x, y);
                else
                    CreatePiece(x + 1, y);
            }
        }

        //black pieces
        for (int y = 7; y > 4; y--)
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                if (oddRow)
                    CreatePiece(x, y);
                else
                    CreatePiece(x + 1, y);
            }
        }


    }
    private void CreatePiece(int x, int y)
    {
        GameObject go;
        if (y > 3)
            go = Instantiate(blackPiecePrefab) as GameObject;
        else
            go = Instantiate(whitePiecePrefab) as GameObject;
        go.transform.SetParent(transform);
        Pieces p = go.GetComponent<Pieces>();
        piece[x, y] = p;
        PlacePiece(p, x, y);
    }

    private void PlacePiece(Pieces p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + offSet + otherOffSet;

    }


}
