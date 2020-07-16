using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    //Variables publicas 
    public int boardRows, boardColumns;
    public int minRoomSize, maxRoomSize;

    public GameObject floorTile;

    private GameObject[,] boardPositionsFloor;
    public class SubDungeon
    {
        //Publicas
        public SubDungeon left, right;
        public Rect rect;
        public Rect room = new Rect(-1, -1, 0, 0);
        public List<Rect> corredores = new List<Rect>();

        public void CreateRoom()
        {
            if(left != null)
            {
                left.CreateRoom();
            }
            if(right != null)
            {
                right.CreateRoom();
            }

            if(IAmLeaf())
            {
                int roomWidth = (int)Random.Range(rect.width / 2, rect.width - 2);
                int roomHeight = (int)Random.Range(rect.height / 2, rect.height - 2);
                int roomX = (int)Random.Range(1, rect.width - roomWidth - 1);
                int roomY = (int)Random.Range(1, rect.height - roomHeight - 1);

                //La room tendra posicion absoluta en el board, no relativa al sub-dungeon
                room = new Rect(rect.x + roomX, rect.y + roomY, roomWidth, roomHeight);
                Debug.Log("Created room " + room + " en sub-dungeon " + debugId + " " + rect);
            }
        }

        public int debugId;

        private static int debugCounter = 0;
        private Transform transform;

        public SubDungeon(Rect mrect)
        {
            rect = mrect;
            debugId = debugCounter;
            debugCounter++;
        }

        public bool IAmLeaf()
        {
            return left == null && right == null;
        }

        public bool Split(int minRoomSize, int maxRoomSize)
        {
            if (!IAmLeaf())
            {
                return false;
            }

            bool splitH;
            if (rect.width / rect.height >= 1.25)
            {
                splitH = false;
            }
            else if(rect.height / rect.width >= 1.25)
            {
                splitH = true;
            }
            else
            {
                splitH = Random.Range(0.0f, 1.0f) > 0.5;
            }

            if(Mathf.Min(rect.height, rect.width) / 2 < minRoomSize)
            {
                Debug.Log("Sub-dungeon " + debugId + "sera una hoja");
                return false;
            }

            if(splitH)
            {
                int split = Random.Range(minRoomSize, (int)(rect.width - minRoomSize));
                left = new SubDungeon(new Rect(rect.x, rect.y, rect.width, split));
                right = new SubDungeon(new Rect(rect.x, rect.y + split, rect.width, rect.height - split));
            }
            else
            {
                int split = Random.Range(minRoomSize, (int)(rect.height - minRoomSize));
                left = new SubDungeon(new Rect(rect.x, rect.y, rect.height, split));
                right = new SubDungeon(new Rect(rect.x + split, rect.y, rect.width - split, rect.height));
            }
            return true;
        }

        public Rect GetRoom()
        {
            if (IAmLeaf())
            {
                return room;
            }
            if (left != null)
            {
                Rect leftRoom = left.GetRoom();

                if (leftRoom.x != -1)
                {
                    return leftRoom;
                }
            }
            if (right != null)
            {
                Rect rightRoom = right.GetRoom();
                if (rightRoom.x != -1)
                {
                    return rightRoom;
                }
            }
            return new Rect(-1, -1, 0, 0);
        }

        public void CreateCorridors(SubDungeon left, SubDungeon right)
        {
            Rect leftRoom = left.GetRoom();
            Rect rightRoom = right.GetRoom();

            Vector2 leftPoint = new Vector2((int)Random.Range(leftRoom.x + 1, leftRoom.xMax - 1), (int)Random.Range(leftRoom.y + 1, leftRoom.yMax - 1));
            Vector2 rightPoint = new Vector2((int)Random.Range(rightRoom.x + 1, rightRoom.xMax - 1), (int)Random.Range(rightRoom.y + 1, rightRoom.yMax - 1));

            //Asegurar que el punto izquierdo siempre este a la izquierda
            if(leftPoint.x > rightPoint.x)
            {
                Vector2 temp = leftPoint;
                leftPoint = rightPoint;
                rightPoint = temp;
            }

            int w = (int)(leftPoint.x - rightPoint.x);
            int h = (int)(leftPoint.y - rightPoint.y);

            Debug.Log("leftPoint: " + leftPoint + ", rightPoint: " + rightPoint + ", w: " + w + ", h: " + h);

            //Si los puntos no estan alineados horizontalmente
            if(w != 0)
            {
                //Elige random si va horizontal, luego vertical y opuesto
                if(Random.Range (0,1) > 2)
                {
                    //Añade corredor a la derecha
                    corredores.Add(new Rect(leftPoint.x, leftPoint.y, Mathf.Abs(w) + 1, 1));

                    //Si el punto izquierdo esta debajo del derecho, lo pone arriba, de lo contrario, lo pone abajo
                    if(h < 0)
                    {
                        corredores.Add(new Rect(rightPoint.x, leftPoint.y, 1, Mathf.Abs(h)));
                    }
                    else
                    {
                        corredores.Add(new Rect(rightPoint.x, leftPoint.y, 1, -Mathf.Abs(h)));
                    }
                }
                else
                {
                    //Va arriba o abajo
                    if(h < 0)
                    {
                        corredores.Add(new Rect(leftPoint.x, leftPoint.y, 1, Mathf.Abs(h)));
                    }
                    else
                    {
                        corredores.Add(new Rect(leftPoint.x, leftPoint.y, 1, -Mathf.Abs(h)));
                    }

                    //Luego va derecha
                    corredores.Add(new Rect(leftPoint.x, rightPoint.y, Mathf.Abs(w) + 1, 1));
                }
            }
            else
            {
                //Si los puntos estan horizontales, sube o baja dependiendo de posiciones
                if(h < 0)
                {
                    corredores.Add(new Rect((int)leftPoint.x, (int)leftPoint.y, 1, Mathf.Abs(h)));
                }
                else
                {
                    corredores.Add(new Rect((int)rightPoint.x, (int)rightPoint.y, 1, Mathf.Abs(h)));
                }
            }
            Debug.Log("Corredores: ");
            foreach(Rect corredor in corredores)
            {
                Debug.Log("Corredor: " + corredor);
            }
        }
    }


    public void DrawRooms(SubDungeon subDungeon)
    {
        if (subDungeon == null)
        {
            return;
        }

        if (subDungeon.IAmLeaf())
        {
            for (int i = (int)subDungeon.room.x; i < subDungeon.room.xMax; i++)
            {
                for (int j = (int)subDungeon.room.y; j < subDungeon.room.yMax; j++)
                {
                    GameObject instance = Instantiate(floorTile, new Vector3(i, j, 0f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(transform);
                    boardPositionsFloor[i, j] = instance;
                }
            }
        }
        else
        {
            DrawRooms(subDungeon.left);
            DrawRooms(subDungeon.right);
        }
    }
    public void CreateBSP(SubDungeon subDungeon)
    {
        Debug.Log("Division sub-dungeon " + subDungeon.debugId + ": " + subDungeon.rect);
        if(subDungeon.IAmLeaf())
        {
            //Si el sub-dungeon es muy largo
            if(subDungeon.rect.width > maxRoomSize || subDungeon.rect.height > maxRoomSize || Random.Range(0.0f, 1.0f) > 0.25)
            {
                if(subDungeon.Split(minRoomSize, maxRoomSize))
                {
                    Debug.Log("Division sub-dungeon" + subDungeon.debugId + "en" + subDungeon.left.debugId + ": " + subDungeon.left.rect + ", " + subDungeon.right.debugId + ": " + subDungeon.right.right);
                    CreateBSP(subDungeon.left);
                    CreateBSP(subDungeon.right);
                }
            }
        }
    }

    void Start()
    {
        SubDungeon rootSubDungeon = new SubDungeon(new Rect(0, 0, boardRows, boardColumns));
        CreateBSP(rootSubDungeon);
        rootSubDungeon.CreateRoom();
        boardPositionsFloor = new GameObject[boardRows, boardColumns];
        DrawRooms(rootSubDungeon);
    }
}
