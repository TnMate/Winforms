using System;
using System.Drawing;
using System.Windows.Forms;
using Tetris.Model;
using Tetris.Persistence;

namespace Tetris
{
    public partial class TetrisForm : Form
    {

        #region Fields

        private TetrisDataAccessInterface _dataAccess;
        private TetrisModel _model;
        private Button[,] _buttonGrid;
        private Timer _timer;
        private Boolean _timerActive;

        #endregion

        #region Constructors

        public TetrisForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Form Event Handler

        private void TetrisForm_Load_1(object sender, EventArgs e)
        {
            _dataAccess = new TetrisFileDataAccess();

            _model = new TetrisModel(_dataAccess);
            _model.GameAdvanced += new EventHandler<TetrisEventArgs>(model_gameAdvanced);
            _model.GameOver += new EventHandler<TetrisEventArgs>(model_gameOver);

            KeyPreview = true;
            KeyDown += new KeyEventHandler(TetrisForm_KeyDown);

            NewGame();
        }

        #endregion

        #region Private Methods

        private void GenerateTable()
        {
            if (_buttonGrid != null)
            {
                foreach (Button button in _buttonGrid)
                {
                    Controls.Remove(button);
                }
            }

            _buttonGrid = new Button[_model.getTableSize(), 16];
            for (Int32 i = 0; i < _model.getTableSize(); i++)
                for (Int32 j = 0; j < 16; j++)
                {
                    _buttonGrid[i, j] = new Button();
                    _buttonGrid[i, j].BackColor = Color.White;
                    _buttonGrid[i, j].Location = new Point(5 + 25 * j, 25 + 25 * i);
                    _buttonGrid[i, j].Size = new Size(25, 25);
                    _buttonGrid[i, j].Enabled = false; 
                    _buttonGrid[i, j].TabIndex = 100 + i * _model.getTableSize() + j;
                    _buttonGrid[i, j].FlatStyle = FlatStyle.Flat;

                    Controls.Add(_buttonGrid[i, j]);
                }
        }

        private void SetupWindow()
        {
            switch (_model.getTableSize())
            {
                case 4:
                    this.Width = 425;
                    this.Height = 200;
                    break;
                case 8:
                    this.Width = 425;
                    this.Height = 300;
                    break;
                case 12:
                    this.Width = 425;
                    this.Height = 400;
                    break;
                default:
                    break;
            }
        }

        private void NewGame()
        {
            _model.newGame();
            SetupWindow();
            GenerateTable();
            if (_timer!=null)
                _timer.Dispose();

            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += new EventHandler(Timer_Tick);
            _timer.Start();
            _timerActive = true;

            _actualTime.Text = _model.getTime().ToString();
            SetupCurrentTableState();

            pauseToolStripMenuItem.Enabled = true;
        }

        private void SetupCurrentTableState()
        {
            if (_buttonGrid != null)
            {
                var map = _model.getTable();

                for (Int32 i = 0; i < _model.getTableSize(); i++)
                    for (Int32 j = 0; j < 16; j++)
                    {
                        if (map[i + 1, j] == 1)
                        {
                            _buttonGrid[i, j].BackColor = Color.Purple;
                        }
                        else if (map[i + 1, j] == 2)
                        {
                            _buttonGrid[i, j].BackColor = Color.Orange;
                        }
                        else if (map[i + 1, j] == 3)
                        {
                            _buttonGrid[i, j].BackColor = Color.Green;
                        }
                        else if (map[i + 1, j] == 4)
                        {
                            _buttonGrid[i, j].BackColor = Color.Yellow;
                        }
                        else if (map[i + 1, j] == 5)
                        {
                            _buttonGrid[i, j].BackColor = Color.Red;
                        }
                        else if (map[i + 1, j] == 6)
                        {
                            _buttonGrid[i, j].BackColor = Color.Blue;
                        }
                        else if (map[i + 1, j] == 7)
                        {
                            _buttonGrid[i, j].BackColor = Color.Turquoise;
                        }
                        else
                        {
                            _buttonGrid[i, j].BackColor = Color.White;
                        }
                    }
            }
        }

        #endregion

        #region Timer event handlers

        private void Timer_Tick(Object sender, EventArgs e)
        {
            _model.advanceTime();
            _actualTime.Text = _model.getTime().ToString();
        }

        #endregion

        #region Other event handlers

        private void model_gameAdvanced(Object sender, TetrisEventArgs e)
        {
            SetupCurrentTableState();
        }

        private void model_gameOver(Object sender, TetrisEventArgs e)
        {
            _timer.Stop();
            _timerActive = false;
            pauseToolStripMenuItem.Enabled = false;
            MessageBox.Show("Game Over!" + Environment.NewLine +
                               e.ReturnGameTime + " time",
                               "Tetris game",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Asterisk);
        }

        private void TetrisForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (_buttonGrid != null && _timerActive)
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        _model.GoLeft();
                        break;
                    case Keys.Right:
                        _model.GoRight();
                        break;
                    case Keys.Down:
                        _model.GoDown();
                        break;
                    case Keys.Up:
                        _model.RotateMeSenpai();
                        break;
                    default:
                        break;
                }
                e.SuppressKeyPress = true;
            }
            SetupCurrentTableState();
        }

        #endregion

        #region menu handlers

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewGame();
        }

        private void smallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _model.SetTableSize(0);
            NewGame();
        }

        private void mediumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _model.SetTableSize(1);
            NewGame();
        }

        private void largeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _model.SetTableSize(2);
            NewGame();
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_timerActive)
            {
                _timer.Stop();
                _timerActive = false;
            }
            else
            {
                _timer.Start();
                _timerActive = true;
            }
        }

        private async void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Boolean restartTimer = _timer.Enabled;
            _timer.Stop();

            if (_saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await _model.SaveGameAsync(_saveFileDialog.FileName);
                }
                catch (TetrisDataException)
                {
                    MessageBox.Show("Couldn't save the game!" + Environment.NewLine + "Wrong path or you don't have the permission to write to the directory", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception)
                {
                    MessageBox.Show("Couldn't save the game!" + Environment.NewLine + "You can't save a lost game!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (restartTimer)
                _timer.Start();
        }

        private async void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Boolean restartTimer = _timer.Enabled;
            _timer.Stop();

            if (_openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await _model.LoadGameAsync(_openFileDialog.FileName);
                    saveToolStripMenuItem.Enabled = true;
                }
                catch (TetrisDataException)
                {
                    MessageBox.Show("Couldn't load the game!" + Environment.NewLine + "Wrong path or file format", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    _model.newGame();
                    GenerateTable();
                    saveToolStripMenuItem.Enabled = true;
                    pauseToolStripMenuItem.Enabled = true;

                }

                SetupWindow();
                GenerateTable();
                SetupCurrentTableState();
                pauseToolStripMenuItem.Enabled = true;

            }

            if (restartTimer)
                _timer.Start();
        }

        #endregion

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
