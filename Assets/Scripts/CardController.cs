using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MapleGlaze.CardsOfLife.Components;
using MapleGlaze.CardsOfLife.Constants;
using MapleGlaze.CardsOfLife.Utils;

namespace MapleGlaze.CardsOfLife
{
    public class CardController
    {
        private string selectCardAnimationState = "CardFlip";
        private string deselectCardAnimationState = "CardFlipReverse";
        private string dropCardAnimationState = "CardDrop";

        private GameObject container;
        private GameObject cardPrefab;
        private float cardWidth;
        private float cardHeight;
        private float waitElapsed;
        private float waitDuration;

        public float CardWidth {
            get { return this.cardWidth; }
        }

        public float CardHeight {
            get { return this.cardHeight; }
        }


        private Dictionary<CardType, Material> materials = new Dictionary<CardType, Material>();

        public enum RoundState {
            Selection,
            Evaluation,
            EvaluationWait,
            Over
        }

        public RoundState State = RoundState.Over;

        public enum Participant {
            Player,
            AI
        }

        public Participant ParticipantTurn = Participant.Player;

        public Dictionary<Participant, int> Score = new Dictionary<Participant, int>();
        public Participant? Winner = null;

        public float NumCardsX = 4;
        public float NumCardsY = 3;

        public float SpaceBetweenCards = 0.010f; // metres

        public float EvaluationWaitTime = 0.7f;
        public float AIWaitTime = 0.6f;

        public List<GameObject> Cards = new List<GameObject>();
        private List<GameObject> selectedCards = new List<GameObject>();
        private List<GameObject> animatingCards = new List<GameObject>();
        private List<GameObject> faceUpCards = new List<GameObject>();

        private List<GameObject> memory = new List<GameObject>();
        private int MaxMemory = 5;

        // TODO: Add Listener
        // Events:
        // - OnRoundOver(bool win, int bet)
        // 

        public CardController(GameObject container, GameObject cardPrefab)
        {
            this.container = container;
            this.cardPrefab = cardPrefab;

            materials.Add(CardType.RED, Resources.Load<Material>("Materials/Cards/RedCard"));
            materials.Add(CardType.ORANGE, Resources.Load<Material>("Materials/Cards/OrangeCard"));
            materials.Add(CardType.YELLOW, Resources.Load<Material>("Materials/Cards/YellowCard"));
            materials.Add(CardType.GREEN, Resources.Load<Material>("Materials/Cards/GreenCard"));
            materials.Add(CardType.BLUE, Resources.Load<Material>("Materials/Cards/BlueCard"));
            materials.Add(CardType.BLACK, Resources.Load<Material>("Materials/Cards/BlackCard"));

            if (this.cardPrefab != null && this.cardPrefab.transform.GetChild(0).GetComponent<Renderer>())
            {
                this.cardWidth = this.cardPrefab.transform.GetChild(0).GetComponent<Renderer>().bounds.size.x;
                this.cardHeight = this.cardPrefab.transform.GetChild(0).GetComponent<Renderer>().bounds.size.z;
            }

            this.Score[Participant.Player] = 0;
            this.Score[Participant.AI] = 0;
        }

        public void StartRound()
        {
            if (this.State == RoundState.Over)
            {
                this.GenerateCards();

                this.Score[Participant.Player] = 0;
                this.Score[Participant.AI] = 0;
                this.Winner = null;

                this.State = RoundState.Selection;
                this.ParticipantTurn = Participant.Player;

                this.selectedCards.Clear();
                this.animatingCards.Clear();
                this.faceUpCards.Clear();
                this.memory.Clear();
            }
        }

        public void EndRound()
        {
            if (this.State != RoundState.Over)
            {
                foreach (Participant participant in this.Score.Keys)
                {
                    if (this.Winner == null 
                        || this.Score[participant] > this.Score[(Participant)this.Winner])
                    {
                        this.Winner = participant;
                    }
                    else if (this.Winner != null 
                            && this.Score[participant] == this.Score[(Participant)this.Winner])
                    {
                        this.Winner = null;
                    }
                }

                this.State = RoundState.Over;

                this.selectedCards.Clear();
                this.animatingCards.Clear();
                this.faceUpCards.Clear();
                this.memory.Clear();
            }
        }

