using CommunityToolkit.Mvvm.ComponentModel;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public abstract class DataTypeExpression : ObservableObject
    {
        private Condition _selectedCondition;
        public Condition SelectedCondition
        {
            get { return _selectedCondition; }
            set { SetProperty(ref _selectedCondition, value); }
        }

        private bool _isValueRequired = true;
        public bool IsValueRequired
        {
            get { return _isValueRequired; }
            set { SetProperty(ref _isValueRequired, value); }
        }

        private ValueControlType _valueControlType;
        public ValueControlType ValueControlType 
        {
            get { return _valueControlType; }
            set { SetProperty(ref _valueControlType, value); }
        }

        private void OnSelectedConditionChanged()
        {
            IsValueRequired = ConditionHelper.GetIsValueRequired(SelectedCondition);
        }

        public abstract bool CalculateResult(IPropertyMetadata propertyMetadata, object entity);
    }
}
