﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Balda
{
    public class Game
    {
        public FieldState State { get;  private set; }
        public Rules Rules { get; private set; }
        public List<Player> Players { get; private set; }

        public Game(string startWord, List<Player> players, Rules rules)
        {
            State = new FieldState(startWord);
            Rules = rules;
            Players = players;
        }

        public SortedDictionary<int, Player> play() 
        {
            bool gameEnded = false;
            while (!gameEnded) 
            {
                foreach (Player player in Players) 
                {
                    processPlayer(player);
                    gameEnded = winCondition();
                    if (gameEnded)
                    {
                        break;
                    }
                }
                // highlight the whole move
                // wait 4 secs
            }
            // form a dictionary of players, sorted by their game score
            // return IT, not null!
            return null;        
        }

        private bool winCondition()
        {
            return true;            
        }

        private void processPlayer(Player player)
        {
            Move move = new Move();
            do
            {
                player.Strategy.move(State, ref move, Rules);                
                switch (move.Action)
                {
                    case ActionType.EnterLetter:
                    {
                        int x = move.X, y = move.Y;
                        int width = State.Field.GetLength(0);
                        if (x < 0 || x >= width || y < 0 || y >= width)
                        {
                            // indices out of range
                            break;
                        }
                        move.Letter = Char.ToUpper(move.Letter);
                        if (!isCapitalRussianLetter(move.Letter))
                        {
                            // incorrect character
                            break;
                        }
                        if (State.NewX != -1 && State.NewY != -1)
                        {
                            // user already entered new letter
                            break;
                        }
                        State.NewX = x;
                        State.NewY = y;
                        State.Field[y, x] = move.Letter;
                    }
                    break;
                    case ActionType.SelectLetter:
                    {
                        int wordLength = State.X.Count; 
                        int x = move.X, y = move.Y;
                        bool isCorrect = true;
                        int width = State.Field.GetLength(0);
                        if (x < 0 || x >= width || y < 0 || y >= width)
                        {
                            // indices out of range
                            break;
                        }
                        char nullLetter = '\0';
                        if (State.Field[y, x] == nullLetter)
                        {//user must select only letters;
                            break;
                        }
                        if (wordLength == 0)
                        {
                            State.X.Add(x);
                            State.Y.Add(y);
                            break;
                        }
                        if (!Rules.AllowIntersections)
                        {
                            for (int i = 0; i < wordLength; i++)
                            {
                                if (State.X[i] == x && State.Y[i] == y)
                                {
                                    //new coordinate intersects with the word
                                    isCorrect = false;
                                    break;
                                }
                            }
                            if (!isCorrect) break;
                        }
                        if (Rules.isNeighbours(x, y, State.X[wordLength-1], State.Y[wordLength-1]))
                        {
                            State.X.Add(x);
                            State.Y.Add(y);
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
                }
            }
            while (move.Action != ActionType.EndTurn && move.Action != ActionType.PassTurn);
            // if the user chose non-existing word, we should ask him for another word instead of exitting
        }

        private bool isCapitalRussianLetter(Char letter)
        {
            return letter >= 'А' && letter <= 'Я';
        }
    }
}
