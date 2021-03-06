﻿using Automata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConceptsOfProgrammingLanguages
{
    public class FaToReConverter
    {
        public static string EMPTY = "Ø";
        public static string LAMBDA = "";
        public static string KLEENE_STAR = "*";
        public static string OR = "+";
        public static string RIGHT_PAREN = ")";
        public static string LEFT_PAREN = "(";
        public static char VALUE_E = 'ε';
        public static char VALUE_NULL = 'Ø';

        public static StateConnector GrossConnector(StateConnector stateConnector)
        {
            string label = stateConnector.Label.Text.Replace(" ", LAMBDA).Replace(",", OR);
            stateConnector.Label.Text = label;
            return stateConnector;
        }
    }
}