        public void HandleUpdate()
        {
            if (this.waitDuration > 0)
            {
                this.waitElapsed += Time.deltaTime;

                if (this.waitElapsed < this.waitDuration)
                {
                    return;
                }
            }

            this.waitDuration = 0;
            this.waitElapsed = 0;

            bool animatingCards = this.isCardAnimating(this.animatingCards, true);

            if (this.State == RoundState.Selection)
            {
                if (this.selectedCards.Count >= 2)
                {
                    this.State = RoundState.Evaluation;
                }
                else if (this.ParticipantTurn == Participant.Player)
                {
                    if (Input.GetMouseButton(0))
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit = new RaycastHit();

                        int layerMask = 1;

                        if (Physics.Raycast(ray, out hit, 100, layerMask))
                        {
                            GameObject card = this.findCard(hit.transform.gameObject);
                            if (card != null && !this.faceUpCards.Contains(card))
                            {
                                this.SelectCard(card);
                            }
                        }
                    }
                }
                else if (this.ParticipantTurn == Participant.AI && !animatingCards)
                {
                    this.AIPickCard();
                }
            }

            if (this.State == RoundState.Evaluation || this.State == RoundState.EvaluationWait)
            {
                if (!animatingCards)
                {
                    if (this.isMatch(this.selectedCards))
                    {
                        if (this.State == RoundState.EvaluationWait)
                        {
                            // Increment score
                            this.Score[this.ParticipantTurn]++;

                            this.State = RoundState.Selection;
                            this.ToggleParticipantTurn();

                            // Drop cards
                            for (int i = this.selectedCards.Count - 1; i >= 0; i--)
                            {
                                this.faceUpCards.Add(this.selectedCards[i]);
                                this.memory.Remove(this.selectedCards[i]);
                                this.DeselectCard(this.selectedCards[i], this.dropCardAnimationState);
                            }

                            // add wait so that AI doesn't select card immediately.
                            if (this.ParticipantTurn == Participant.AI)
                            {
                                this.waitDuration = 1.5f;
                                this.waitElapsed = 0;
                            }

                            // Check for end of round
                            if (this.faceUpCards.Count >= this.Cards.Count)
                            {
                                this.EndRound();
                            }
                        }
                        else
                        {
                            this.State = RoundState.EvaluationWait;
                            this.waitDuration = this.EvaluationWaitTime;
                        }
                    }
                    else
                    {
                        if (this.State == RoundState.EvaluationWait)
                        {
                            for (int i = this.selectedCards.Count - 1; i >= 0; i--)
                            {
                                this.DeselectCard(this.selectedCards[i], this.deselectCardAnimationState);
                            }

                            this.State = RoundState.Selection;
                            this.ToggleParticipantTurn();

                            // add wait so that AI doesn't select card immediately.
                            if (this.ParticipantTurn == Participant.AI)
                            {
                                this.waitDuration = 1.5f;
                                this.waitElapsed = 0;
                            }
                        }
                        else
                        {
                            this.State = RoundState.EvaluationWait;
                            this.waitDuration = this.EvaluationWaitTime;
                        }
                    }
                }
            }
        }

        private void GenerateCards()
        {
            if (this.cardPrefab == null)
            {
                return;
            }

            if (this.Cards.Count > 0)
            {
                this.DestroyCards();
            }

            List<CardType> deck = new List<CardType>() {
                CardType.RED,
                CardType.RED,
                CardType.ORANGE,
                CardType.ORANGE,
                CardType.YELLOW,
                CardType.YELLOW,
                CardType.GREEN,
                CardType.GREEN,
                CardType.BLUE,
                CardType.BLUE,
                CardType.BLACK,
                CardType.BLACK
            };

            this.shuffleDeck(deck);

            float cardsWidth = (this.NumCardsX * this.cardWidth) + ((this.NumCardsX - 1) * this.SpaceBetweenCards);
            float cardsHeight = (this.NumCardsY * this.cardHeight) + ((this.NumCardsY - 1) * this.SpaceBetweenCards);
            float startX = this.container.transform.position.x - (cardsWidth / 2f) + (this.cardWidth / 2f);
            float startZ = this.container.transform.position.z - (cardsHeight / 2f) + (this.cardHeight / 2f);

            // Generate the cards
            for (int y = 0; y < this.NumCardsY; y++)
            {
                for (int x = 0; x < this.NumCardsX; x++)
                {
                    if (deck.Count > 0)
                    {
                        GameObject cardRoot = GameObject.Instantiate(this.cardPrefab);
                        GameObject card = cardRoot.transform.GetChild(0).gameObject;

                        card.GetComponent<Card>().CardType = deck[0];
                        card.GetComponent<MeshRenderer>().material = this.materials[deck[0]];
                        deck.RemoveAt(0);

                        cardRoot.transform.position = new Vector3(
                            startX + (x * this.cardWidth) + (x * this.SpaceBetweenCards),
                            0,
                            startZ + (y * this.cardHeight) + (y * this.SpaceBetweenCards));

                        GameObjectUtil.SetParentRetainLocal(cardRoot, container.transform);

                        this.Cards.Add(card);
                    }
                }
            }
        }

        private void DestroyCards()
        {
            foreach(GameObject card in this.Cards)
            {
                GameObject.Destroy(card);
            }

            this.Cards.Clear();
        }

