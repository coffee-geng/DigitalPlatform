
using Coffee.DigitalPlatform.Controls.Properties;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public static class ConditionExtensions
    {
        public static string Humanize(this Condition condition)
        {
            return condition switch
            {
                Condition.Contains => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_Contains)),
                Condition.DoesNotContain => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_DoesNotContain)),
                Condition.StartsWith => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_StartsWith)),
                Condition.DoesNotStartWith => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_DoesNotStartWith)),
                Condition.EndsWith => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_EndsWith)),
                Condition.DoesNotEndWith => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_DoesNotEndWith)),
                Condition.EqualTo => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_EqualTo)),
                Condition.NotEqualTo => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_NotEqualTo)),
                Condition.GreaterThan => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_GreaterThan)),
                Condition.LessThan => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_LessThan)),
                Condition.GreaterThanOrEqualTo => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_GreaterThanOrEqualTo)),
                Condition.LessThanOrEqualTo => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_LessThanOrEqualTo)),
                Condition.IsEmpty => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_IsEmpty)),
                Condition.NotIsEmpty => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_NotIsEmpty)),
                Condition.IsNull => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_IsNull)),
                Condition.NotIsNull => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_NotIsNull)),
                Condition.Matches => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_Matches)),
                Condition.DoesNotMatch => LanguageHelper.GetString(nameof(FilterBuilderResource.FilterBuilder_DoesNotMatch)),
                _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, null)
            };
        }
    }
}
