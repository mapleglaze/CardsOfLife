using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using MapleGlaze.CardsOfLife.Utils;

namespace MapleGlaze.CardsOfLife
{
    public class MainController : MonoBehaviour
    {
        private CardController cardController;
        private int bet;
        private int playerLife;
        private int demonLife;

        public enum GameState {
            Talking,
            Betting,
            Playing
        }

        public GameState State;

        //private GameStateManager gameStateManager;

        // Start is called before the first frame update
        void Start()
        {
            GameObject cardPrefab = Resources.Load<GameObject>("Prefabs/Card");
            cardController = new CardController(GameObject.Find("CardContainer"), cardPrefab);

            playerLife = 3;
            demonLife = 100;

            // Testing - TODO: REMOVE
            bet = 2;
            cardController.StartRound();
            State = GameState.Playing;

            this.UpdateText();
        }

        // Update is called once per frame
        void Update()
        {
            if (this.State == GameState.Playing)
            {
                if (this.cardController != null && this.cardController.State == CardController.RoundState.Over)
                {
                    if (this.cardController.Winner == CardController.Participant.Player)
                    {
                        playerLife += this.bet;
                        demonLife -= this.bet;
                    }
                    else if (this.cardController.Winner == CardController.Participant.AI)
                    {
                        demonLife += this.bet;
                        playerLife -= this.bet;
                    }
                    else
                    {
                        // tie
                    }

                    //this.State = GameState.Talking;

                    this.cardController.StartRound();
                }
                else if (this.cardController != null)
                {
                    this.cardController.HandleUpdate();
                }
                
                this.UpdateText();
            }

            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();

                int layerMask = 1;

                if (Physics.Raycast(ray, out hit, 100, layerMask))
                {
                    GameObject clickedObject = hit.transform.gameObject;
                    if (clickedObject.name == "Exit")
                    {
                        #if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
                        #else
                            Application.Quit();
                        #endif
                    }
                }
            }
        }

        private void UpdateText()
        {
            GameObject.Find("LifeBetText").GetComponent<TextMeshPro>().text = "Years: " + this.playerLife
                        + "\n\n\nBet: " + this.bet;

            int playerScore = this.cardController.Score.ContainsKey(CardController.Participant.Player)
                        ? this.cardController.Score[CardController.Participant.Player] : 0;

            int demonScore = this.cardController.Score.ContainsKey(CardController.Participant.AI)
                ? this.cardController.Score[CardController.Participant.AI] : 0;

            GameObject.Find("ScoreText").GetComponent<TextMeshPro>().text = 
                        demonScore + "\n\n\n\n\n\n\n" + playerScore;
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            int numCardsX = 4;
            int numCardsY = 3;
            float cardWidth = 0.070f;
            float cardHeight = 0.121f;
            float spaceBetweenCards = 0.01f;

            float cardsWidth = (numCardsX * cardWidth) + ((numCardsX - 1) * spaceBetweenCards);
            float cardsHeight = (numCardsY * cardHeight) + ((numCardsY - 1) * spaceBetweenCards);
            float startX = GameObject.Find("CardContainer").transform.position.x - (cardsWidth / 2f);
            float startZ = GameObject.Find("CardContainer").transform.position.z - (cardsHeight / 2f);

            GizmoUtil.DrawRectGizmoY(new Rect(startX, startZ, cardsWidth, cardsHeight), GameObject.Find("CardContainer").transform.position.y);
        }
    }
}
