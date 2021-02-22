﻿using System.Collections;
using Pathfinding;
using ProjectStavitski.Combat;
using ProjectStavitski.Core;
using ProjectStavitski.Items;
using ProjectStavitski.Units;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace ProjectStavitski.Player 
{
    public class PlayerMovementController : MonoBehaviour
    {
        [SerializeField] private float speed = 10;
        [SerializeField] private LayerMask whatIsBlocked;
        [SerializeField] private LayerMask whatIsCollision;
        [SerializeField] private ItemConfig currentItem;
        [SerializeField] private Image inventoryImage;
        
        private bool _isFacingRight = true;
        private Unit _unit;
        private SingleNodeBlocker _blocker;

        private void Awake()
        {
            _unit = GetComponent<Unit>();
            _blocker = GetComponent<SingleNodeBlocker>();
        }

        private void Start()
        {
            GameManager.Instance.player = gameObject;
            GameManager.Instance.AddObstacleToList(_blocker);
        }

        private void Update()
        {
            if (GameManager.Instance.playerTurn)
            {
                GetInput();
            }
        }

        private void GetInput()
        {
            if (Input.GetKey(KeyCode.W))
            {
                Vector3 destination = new Vector3(transform.position.x, transform.position.y + 1);
                
                if (CheckForCollisions(transform.up)) return;
                
                StartCoroutine(SmoothMovement(destination,transform.up));
            }
            else if (Input.GetKey(KeyCode.S))
            {
                Vector3 destination = new Vector3(transform.position.x, transform.position.y - 1);
                
                if (CheckForCollisions(-transform.up)) return;
                
                StartCoroutine(SmoothMovement(destination,-transform.up));
            }
            else if (Input.GetKey(KeyCode.A))
            {
                if (_isFacingRight)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                    _isFacingRight = false;
                }
                Vector3 destination = new Vector3(transform.position.x - 1, transform.position.y);
                
                if (CheckForCollisions(-transform.right)) return;
                
                StartCoroutine(SmoothMovement(destination,-transform.right));
            }
            else if (Input.GetKey(KeyCode.D))
            {
                if (!_isFacingRight)
                {
                    transform.localScale = new Vector3(1, 1, 1);
                    _isFacingRight = true;
                }
                Vector3 destination = new Vector3(transform.position.x + 1, transform.position.y);
                
                if (CheckForCollisions(transform.right)) return;
                
                StartCoroutine(SmoothMovement(destination,transform.right));
            }
        }

        private bool CheckForCollisions(Vector3 tr)
        {
            RaycastHit2D hitEnemy = Physics2D.Raycast(transform.position, tr,1.1f, whatIsCollision);

            if (hitEnemy.transform == null) return false;

            if (hitEnemy.transform.CompareTag("Obstacle")) return true;

            if (currentItem != null)
            {
                if (hitEnemy.transform.CompareTag("Tree") && currentItem.canCutTrees)
                { Destroy(hitEnemy.transform.gameObject); 
                    return false;
                }
                else if (hitEnemy.transform.CompareTag("Tree") && !currentItem.canCutTrees)
                { 
                    return true;
                }
                else if (hitEnemy.transform.CompareTag("Wall") && currentItem.canBreakWalls)
                { 
                    Destroy(hitEnemy.transform.gameObject); return false;
                }
                else if (hitEnemy.transform.CompareTag("Wall") && !currentItem.canBreakWalls)
                { 
                    return true;
                }
            }
            else
            {
                return true;
            }

            IDamageable enemy = hitEnemy.transform.gameObject.GetComponent<IDamageable>();
            Attack(enemy);
            return true;
        }

        private void Attack(IDamageable enemy)
        {
            enemy.TakeDamage(_unit.GetDamage());
            GameManager.Instance.playerTurn = false;
        }

        public void EquipItem(ItemConfig item, Vector3 pos)
        {
            if(currentItem != null)
                currentItem.DropThisItem(pos);
            
            currentItem = item;
            currentItem.EquipNewItem(inventoryImage);
        }

        private IEnumerator SmoothMovement(Vector3 destination, Vector3 tr)
        {
            RaycastHit2D hitBlock = Physics2D.Raycast(transform.position, tr,1.1f, whatIsBlocked);
            Debug.DrawRay(transform.position, tr, Color.red,3);
            if (hitBlock.transform == null)
            {
                while (transform.position != destination)
                {
                    transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
                    GameManager.Instance.playerTurn = false;
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }
}
