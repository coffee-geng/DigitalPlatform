using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    public class AlarmVariable
    {
        public string VarNum { get; set; }

        private string _varValue;
        public string VarValue 
        {
            get { return _varValue; }
            set
            {
                string oldValue = _varValue;
                _varValue = value;
                if (!string.Equals(oldValue, value))
                {
                    try
                    {
                        Value = Convert.ChangeType(VarValue, VarType);
                    }
                    catch(Exception ex)
                    {
                        Value = null;
                    }
                }
            }
        }

        private Type _varType;
        public Type VarType 
        {
            get { return _varType; }
            set
            {
                Type oldValue = _varType;
                _varType = value;
                if (!string.Equals(oldValue, value))
                {
                    try
                    {
                        Value = Convert.ChangeType(VarValue, VarType);
                    }
                    catch (Exception ex)
                    {
                        Value = null;
                    }
                }
            }
        }

        public object Value { get; private set; }
    }
}
