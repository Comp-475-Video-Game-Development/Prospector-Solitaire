using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Deck : MonoBehaviour
{
    [Header("Set in Inspector")]
    public bool startFaceUp = false;

    // Suits
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;
    public Sprite[] faceSprites;
    public Sprite[] rankSprites;
    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;
    // Prefabs
    public GameObject prefabSprite;
    public GameObject prefabCard;

    //Initial Vars
    public bool ________________;
    public PT_XMLReader xmlr;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;

    // InitDeck is called by Prospector when it is ready
    public void InitDeck(string deckXMLText)
    {
        // This creates an anchor for all the Card GameObjects in the Hierarchy
        if (GameObject.Find("_Deck") == null)
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }
        // Initialize the Dictionary of SuitSprites with necessary Sprites
        dictSuits = new Dictionary<string, Sprite>() {
            { "C", suitClub },
            { "D", suitDiamond },
            { "H", suitHeart },
            { "S", suitSpade }
        };
        ReadDeck(deckXMLText);
        MakeCards();
    }
    // ReadDeck parses the XML file passed to it into CardDefinitions
    public void ReadDeck(string deckXMLText)
    {
        xmlr = new PT_XMLReader(); // Create a new PT_XMLReader
        xmlr.Parse(deckXMLText);   // Use that PT_XMLReader to parse DeckXML
                                   // This prints a test line to show you how xmlr can be used.
                                   // For more information read about XML in the Useful Concepts Appendix
        string s = "xml[0] decorator[0] ";
        s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
        s += " x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        s += " y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        s += " scale=" + xmlr.xml["xml"][0]["decorator"][0].att("scale");
        //print(s); // Comment out this line, since we're done with the test
        // Read decorators for all Cards
        decorators = new List<Decorator>(); // Init the List of Decorators
        // Grab a PT_XMLHashList of all <decorator>s in the XML file
        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        Decorator deco;
        for (int i = 0; i < xDecos.Count; i++)
        {
            // For each <decorator> in the XML
            deco = new Decorator(); // Make a new Decorator
            // Copy the attributes of the <decorator> to the Decorator
            deco.type = xDecos[i].att("type");
            // Set the bool flip based on whether the text of the attribute is
            //   "1" or something else. This is an atypical but perfectly fine
            //   use of the == comparison operator. It will return a true or
            //   false, which will be assigned to deco.flip.
            deco.flip = (xDecos[i].att("flip") == "1");
            // floats need to be parsed from the attribute strings
            deco.scale = float.Parse(xDecos[i].att("scale"));
            // Vector3 loc initializes to [0,0,0], so we just need to modify it
            deco.loc.x = float.Parse(xDecos[i].att("x"));
            deco.loc.y = float.Parse(xDecos[i].att("y"));
            deco.loc.z = float.Parse(xDecos[i].att("z"));
            // Add the temporary deco to the List decorators
            decorators.Add(deco);
        }
        // Read pip locations for each card number
        cardDefs = new List<CardDefinition>(); // Init the List of Cards
        // Grab a PT_XMLHashList of all the <card>s in the XML file
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
        for (int i = 0; i < xCardDefs.Count; i++)
        {
            // For each of the <card>s
            // Create a new CardDefinition
            CardDefinition cDef = new CardDefinition();
            // Parse the attribute values and add them to cDef
            cDef.rank = int.Parse(xCardDefs[i].att("rank"));
            // Grab a PT_XMLHashList of all the <pip>s on this <card>
            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if (xPips != null)
            {
                for (int j = 0; j < xPips.Count; j++)
                {
                    // Iterate through all the <pip>s
                    deco = new Decorator();
                    // <pip>s on the <card> are handled via the Decorator Class
                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");
                    deco.loc.x = float.Parse(xPips[j].att("x"));
                    deco.loc.y = float.Parse(xPips[j].att("y"));
                    deco.loc.z = float.Parse(xPips[j].att("z"));
                    if (xPips[j].HasAtt("scale"))
                    {
                        deco.scale = float.Parse(xPips[j].att("scale"));
                    }
                    cDef.pips.Add(deco);
                }
            }
            // Face cards (Jack, Queen, & King) have a face attribute
            // cDef.face is the base name of the face card Sprite
            // e.g., FaceCard_11 is the base name for the Jack face Sprites
            // the Jack of Clubs is FaceCard_11C, hearts is FaceCard_11H, etc.
            if (xCardDefs[i].HasAtt("face"))
            {
                cDef.face = xCardDefs[i].att("face");
            }
            cardDefs.Add(cDef);
        }

    }

    // Get the proper CardDefinition based on Rank (1 to 14 is Ace to King)
    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        // Search through all of the CardDefinitions
        foreach (CardDefinition cd in cardDefs)
        {
            // If the rank is correct, return this definition
            if (cd.rank == rnk)
            {
                return (cd);
            }
        }
        return (null);
    }
    // Make the Card GameObjects
    public void MakeCards()
    {
        // cardNames will be the names of cards to build
        // Each suit goes from 1 to 13 (e.g., C1 to C13 for Clubs)
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters)
        {
            for (int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }
        // Make a List to hold all the cards
        cards = new List<Card>();

        for (int i = 0; i < cardNames.Count; i++)
        {
            cards.Add(MakeCard(i));
        }
    }

    private Card MakeCard(int cNum)
    {
        GameObject cgo = Instantiate(prefabCard) as GameObject;

        cgo.transform.parent = deckAnchor;
        Card card = cgo.GetComponent<Card>();

        cgo.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);

        card.name = cardNames[cNum];
        card.suit = card.name[0].ToString();
        card.rank = int.Parse(card.name.Substring(1));

        if (card.suit == "D")
        {
            Debug.Log(card);
        }

        if (card.suit == "D" || card.suit == "H")
        {
            card.colS = "Red";
            card.color = Color.red;
        }

        card.def = GetCardDefinitionByRank(card.rank);

        AddDecorators(card);
        AddPips(card);
        AddFace(card);
        AddBack(card);

        return card;
    }

    private Sprite _tSp = null;
    private GameObject _tGO = null;
    private SpriteRenderer _tSR = null;

    private void AddDecorators(Card card)
    {
        foreach ( Decorator deco in decorators)
        {
            if (deco.type == "suit")
            {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                _tSR.sprite = dictSuits[card.suit];
            }
            else
            {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                _tSp = rankSprites[card.rank];
                _tSR.sprite = _tSp;
                _tSR.color = card.color;
            }

            _tSR.sortingOrder = 1;
            _tGO.transform.SetParent(card.transform);
            _tGO.transform.localPosition = deco.loc;

            if (deco.flip)
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            if (deco.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * deco.scale;
            }

            _tGO.name = deco.type;
            card.decoGOs.Add(_tGO);
        }
    }

    private void AddPips(Card card)
    {
        foreach (Decorator pip in card.def.pips)
        {
            _tGO = Instantiate(prefabSprite) as GameObject;
            _tGO.transform.SetParent(card.transform);
            _tGO.transform.localPosition = pip.loc;

            if (pip.flip)
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            if (pip.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }

            _tGO.name = "pip";
            _tSR = _tGO.GetComponent<SpriteRenderer>();
            _tSR.sprite = dictSuits[card.suit];
            _tSR.sortingOrder = 1;
            card.pipGOs.Add(_tGO);
        }
    }

    private void AddFace(Card card)
    {
        if (card.def.face == "")
        {
            return;
        }

        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSp = GetFace(card.def.face + card.suit);
        _tSR.sprite = _tSp;
        _tSR.sortingOrder = 1;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        _tGO.name = "face";
    }

    private Sprite GetFace(string faceS)
    {
        foreach (Sprite _tSP in faceSprites)
        {
            if (_tSP.name == faceS)
            {
                return (_tSP);
            }
        }

        return null;
    }

    private void AddBack(Card card)
    {
        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSR.sprite = cardBack;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        _tSR.sortingOrder = 2;
        _tGO.name = "back";
        card.back = _tGO;
        card.faceUp = startFaceUp;
    }

    // Shuffle the Cards in Deck.cards
    static public void Shuffle(ref List<Card> oCards)
    {                     // 1
        // Create a temporary List to hold the new shuffle order
        List<Card> tCards = new List<Card>();
        int ndx; // This will hold the index of the card to be moved
        tCards = new List<Card>(); // Initialize the temporary List
        // Repeat as long as there are cards in the original List
        while (oCards.Count > 0)
        {
            // Pick the index of a random card
            ndx = Random.Range(0, oCards.Count);
            // Add that card to the temporary List
            tCards.Add(oCards[ndx]);
            // And remove that card from the original List
            oCards.RemoveAt(ndx);
        }
        // Replace the original List with the temporary List
        oCards = tCards;
        // Because oCards is a reference variable, the original that was
        //   passed in is changed as well.
    }
}