using System;
using System.Collections.Generic;

namespace Server
{
    public class MatlabService
    {
        #region Private
        private MLApp.MLApp _matlab;
        private readonly MatlabServiceType _serviceType;

        public const string VariablesTag = "variables";
        public const string BaseTag = "base";
        public const string ArrayTag = "Array";
        #endregion
        #region Properties
        public MatlabServiceType ServiceType
        {
            get { return _serviceType; }
        }

        public MLApp.MLApp Matlab
        {
            get { return _matlab; }
        }
        #endregion
        #region Ctor
        public MatlabService(MatlabServiceType serviceType)
        {
            _serviceType = serviceType;
            switch (serviceType)
            {
                case MatlabServiceType.Desktop:
                    InitDesktopMatlabService();
                    break;
                case MatlabServiceType.Background:
                    InitBackgroundMatlabService();
                    break;
            }
        }
        public Dictionary<string, object> GetMatlabStruct(string variableName)
        {
            Dictionary<string, object> matlabStruct = new Dictionary<string, object>();
            //string variableString = Matlab.Execute(variableName);
            //string[] splitted = variableString.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            //for (int i = 0; i < splitted.Length; i++)
            //{
            //    string splittedString = splitted[i];
            //    if (splittedString.EndsWith(":"))
            //    {
            //        splittedString = splittedString.TrimEnd(new[] {':'});
            //        object property = Matlab.GetVariable(string.Format("{0}.{1}", variableName, splittedString), BaseTag);
            //        if (property == null)
            //        {
            //            GetMatlabStruct(splittedString);
            //        }
            //    }
            //}
            return matlabStruct;
        }
        #endregion
        #region Private members
        private void InitDesktopMatlabService()
        {
            //The desktop progID only supports single-instance operation
            Type matlabType = Type.GetTypeFromProgID("Matlab.Desktop.Application");
            _matlab = (MLApp.MLApp)Activator.CreateInstance(matlabType);

            //check that we have a valid instance
            if (_matlab == default(MLApp.MLApp))
            {
                throw new ApplicationException("Matlab com object is null");
            }
        }

        private void InitBackgroundMatlabService()
        {
            _matlab = new MLApp.MLApp();
            if (_matlab == null)
            {
                throw new ApplicationException("Matlab com object is null");
            }
        }
        #endregion
    }

    public enum MatlabServiceType
    {
        Desktop, Background
    }
}
