using Coffee.DigitalPlatform.IDataAccess;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class MonitorViewModel : ObservableObject
    {
        MainViewModel _mainViewModel;
        ILocalDataAccess _localDataAccess;

        public MonitorViewModel(MainViewModel mainViewModel, ILocalDataAccess localDataAccess)
        {
            _mainViewModel = mainViewModel;
            _localDataAccess = localDataAccess;

            initDataForMonitor();
        }

        public Variable Temperature { get; set; }
        public Variable Humidity { get; set; }
        public Variable PM { get; set; }
        public Variable Pressure { get; set; }
        public Variable FlowRate { get; set; }

        public List<RankingItem> RankingList { get; set; }

        public List<MonitorWarnning> WarnningList { get; set; }

        void initDataForMonitor()
        {
            Random random = new Random();
            #region 用气排行
            string[] quality = new string[] { "车间-1", "车间-2", "车间-3", "车间-4",
                "车间-5" };
            RankingList = new List<RankingItem>();
            foreach (var q in quality)
            {
                RankingList.Add(new RankingItem()
                {
                    Header = q,
                    PlanValue = random.Next(100, 200),
                    FinishedValue = random.Next(10, 150),
                    TotalValue = 240
                });
            }
            #endregion

            #region 设备提醒
            WarnningList = new List<MonitorWarnning>()
                {
                  new MonitorWarnning{Message= "朝夕PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnning{Message= "朝夕PLT-01：故障",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnning{Message= "朝夕PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnning{Message= "朝夕PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnning{Message= "朝夕PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnning{Message= "朝夕PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnning{Message= "朝夕PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                };
            #endregion
        }
    }
}
