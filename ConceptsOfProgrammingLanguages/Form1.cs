﻿using Automata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConceptsOfProgrammingLanguages
{
    public partial class Form1 : Form
    {
        private IList<State> _State_arr = new List<State>();
        Selectable _selectable;
        private int _lastIndexOfState = 1;
        private State _sourceState = null;
        private State _destinationState = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void BtnAddState_Click(object sender, EventArgs e)
        {
            AddNewState();
        }

        private void InitProject()
        {
            foreach (var item in _State_arr)
            {
                automataView.DeleteSelectsable(item);
            }
            _State_arr.Clear();
            _lastIndexOfState = 1;

            automataView.BuildAutomata();
            automataView.Refresh();
            _handlerStep = 0;
            btnConvert.Text = "Chuyển đổi";
            btnAddState.Visible = true;
            btnAddArrow.Visible = true;
            gridView.DataSource = new DataTable();
        }

        private void AddNewState()
        {
            var state = new State("q" + _lastIndexOfState);
            _State_arr.Add(state);
            _lastIndexOfState++;
            automataView.States.Add(state);

            automataView.BuildAutomata();
            automataView.Refresh();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            automataView.ItemSelected += automataView_ItemSelected;
            KeyPreview = true;
        }

        private void automataView_ItemSelected(object sender, ItemSelectInfo info)
        {
            _selectable = info.selected;
        }
        private void automataView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && _selectable != null)
            {
                if (_selectable is StateConnector)
                {
                    itemFinalState.Enabled = false;
                    itemStartState.Enabled = false;
                    selectableContextMenu.Show(automataView, e.Location);
                }
                else if (_selectable is State)
                {
                    itemCurve.Checked = _selectable is State;
                    itemFinalState.Enabled = true;
                    itemStartState.Enabled = true;

                    if (automataView.IsStartState((State)_selectable))
                    {
                        itemStartState.Checked = true;
                    }
                    else
                    {
                        itemStartState.Checked = false;
                    }

                    if (automataView.IsFinalState((State)_selectable))
                    {
                        itemFinalState.Checked = true;
                    }
                    else
                    {
                        itemFinalState.Checked = false;
                    }
                    selectableContextMenu.Show(automataView, e.Location);
                }
            }
        }

        //khi click vao menu trip
        private void connectorContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int selItemIndex = selectableContextMenu.Items.IndexOf(e.ClickedItem);
            switch (selItemIndex)
            {
                case 0:
                    if (_selectable != null && _selectable is State)
                    {
                        State currentState = (State)_selectable;
                        automataView.SetStartState(currentState);
                    }
                    automataView.Invalidate();
                    break;
                case 1:
                    if (_selectable != null && _selectable is State)
                    {
                        var state = (State)_selectable;
                        automataView.SetFinalState((State)_selectable, automataView.IsFinalState(state) ? false : true);
                    }
                    automataView.Invalidate();
                    break;
                //xóa selectable
                case 2:
                    if (_selectable != null && ((_selectable is State) || (_selectable is StateConnector)))
                    {
                        automataView.DeleteSelectsable(_selectable);
                    }
                    break;
            }
            automataView.Refresh();
        }


        private bool _isMouseDown = false;
        private void AutomataView_MouseDown(object sender, MouseEventArgs e)
        {
            if (!automataView.isEnableMouseHere)
            {
                automataView.startPoint = e.Location;
                _isMouseDown = true;

                foreach (var item in _State_arr)
                {
                    if (item.HitTest(e.Location, automataView.Graphics))
                    {
                        _sourceState = item;
                        break;
                    }
                }
            }
        }

        private void BtnAddArrow_Click(object sender, EventArgs e)
        {
            automataView.isEnableMouseHere = !automataView.isEnableMouseHere;
            if (!automataView.isEnableMouseHere)
            {
                btnAddArrow.BackColor = Color.BlueViolet;
            }
            else
            {
                btnAddArrow.BackColor = Color.Transparent;
            }
        }

        private void AutomataView_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                automataView.endPoint = e.Location;
                Refresh();
            }
        }

        private void AutomataView_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isMouseDown && !automataView.isEnableMouseHere)
            {
                automataView.endPoint = e.Location;
                _isMouseDown = false;

                if (_sourceState != null)
                {
                    foreach (var item in _State_arr)
                    {
                        if (item.HitTest(e.Location, automataView.Graphics))
                        {
                            _destinationState = item;
                            break;
                        }
                    }
                    if (_destinationState != null)
                    {
                        char resultForm = new char();
                        using (FormInputValue form = new FormInputValue())
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                            {
                                resultForm = form.Result;
                            }
                        }
                        if (!string.IsNullOrEmpty(resultForm.ToString()) && !resultForm.Equals('\0'))
                        {
                            _sourceState.AddTransition(resultForm, _destinationState);
                            automataView.BuildAutomata();
                        }
                    }
                }
            }

            automataView.startPoint = automataView.endPoint = AutomataView.InitPoint;
            automataView.Refresh();
        }



        private void BtnRemove_Click(object sender, EventArgs e)
        {
            InitProject();
        }

        //
        //Xử lý chuyển đổi
        //

        private int _handlerStep = 0;

        IList<ItemTableConnector> _listItemConnector = new List<ItemTableConnector>();

        //chuot click
        private void BtnConvert_Click(object sender, EventArgs e)
        {
            IList<State> listFinalState = new List<State>();
            foreach (var item in _State_arr)
            {
                if (automataView.IsFinalState(item))
                {
                    listFinalState.Add(item);
                }
            }
            if (listFinalState.Count == 0)
            {
                MessageBox.Show("Lỗi: Không có trạng thái kết thúc");
                return;
            }
            else if (automataView.GetStartState() == null)
            {
                MessageBox.Show("Lỗi: Không có trạng thái bắt đầu");
                return;
            }

            switch (_handlerStep)
            {
                case 0:
                    HandlerStartState();
                    btnConvert.Text = "Bước tiếp";
                    btnAddState.Visible = false;
                    btnAddArrow.Visible = false;
                    break;
                case 1:
                    DetectFinalState();
                    break;
                case 2:
                    HandlerFinalState();
                    break;
                case 3:
                    CreateTransitionAllState();
                    break;
                case 4:
                    GroupStateConnector();
                    break;
                case 5:
                    HandlerConnectorLoopStates();
                    break;
                case 6:
                    HandlerConnectorLoopOneState();
                    break;
            }
            _handlerStep++;
        }
        //Xác định trạng thái cuối cùng hay không
        private void DetectFinalState()
        {
            IList<State> listFinalState = new List<State>();
            foreach (var item in _State_arr)
            {
                if (automataView.IsFinalState(item))
                {
                    listFinalState.Add(item);
                }
            }
            if (listFinalState.Count > 1)
            {
                CreateSigleFinalState(listFinalState);
            }
            else if (listFinalState.Count == 1)
            {
                MessageBox.Show("Automata không có nhiều hơn 1 trạng thái kết thúc");
            }
        }


        //thay đổi trạng thái cuối cùng nếu có connector quay ngược lại start state
        private void HandlerStartState()
        {
            var startState = automataView.GetStartState();
            var connectors = automataView.GetListStateConnectorBySigleState(startState, false);
            bool check = true;
            if (connectors.Count > 0)
            {
                foreach (var item in connectors)
                {
                    if (item.DestinationState != item.SourceState)
                    {
                        check = false;
                        break;
                    }
                }
            }
            if (!check)
            {
                var state = new State("q0");
                _State_arr.Add(state);
                automataView.States.Add(state);
                automataView.SetStartState(state);
                state.AddTransition(FaToReConverter.VALUE_E, startState);
                automataView.BuildAutomata();
                automataView.Refresh();
            }
            else
            {
                MessageBox.Show("Không có đường nối nào quay lại trạng thái kết thúc");
            }
        }

        //thay đổi trạng thái cuối cùng nếu có connector đi từ state cuối đến state khác
        private void HandlerFinalState()
        {
            var listFinalState = automataView.GetListFinalState();
            if (listFinalState.Count == 1)
            {
                var state = listFinalState.ElementAt(0);

                var connectors = automataView.GetListStateConnectorBySigleState(state, true);
                bool check = true;
                foreach (var item in connectors)
                {
                    if (item.SourceState != item.DestinationState)
                    {
                        check = false;
                        break;
                    }
                }
                if (!check)
                {
                    AddNewState();
                    var stateFinal = _State_arr.ElementAt(_State_arr.Count - 1);

                    foreach (var item in listFinalState)
                    {
                        automataView.SetFinalState(item, false);
                        break;
                    }
                    automataView.SetFinalState(stateFinal, true);
                    state.AddTransition(FaToReConverter.VALUE_E, stateFinal);

                    automataView.BuildAutomata();
                    automataView.Refresh();
                }
                else
                {
                    MessageBox.Show("Không có đường nối từ trạng thái kết thúc sang trạng thái khác");
                }
            }
        }

        //Nhóm trạng thái khi chuyển từ state1 -> state1
        private void GroupStateConnector()
        {
            foreach (var item in automataView.GetListConnector())
            {
                if (item.Label.Text.Contains(','))
                {
                    automataView.SetStateConnectorInList(FaToReConverter.GrossConnector(item));
                }
            }

            CreateDataTable(_listItemConnector);
            automataView.Refresh();
        }

        //Tạo tất cả các đường nối giữa các trạng thái
        private void CreateTransitionAllState()
        {

            var listStateConnector = automataView.GetListConnector();

            foreach (var item1 in _State_arr)
            {
                foreach (var item2 in _State_arr)
                {
                    string value;
                    StateConnector tmp = null;
                    foreach (var item in listStateConnector)
                    {
                        if (item.SourceState == item1 && item.DestinationState == item2)
                        {
                            tmp = item;
                            break;
                        }
                    }
                    if (tmp != null)
                    {
                        value = tmp.Label.Text.Replace(" ", FaToReConverter.LAMBDA).Replace(",", FaToReConverter.OR);
                    }
                    else
                    {
                        value = Extention.VALUE_NULL.ToString();
                        item1.AddTransition(Extention.VALUE_NULL, item2);
                    }
                    _listItemConnector.Add(new ItemTableConnector()
                    {
                        SourceState = item1,
                        DestinationState = item2,
                        Value = value
                    });
                }
            }

            automataView.BuildAutomata();
            automataView.Refresh();
        }

        //Tạo bảng giá trị
        private void CreateDataTable(IList<ItemTableConnector> listItemConnector)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("Trạng thái bắt đầu");
            dt.Columns.Add("Trạng thái kết thúc");
            dt.Columns.Add("Giá trị");

            foreach (var item in listItemConnector)
            {
                dt.Rows.Add(item.SourceState.Label, item.DestinationState.Label, item.Value);
            }

            gridView.DataSource = dt;
        }


        //Tạo duy nhất 1 trạng thái kết thúc
        private void CreateSigleFinalState(IList<State> listFinalState)
        {
            AddNewState();
            var stateFinal = _State_arr.ElementAt(_State_arr.Count - 1);
            automataView.SetFinalState(stateFinal);

            foreach (var item in listFinalState)
            {
                automataView.SetFinalState(item, false);
                item.AddTransition(Extention.VALUE_E, stateFinal);
            }
            automataView.BuildAutomata();
            automataView.Refresh();
        }

        //biến những đường tạo với nhau thành vòng lặp và biến những đường 2 chiều thành 1: BƯỚC 1
        private void HandlerConnectorLoopStates()
        {
            var listConnector = automataView.GetListConnector();
            var listRestoreConnector = new List<StateConnector>();
            foreach (var connector in listConnector)
            {
                if (connector.SourceState != connector.DestinationState)
                {
                    if (connector.Label.Text != FaToReConverter.VALUE_NULL.ToString())
                    {
                        bool check = true;
                        foreach (var passed in listRestoreConnector)
                        {
                            if (connector.SourceState == passed.SourceState &&
                                connector.DestinationState == passed.DestinationState &&
                                connector.Label.Text != FaToReConverter.VALUE_NULL.ToString())
                            {
                                check = false;
                                break;
                            }
                        }

                        if (check)
                        {
                            StateConnector revertConnector = automataView.GetStateConnectorByState(connector.DestinationState, connector.SourceState);
                            StateConnector loopConnector = automataView.GetStateConnectorByState(connector.SourceState, connector.SourceState);

                            if (revertConnector != null && loopConnector != null)
                            {
                                loopConnector.Label.Text = Extention.JoinString(loopConnector.Label.Text,
                                                            Extention.JoinString(connector.Label.Text, revertConnector.Label.Text, FaToReConverter.LAMBDA),
                                                            FaToReConverter.OR);
                                automataView.DeleteSelectsable(revertConnector);
                                listRestoreConnector.Add(revertConnector);
                            }
                        }
                    }
















                }
            }
            automataView.Refresh();
        }


        //chuyển đổi đường đi qua chính nó ( đường lặp), BƯỚC 2
        private void HandlerConnectorLoopOneState()
        {
            foreach (var item in automataView.GetListConnector())
            {
                if (item.SourceState == item.DestinationState
                    && item.Label.Text != Extention.VALUE_NULL.ToString())
                {
                    State currenState = item.SourceState;
                    IList<StateConnector> statesConnectionBefore = new List<StateConnector>();
                    IList<StateConnector> statesConnectionAfter = new List<StateConnector>();
                    foreach (var st in automataView.GetListConnector())
                    {
                        if (st.DestinationState != st.SourceState && st.DestinationState == currenState)
                        {
                            statesConnectionBefore.Add(st);
                        }
                        else if (st.DestinationState != st.SourceState && st.SourceState == currenState)
                        {
                            statesConnectionAfter.Add(st);
                        }
                    }

                    if (statesConnectionBefore.Count > 0)
                    {
                        foreach (var st in statesConnectionBefore)
                        {
                            st.Label.Text = Extention.JoinString(st.Label.Text, Extention.GroupString(item.Label.Text), FaToReConverter.LAMBDA);
                        }
                    }
                    else if (statesConnectionAfter.Count > 0)
                    {
                        foreach (var st in statesConnectionAfter)
                        {
                            st.Label.Text = Extention.JoinString(Extention.GroupString(item.Label.Text), st.Label.Text, FaToReConverter.LAMBDA);
                        }
                    }
                    automataView.DeleteSelectsable(item);
                }
                else if (item.SourceState == item.DestinationState
                    && item.Label.Text == Extention.VALUE_NULL.ToString())
                {
                    automataView.DeleteSelectsable(item);
                }
            }
            automataView.Refresh();
        }

        //Loại bỏ state trung gian, BƯỚC 3
        private void RemoveStateIntermediate()
        {
            foreach (var item in _State_arr)
            {
                if (!automataView.IsFinalState(item) && !automataView.IsStartState(item))
                {
                    IList<StateConnector> connectorsBefore = automataView.GetListStateConnectorBySigleState(item, false);
                    IList<StateConnector> connectorsAfter = automataView.GetListStateConnectorBySigleState(item, true);

                    foreach (var i1 in connectorsBefore)
                    {
                        foreach (var i2 in connectorsAfter)
                        {
                            i1.SourceState.AddTransition(i1.Label.Text + FaToReConverter.OR + i2.Label.Text, i2.DestinationState);
                        }
                    }
                    foreach (var i in connectorsBefore)
                    {
                        automataView.DeleteSelectsable(i);
                    }
                    foreach (var i in connectorsAfter)
                    {
                        automataView.DeleteSelectsable(i);
                    }
                    automataView.DeleteSelectsable(item);
                }
            }
            automataView.BuildAutomata();
            automataView.Refresh();
        }
    }
}
