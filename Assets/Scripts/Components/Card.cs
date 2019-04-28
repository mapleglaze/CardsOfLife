using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MapleGlaze.CardsOfLife.Constants;

namespace MapleGlaze.CardsOfLife.Components
{
    public class Card : MonoBehaviour
    {
        private CardType cardType;
        public CardType CardType {
            get { return this.cardType; }
            set { this.cardType = value; }
        }
    }
}