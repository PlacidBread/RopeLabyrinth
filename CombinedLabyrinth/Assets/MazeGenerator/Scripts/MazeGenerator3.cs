using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using MazeGenerator.Scripts.Enums;
using Mono.Cecil.Cil;
using StarterAssets;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

namespace MazeGenerator.Scripts
{
    public class MazeGenerator3 : MonoBehaviour
    {
        [SerializeField] private MazeNode mazeNodePrefab;
        [SerializeField] private MazeNodeExit mazeNodeExitPrefab;
        [SerializeField] private GameObject endColliderPrefab;
        [SerializeField] private GameObject player;
        // [SerializeField] private Camera cameraController;
        private GameRespawn gameRespawn;
        private Rope rope;

        [SerializeField] private int mazeWidth;
        [SerializeField] private int mazeHeight;
        [SerializeField] private int mazeNodeScale = 4;
        [SerializeField] private int torchSpawnGap = 3;
        [SerializeField] private float startingMaxRopeLength = 50.0f;
        private bool occupied;

        private int _count = 0;
        [SerializeField] private InputActionReference debugInput;
        private MazeNode[,] _mazeNodes;

        private GameObject _endCollider;
        private MazeNodeExit _mazeNodeExit;
        private MazeNode _mazeNodeButton;
        private Transform spawn;

        private void DebugFunction(InputAction.CallbackContext obj)
        {
            if (!rope.RenderRope) return;
            rope.LogRopePos();
        }

        // Start is called before the first frame update
        void Start()
        {
            // _input = GetComponent<StarterAssetsInputs>();
            gameRespawn = player.GetComponent<GameRespawn>();
            if (gameRespawn is null)
            {
                Debug.Log("Error");
                return;
            }


        
            // SmoothCameraController.Activate();
            // ThirdPersonController.CanMove = true;
            SetupMaze();
        }

        private void SetupMaze()
        {
            _mazeNodes = new MazeNode[mazeWidth, mazeHeight];

            for (int i = 0; i < mazeWidth; i++)
            {
                for (int j = 0; j < mazeHeight; j++)
                {
                    _mazeNodes[i, j] = Instantiate(mazeNodePrefab, new Vector3(i*mazeNodeScale, 0, j*mazeNodeScale), Quaternion.identity);
                    _mazeNodes[i, j].SetIndex(i, j);
                }
            }

            var spawnPos = gameRespawn.SetSpawn(_mazeNodes, mazeWidth, mazeHeight);
            spawn = spawnPos;

            GenerateMaze(null, _mazeNodes[0, 0]);
            LoopClearDuplicateWalls();
            GenerateExit();

            
            
            rope = player.GetComponent<Rope>();
            rope.StartRenderRope(spawnPos);
            rope.SetMaxRopeLength(startingMaxRopeLength);
            
            SmoothCameraController.Activate();
            ThirdPersonController.CanMove = true;
        
            debugInput.action.performed += DebugFunction;
            debugInput.action.Enable();
        }
        

