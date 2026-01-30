using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.Entities;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.ViewModels
{
    public abstract class AbstractComponentViewModel : ObservableObject
    {
        #region 条件选项
        /// <summary>
        /// 递归创建条件选项（包含子条件）实体，用于数据库操作
        /// </summary>
        /// <param name="condition">条件项</param>
        /// <param name="conditionParent">条件项的父级</param>
        /// <param name="conditionEntities">创建的条件项实体都存入这个集合</param>
        protected void createConditionEntitiesBy(ICondition condition, ConditionChain conditionParent, IList<ConditionEntity> conditionEntities)
        {
            if (condition == null)
                return;
            if (conditionEntities == null)
            {
                conditionEntities = new List<ConditionEntity>();
            }

            if (condition is Coffee.DigitalPlatform.Models.Condition expCondition)
            {
                var conditionEntity = new ConditionEntity()
                {
                    CNum = condition.ConditionNum,
                    ConditionNodeTypes = ConditionNodeTypes.ConditionExpression,
                    VarNum = expCondition.Source.VarNum,
                    Operator = Enum.GetName(typeof(ConditionOperators), expCondition.Operator.Operator),
                    CNum_Parent = conditionParent?.ConditionNum ?? null,
                    Value = expCondition.TargetValue
                };
                conditionEntities.Add(conditionEntity);
            }
            else if (condition is Coffee.DigitalPlatform.Models.ConditionChain conditionGroup) // ConditionChain
            {
                var conditionGroupEntity = new ConditionEntity()
                {
                    CNum = conditionGroup.ConditionNum,
                    ConditionNodeTypes = ConditionNodeTypes.ConditionGroup,
                    Operator = Enum.GetName(typeof(ConditionChainOperators), conditionGroup.Operator),
                    CNum_Parent = conditionParent?.ConditionNum ?? null
                };
                conditionEntities.Add(conditionGroupEntity);

                if (conditionGroup.ConditionItems.Any())
                {
                    foreach (var conditionItem in conditionGroup.ConditionItems)
                    {
                        createConditionEntitiesBy(conditionItem, conditionGroup, conditionEntities);
                    }
                }
            }
        }

        /// <summary>
        /// 根据实体创建条件项（包含子条件项）
        /// </summary>
        /// <param name="conditionEntity">待创建条件项的实体</param>
        /// <param name="conditionEntities">所有条件项实体</param>
        /// <param name="variableNumDict">字典保存当前设备的变量名及点位信息</param>
        /// <returns>返回条件项</returns>
        protected ICondition createConditionByEntity(ConditionEntity conditionEntity, IEnumerable<ConditionEntity> conditionEntities, Dictionary<string, Variable> variableNumDict)
        {
            if (conditionEntity == null)
                return null;
            if (conditionEntity.ConditionNodeTypes == ConditionNodeTypes.ConditionGroup)
            {
                var conditionGroup = new ConditionChain((ConditionChainOperators)Enum.Parse(typeof(ConditionChainOperators), conditionEntity.Operator), conditionEntity.CNum);
                var childConditions = createChildConditionsByEntity(conditionGroup, conditionEntities, variableNumDict);
                //将当前条件的子条件添加给条件条件组
                if (childConditions != null)
                {
                    foreach (var childCondition in childConditions)
                    {
                        conditionGroup.ConditionItems.Add(childCondition);
                    }
                }
                return conditionGroup;
            }
            else if (conditionEntity.ConditionNodeTypes == ConditionNodeTypes.ConditionExpression)
            {
                if (variableNumDict != null && variableNumDict.TryGetValue(conditionEntity.VarNum, out Variable variable))
                {
                    var @operator = new ConditionOperator((ConditionOperators)Enum.Parse(typeof(ConditionOperators), conditionEntity.Operator));
                    var conditionExp = new Coffee.DigitalPlatform.Models.Condition(variable, conditionEntity.Value, @operator, conditionEntity.CNum);
                    return conditionExp;
                }
                else
                {
                    return null;
                }
            }
            else
                return null;
        }

        //递归调用，返回根据指定条件实体下的所有子条件项实体创建的条件项集合
        private IEnumerable<ICondition> createChildConditionsByEntity(ICondition parentCondition, IEnumerable<ConditionEntity> conditionEntities, Dictionary<string, Variable> variableNumDict)
        {
            if (conditionEntities == null || !conditionEntities.Any())
                return Enumerable.Empty<ICondition>();
            if (parentCondition == null)
                return Enumerable.Empty<ICondition>();
            //获取当前条件实体的所有子条件实体集合
            var childConditionEntities = conditionEntities.Where(c => !string.IsNullOrWhiteSpace(c.CNum_Parent) && string.Equals(c.CNum_Parent, parentCondition.ConditionNum));
            IList<ICondition> childConditions = new List<ICondition>();
            foreach (var childConditionEntity in childConditionEntities)
            {
                if (childConditionEntity.ConditionNodeTypes == ConditionNodeTypes.ConditionGroup)
                {
                    var conditionGroup = new ConditionChain((ConditionChainOperators)Enum.Parse(typeof(ConditionChainOperators), childConditionEntity.Operator), childConditionEntity.CNum);
                    //将当前子条件的后代条件添加给子条件组
                    var descendantConditions = createChildConditionsByEntity(conditionGroup, conditionEntities, variableNumDict);
                    if (descendantConditions != null)
                    {
                        foreach (var desCondition in descendantConditions)
                        {
                            conditionGroup.ConditionItems.Add(desCondition);
                        }
                    }
                }
                else if (childConditionEntity.ConditionNodeTypes == ConditionNodeTypes.ConditionExpression)
                {
                    if (variableNumDict != null && variableNumDict.TryGetValue(childConditionEntity.VarNum, out Variable variable))
                    {
                        var @operator = new ConditionOperator((ConditionOperators)Enum.Parse(typeof(ConditionOperators), childConditionEntity.Operator));
                        var conditionExp = new Coffee.DigitalPlatform.Models.Condition(variable, childConditionEntity.Value, @operator, childConditionEntity.CNum);
                        childConditions.Add(conditionExp);
                    }
                }
            }
            return childConditions;
        }
        #endregion
    }
}
