using System;
using System.Collections.Generic;
using System.Text;

namespace TTMobileClient
{
    class MissionOps
    {
        private Models.Mission currentMission;

        public Models.Mission CurrentMission => currentMission;

        public void AddMissionItem( Models.MissionItem missionItem )
        {
            if (null == currentMission)
                currentMission = new Models.Mission();

            currentMission.AddMissionItem(missionItem);
        }

        public Models.Mission CreateDummyMission()
        {
            Models.Mission theMission = new Models.Mission();

            theMission.AddMissionItem(new Models.MissionItem());
            theMission.AddMissionItem(new Models.MissionItem());

            return theMission;
        }
    }
}