        private void GenerateExit()
        {
            ExitSide exitSide = (ExitSide)Random.Range(0, 4);
            int random;
            Vector2Int index = new Vector2Int();
            // GameObject endCol;
            // Vector3 euler;
            int buttonIndex = 0;
            
            switch ((ExitSide)exitSide)
            {
                case ExitSide.Top:
                    random = Random.Range(1, mazeWidth-1);
                    index = new Vector2Int(random, mazeHeight-1);
                    _endCollider = Instantiate(endColliderPrefab, new Vector3(index.x * mazeNodeScale, 0, (index.y + 1) * mazeNodeScale), quaternion.identity);
                    buttonIndex = mazeWidth - 1 - random;
                    if (buttonIndex == random)
                    {
                        buttonIndex++;
                    }
                    _mazeNodeButton = _mazeNodes[buttonIndex, index.y];
                    // _mazeNodes[random, mazeHeight-1].ClearFrontWall();
                    // _mazeNodes[random, mazeHeight-1].ClearAll();
                    break;
                case ExitSide.Right:
                    random = Random.Range(1, mazeHeight-1);
                    index = new Vector2Int(mazeWidth-1, random);
                    _endCollider = Instantiate(endColliderPrefab, new Vector3((index.x + 1) * mazeNodeScale, 0, index.y * mazeNodeScale), quaternion.identity);
                    // euler = endCol.transform.eulerAngles;
                    // endCol.transform.rotation = quaternion.Euler(euler.x, euler.y + 90, euler.z);
                    _endCollider.transform.eulerAngles = new Vector3(0, 90, 0);
                    buttonIndex = mazeHeight - 1 - random;
                    if (buttonIndex == random)
                    {
                        buttonIndex++;
                    }
                    _mazeNodeButton = _mazeNodes[index.x, buttonIndex];
                    // _mazeNodes[mazeWidth-1, random].ClearRightWall();ds
                    // _mazeNodes[mazeWidth-1, random].ClearAll();
                    break;
                case ExitSide.Bottom:
                    random = Random.Range(1, mazeWidth-1);
                    index = new Vector2Int(random, 0);
                    _endCollider = Instantiate(endColliderPrefab, new Vector3(index.x * mazeNodeScale, 0, (index.y - 1) * mazeNodeScale), quaternion.identity);
                    
                    buttonIndex = mazeWidth - 1 - random;
                    if (buttonIndex == random)
                    {
                        buttonIndex++;
                    }
                    _mazeNodeButton = _mazeNodes[buttonIndex, index.y];
                    // _mazeNodes[random, 0].ClearBackWall();
                    // _mazeNodes[random, 0].ClearAll();
                    break;
                case ExitSide.Left:
                    random = Random.Range(1, mazeHeight-1);
                    index = new Vector2Int(0, random);
                    _endCollider = Instantiate(endColliderPrefab, new Vector3((index.x - 1) * mazeNodeScale, 0, index.y * mazeNodeScale), quaternion.identity);
                    // euler = endCol.transform.eulerAngles;
                    // endCol.transform.rotation = quaternion.Euler(euler.x, euler.y+90, euler.z);
                    _endCollider.transform.eulerAngles = new Vector3(0, 90, 0);
                    buttonIndex = mazeHeight - 1 - random;
                    if (buttonIndex == random)
                    {
                        buttonIndex++;
                    }
                    _mazeNodeButton = _mazeNodes[index.x, buttonIndex];
                    
                    // _mazeNodes[0, random].ClearLeftWall();
                    // _mazeNodes[0, random].ClearAll();
                    break;
                default:
                    _mazeNodeButton = _mazeNodes[0, 0];
                    Debug.Log("WARNING - default switch result on mazeGenerator");
                    break;
            }
        
            _mazeNodes[index.x, index.y].ClearAll();
            _mazeNodeExit = Instantiate(mazeNodeExitPrefab, new Vector3(index.x * mazeNodeScale, 0, index.y * mazeNodeScale), quaternion.identity);
            
            _mazeNodeButton.SetButton();
            _mazeNodeExit.ActivateDoor(exitSide);
            
            // TODO: Generate collider (& Door?) at exit position (var random range...)
        }

        private void GenerateMaze(MazeNode prevNode, MazeNode currNode)
        {
            occupied = false;
            currNode.Visit();
            ClearWalls(prevNode, currNode);

            // yield return new WaitForSeconds(0.05f);

            MazeNode nextNode;
            if (occupied == false && currNode.transform != spawn) occupied = currNode.SetCoin();
            if (occupied == false && currNode.transform != spawn) occupied = currNode.SetSpike();
            do
            {
                nextNode = GetNextUnvisitedNode(currNode);

                if (nextNode != null)
                {
                    _count++;
                    if (_count >= torchSpawnGap)
                    {
                        _count = 0;
                        prevNode.SetTorch();
                    }
                    
                    GenerateMaze(currNode, nextNode);
                }
            } while (nextNode != null);
        }

        private MazeNode GetNextUnvisitedNode(MazeNode currNode)
        {
            var unvisitedNodes = GetUnvisitedNodes(currNode);

            return unvisitedNodes.OrderBy(_ => Random.Range(1, 10)).FirstOrDefault();
        }

