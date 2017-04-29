﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Balda
{
    delegate void formUpdatingDelegate(FieldState f, Rules r);

    public partial class GamingForm : Form
    {
        Mutex humanMoveMutex;
        Thread playingThread;
        private Move humanMove;
        public Move HumanMove
        {
            get
            {
                humanMoveMutex.WaitOne();
                if (humanMove == null)
                {
                    humanMoveMutex.ReleaseMutex();
                    return null;
                }
                Move moveCopy = (Move)humanMove.Clone();
                humanMove = null;
                humanMoveMutex.ReleaseMutex();
                return moveCopy;
            }
            private set
            {
                humanMove = value;
            }
        }

        private Game game;
        public Game Game
        {
            set
            {
                game = value;
            }
        }

        public GamingForm()
        {
            humanMoveMutex = new Mutex();
            InitializeComponent();
        }

        private void GamingForm_Shown(object sender, EventArgs e)
        {
            // Thread playing = new Thread( new ThreadStart(play));
            //  playing.Start();
            playingThread = new Thread(() => game.play());
            playingThread.Start();
        }

        public void updateForm(FieldState state, Rules rules)
        {
            if (this.InvokeRequired)
            {
                formUpdatingDelegate fud = new formUpdatingDelegate(updateForm);
                this.Invoke(fud, new object[] { state, rules });
            }
            else
            {
                fieldDataGridView.RowCount = state.Field.GetLength(0);
                fieldDataGridView.ColumnCount = state.Field.GetLength(1);
                for (int i = 0; i < state.Field.GetLength(0); ++i)
                {
                    fieldDataGridView.Rows[i].Height = fieldDataGridView.Size.Height / state.Field.GetLength(1);
                    for (int j = 0; j < state.Field.GetLength(1); ++j)
                    {
                        fieldDataGridView.Rows[i].Cells[j].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        fieldDataGridView.Rows[i].Cells[j].Value = state.Field[i, j];
                        fieldDataGridView.Rows[i].Cells[j].ReadOnly = true;
                    }
                }
                // ↓ is needed to remove the scroll bar
                fieldDataGridView.Rows[0].Height = fieldDataGridView.Size.Height / state.Field.GetLength(1) - 2;
                int newX = state.NewX;
                int newY = state.NewY;
                if (newX != -1 && newY != -1)
                {
                    // highlight the new letter
                    fieldDataGridView.Rows[newY].Cells[newX].Style.ForeColor = Color.Red;
                }
                else
                {
                    // if availableCells[i, j] = true, then the user is permitted to place letters there
                    bool[,] availableCells = rules.findAvailableCells(state);
                    // set all the cells to match this array
                    for (int i = 0; i < state.Field.GetLength(0); ++i)
                    {
                        for (int j = 0; j < state.Field.GetLength(1); ++j)
                        {
                            fieldDataGridView.Rows[i].Cells[j].ReadOnly = !availableCells[i, j];
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            playingThread.Abort();
            this.Owner.Show();
            Close();
        }

        private void fieldDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int rowIdx = e.RowIndex;
            int colIdx = e.ColumnIndex;
            string newValue = fieldDataGridView.Rows[rowIdx].Cells[colIdx].Value.ToString();
            if (newValue.Length == 1)
            {
                humanMoveMutex.WaitOne();
                this.humanMove = new Move();
                this.humanMove.Action = ActionType.EnterLetter;
                this.humanMove.Letter = newValue[0];
                this.humanMove.X = colIdx;
                this.humanMove.Y = rowIdx;
                MessageBox.Show(humanMove.ToString());
                humanMoveMutex.ReleaseMutex();
            }
            else
            {
                fieldDataGridView.Rows[rowIdx].Cells[colIdx].Value = "\0";
            }
        }
    }
}
