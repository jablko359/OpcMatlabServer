// Copyright (c) Traeger Industry Components GmbH. All Rights Reserved.

using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using Opc.Ua;
using Opc.UaFx.Client;

namespace Server
{
    using System.Collections.Generic;

    using Opc.UaFx;
    using Opc.UaFx.Server;

    /// <summary>
    /// Represents a sample implementation of a custom OpcNodeManager.
    /// </summary>
    internal class MatlabNodeManager : OpcNodeManager
    {
        private delegate List<string> MyDeleagate();

        private readonly MatlabService _matlabService;
        private OpcFolderNode _matlabFolder;
        private readonly Dictionary<string, object> _currentVariables = new Dictionary<string, object>();
        private readonly Dictionary<string, OpcDataVariableNode> _variableNodesIds = new Dictionary<string, OpcDataVariableNode>();


        private Timer _refreshTimer = new Timer();

        public const string GetVariablesMethod = "GetWorkspaceVariables";
        public const string MatlabFolderName = "MatlabWorkspace";
        #region Constructors

        public MatlabNodeManager(MatlabService matlabService)
            : base("http://sampleserver/machines")
        {
            _matlabService = matlabService;
            if (Properties.Settings.Default.UpdateInterval != 0)
            {
                if (Properties.Settings.Default.UpdateInterval < 5000)
                {
                    throw new ApplicationException("Refresh interval cannot be lower than 5s");
                }
                _refreshTimer.AutoReset = true;
                _refreshTimer.Interval = Properties.Settings.Default.UpdateInterval;
                _refreshTimer.Elapsed += _refreshTimer_Elapsed;
                _refreshTimer.Start();
            }
            

        }

        private void _refreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateVariables();
        }

        #endregion

        #region Protected  methods


        protected override IEnumerable<IOpcNode> CreateNodes(OpcNodeReferenceCollection references)
        {
            _matlabFolder = new OpcFolderNode(
                    new OpcName(MatlabFolderName, this.DefaultNamespaceIndex));


            references.Add(_matlabFolder, OpcObjectTypes.ObjectsFolder);
            MyDeleagate del = UpdateVariables;

            new OpcMethodNode(_matlabFolder, GetVariablesMethod, del);
            var v = new OpcDataVariableNode<int>(_matlabFolder, new OpcName("GetVariablesVariable"), 0);//new OpcVariableNode(_matlabFolder,new OpcName("GetVariablesVariable"), 0);
            v.WriteVariableValueCallback = WriteVariableValueCallback;
            v.AccessLevel = OpcAccessLevel.CurrentReadOrWrite;
            Console.WriteLine();
            UpdateVariables();

            return new IOpcNode[] { _matlabFolder };
        }

        private OpcVariableValue WriteVariableValueCallback(OpcWriteVariableValueContext context, OpcVariableValue value)
        {
            UpdateVariables();
            return value;
        }

        #endregion
        #region Prievate methods
        private List<string> UpdateVariables()
        {
            try
            {
                HashSet<string> variableList = new HashSet<string>();
                _matlabService.Matlab.Execute(string.Format("{0} = who", MatlabService.VariablesTag));
                object vars;
                _matlabService.Matlab.GetWorkspaceData(MatlabService.VariablesTag, MatlabService.BaseTag, out vars);
                object[,] variables = vars as object[,];
                if (variables != null)
                {
                    for (int i = 0; i < variables.GetLength(0); i++)
                    {
                        string variableName = variables[i, 0].ToString();
                        if (variableName != MatlabService.VariablesTag)
                        {
                            variableList.Add(variableName);
                            object variable = _matlabService.Matlab.GetVariable(variableName, MatlabService.BaseTag);
                            OpcDataVariableNode dataNode;
                            if (!_variableNodesIds.TryGetValue(variableName, out dataNode))
                            {
                                dataNode = new OpcDataVariableNode(_matlabFolder,
                                    new OpcName(variableName), variable);
                                dataNode.WriteVariableValueCallback = WriteMatlabVariableValueCallback;
                                _variableNodesIds.Add(variableName, dataNode);
                                AddNode(dataNode);
                                Console.WriteLine("Added variable {0} : {1}", variableName, variable);
                            }
                            else if(!dataNode.Value.Equals(variable))
                            {
                                dataNode.Value = variable;
                                Console.WriteLine("Modified variable {0} : {1}", variableName, variable);
                            }
                            dataNode.DataType = OpcDataTypes.GetDataType(variable.GetType());
                        }
                    }
                    //Remove variables that no longer exists
                    List<string> keys = _variableNodesIds.Keys.ToList();
                    foreach (string key in keys)
                    {
                        if (!variableList.Contains(key))
                        {
                            IOpcNode removeNode = _variableNodesIds[key];
                            this.RemoveNode(removeNode);
                            _variableNodesIds.Remove(key);
                            Console.WriteLine("Removed variable: {0}", key);
                        }
                    }
                }
                return variableList.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Update variables error:{0}{1}{0}{2}", Environment.NewLine, ex.Message, ex.StackTrace);
                return null;
            }
        }

        private OpcVariableValue WriteMatlabVariableValueCallback(OpcWriteVariableValueContext context, OpcVariableValue value)
        {
            try
            {
                string variableName = context.Node.Name.SymbolicName;
                string variableValueAsString = value.Value.ToString();
                double n;
                bool isNumeric = double.TryParse(variableValueAsString, out n);
                if (isNumeric)
                {
                    _matlabService.Matlab.Execute(string.Format("{0} = {1}", variableName, variableValueAsString));
                }
                else
                {
                    _matlabService.Matlab.Execute(string.Format("{0} = '{1}'", variableName, variableValueAsString));
                }
                UpdateVariables();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong while modifying variable:{0}{1}{0}{2}", Environment.NewLine, ex.Message, ex.StackTrace);
            }

            return value;
        }

        #endregion
    }
}