        // check surrounding cells
        private IEnumerable<MazeNode> GetUnvisitedNodes(MazeNode currNode)
        {
            // int x = (int)currNode.transform.position.x;
            // int z = (int)currNode.transform.position.z;

            int x = (int)currNode.Index.x;
            int z = (int)currNode.Index.y;
        
            if (x + 1 < mazeWidth)
            {
                var cellToRight = _mazeNodes[x + 1, z];

                if (!cellToRight.IsVisited)
                {
                    yield return cellToRight;
                }
            }

            if (x - 1 >= 0)
            {
                var cellToLeft = _mazeNodes[x - 1, z];

                if (!cellToLeft.IsVisited)
                {
                    yield return cellToLeft;
                }
            }
        
            if (z + 1 < mazeHeight)
            {
                var cellToFront = _mazeNodes[x, z + 1];

                if (!cellToFront.IsVisited)
                {
                    yield return cellToFront;
                }
            }
        
            if (z - 1 >= 0)
            {
                var cellToBack = _mazeNodes[x, z - 1];

                if (!cellToBack.IsVisited)
                {
                    yield return cellToBack;
                }
            } 
        }
    
        private void LoopClearDuplicateWalls()
        {
            for (int i = 1; i < mazeWidth - 1; i++)
            {
                for (int j = 1; j < mazeHeight - 1; j++)
                {
                    if (_mazeNodes[i + 1, j].GetActiveLW())
                    {
                        _mazeNodes[i, j].ClearRightWall();
                    }
                    if (_mazeNodes[i - 1, j].GetActiveRW())
                    {
                        _mazeNodes[i, j].ClearLeftWall();
                    }
                    if (_mazeNodes[i, j + 1].GetActiveFW())
                    {
                        _mazeNodes[i, j].ClearBackWall();
                    }
                    if (_mazeNodes[i, j - 1].GetActiveBW())
                    {
                        _mazeNodes[i, j].ClearFrontWall();
                    }
                }
            }
        }
        private void ClearWalls(MazeNode prevNode, MazeNode currNode)
        {
            // check start node
            if (prevNode == null) return;
        
            Vector3 direction = (currNode.transform.position - prevNode.transform.position).normalized;
            Vector3Int prevPosition = Vector3Int.RoundToInt(prevNode.transform.position);
            Vector3Int currPosition = Vector3Int.RoundToInt(currNode.transform.position);
        
            // Calculate the position difference between the nodes
            Vector3Int diff = currPosition - prevPosition;
        
            if (prevNode.transform.position.x < currNode.transform.position.x)
            {
                prevNode.ClearRightWall();
                currNode.ClearLeftWall();
                // ClearDuplicateWalls(true, currNode);
                return;
            }
            if (prevNode.transform.position.x > currNode.transform.position.x)
            {
                prevNode.ClearLeftWall();
                currNode.ClearRightWall();
                // ClearDuplicateWalls(true, currNode);
                return;
            }
            if (prevNode.transform.position.z < currNode.transform.position.z)
            {
                prevNode.ClearFrontWall();
                currNode.ClearBackWall();
                // ClearDuplicateWalls(false, currNode);
                return;
            }
            if (prevNode.transform.position.z > currNode.transform.position.z)
            {
                prevNode.ClearBackWall();
                currNode.ClearFrontWall();
                // ClearDuplicateWalls(false, currNode);
                return;
            }
        }

        public void ButtonPressed()
        {
            // add delay before door opens?
            _mazeNodeExit.SetDoOpenDoor(true);
            _mazeNodeButton.ButtonPressed();
            // Debug.Log("Succ");
        }
        
        /// Difficulty scaling with level:
        ///     - mazeWidth * 2 and mazeHeight * 2
            public void EndReached()
            {
                Debug.Log("Next Level Reached!");

                // Load the next scene
                LoadNextLevel();
            }

            private void LoadNextLevel()
            {
                // Get the current scene build index
                int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

                // Load the next scene by incrementing the current scene index
                SceneManager.LoadScene(currentSceneIndex + 1);
            }


        private void ClearMaze()
        {
            // for (int i = mazeWidth - 1; i >= 0; i--)
            // {
            //     for (int j = mazeHeight - 1; j >= 0; j--)
            //     {
            //         Destroy(_mazeNodes[i, j]);
            //    }
            // }
            foreach (var mazeNode in _mazeNodes)
            {
                Destroy(mazeNode.gameObject);
            }
            Destroy(_mazeNodeExit.gameObject);
            Destroy(_endCollider.gameObject);
            // Destroy(_mazeNodeButton);
        }
    }
}