        private void shuffleDeck(List<CardType> deck)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                int index = Random.Range(i, deck.Count);
                CardType type = deck[index];
                deck.RemoveAt(index);
                deck.Insert(0, type);
            }
        }

        private void SelectCard(GameObject card)
        {
            if (card != null && card.GetComponent<Card>() != null && !this.selectedCards.Contains(card))
            {
                this.selectedCards.Add(card);

                if (card.GetComponent<Animator>() != null)
                {
                    card.GetComponent<Animator>().Play(this.selectCardAnimationState, 0, 0);
                    this.animatingCards.Add(card);
                }

                // Remember shown cards for AI
                this.memory.Insert(0, card);
                if (this.memory.Count > this.MaxMemory)
                {
                    this.memory.RemoveRange(this.MaxMemory - 1, this.memory.Count - this.MaxMemory);
                }
            }
        }

        private void DeselectCard(GameObject card, string animationState)
        {
            if (card != null && card.GetComponent<Card>() != null && this.selectedCards.Contains(card))
            {
                this.selectedCards.Remove(card);

                if (card.GetComponent<Animator>() != null)
                {
                    card.GetComponent<Animator>().Play(animationState, 0, 0);
                    this.animatingCards.Add(card);
                }
            }
        }

        private GameObject findCard(GameObject obj)
        {
            Transform tran = obj.transform;

            if (obj.GetComponent<Card>() != null)
            {
                return obj;
            }

            while (tran.parent != null)
            {
                if (tran.parent.gameObject.GetComponent<Card>() != null)
                {
                    return tran.parent.gameObject;
                }

                tran = tran.parent.transform;
            }
            
            return null;
        }

        private bool isCardAnimating(List<GameObject> cards, bool removeCards)
        {
             bool isAnimating = false;

            List<GameObject> removeList = new List<GameObject>();

            foreach (GameObject card in cards)
            {
                if (!this.isAnimationEnded(card))
                {
                    isAnimating = true;
                }
                else if (removeCards)
                {
                    removeList.Add(card);
                }
            }

            foreach (GameObject card in removeList)
            {
                cards.Remove(card);
            }

            return isAnimating;
        }

        private bool isAnimationEnded(GameObject card)
        {
            return card.GetComponent<Animator>() != null 
                    && card.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime 
                        >= card.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
        }

        private bool isMatch(List<GameObject> cards)
        {
            bool match = true;
            CardType? previousCardType = null;

            foreach (GameObject card in cards)
            {
                if (card.GetComponent<Card>() != null)
                {
                    if (previousCardType != null
                        && card.GetComponent<Card>().CardType != previousCardType)
                    {
                        match = false;
                    }

                    previousCardType = card.GetComponent<Card>().CardType;
                }
                
            }

            return match;
        }

        private void ToggleParticipantTurn()
        {
            if (this.ParticipantTurn == Participant.Player)
            {
                this.ParticipantTurn = Participant.AI;
            }
            else
            {
                this.ParticipantTurn = Participant.Player;
            }
        }

        private void AIPickCard()
        {
            GameObject selection = null;

            // If there is a match in memory, then pick on of those cards.
            if (this.selectedCards.Count > 0)
            {
                foreach(GameObject card1 in this.memory)
                {
                    foreach(GameObject card2 in this.memory)
                    {
                        if (card1 != card2 
                            && card1.GetComponent<Card>() != null && card2.GetComponent<Card>() != null
                            && card1.GetComponent<Card>().CardType == card2.GetComponent<Card>().CardType
                            && !this.selectedCards.Contains(card1) && !this.faceUpCards.Contains(card1))
                        {
                            selection = card1;
                        }
                    }
                }
            }
            else
            {
                // If card matching currently selected card is in memory, then select that.
                foreach(GameObject card in this.memory)
                {
                    foreach(GameObject selectedCard in this.selectedCards)
                    {
                        if (card != selectedCard && card.GetComponent<Card>() != null && selectedCard.GetComponent<Card>() != null
                            && card.GetComponent<Card>().CardType == selectedCard.GetComponent<Card>().CardType
                            && !this.selectedCards.Contains(card) && !this.faceUpCards.Contains(card))
                        {
                            selection = card;
                        }
                    }

                    Debug.Log("Memory: " + card.GetComponent<Card>().CardType.ToString());
                }
            }

            while (selection == null && this.faceUpCards.Count < this.Cards.Count)
            {
                selection = this.Cards[Random.Range(0, this.Cards.Count)];

                if (this.faceUpCards.Contains(selection) || this.selectedCards.Contains(selection))
                {
                    selection = null;
                }
            }

            if (selection != null)
            {
                this.SelectCard(selection);
            }

            this.waitDuration = this.AIWaitTime;
            this.waitElapsed = 0;
        }
    }
}
