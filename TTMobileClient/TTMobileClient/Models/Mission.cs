using System;
using System.Collections.Generic;
using System.Text;

namespace TTMobileClient.Models
{
    class Mission
    {
        List<Models.MissionItem> missionItemList;

        public void AddMissionItem(Models.MissionItem missionItem)
        {
            missionItemList.Add(missionItem);
        }
    }
}
