using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;

namespace LokFu.Controllers
{
    public class AgreementController : InitController
    {
        public AgreementController()
        {
            if (!InitState)
            {
                DataObj.OutError("8080");
                return;
            }
            if (DataObj == null)
            {
                DataObj.OutError("1000");
                return;
            }
            if (!DataObj.IsReg)
            {
                DataObj.OutError("3002");
                return;
            }
        }
        public void Post()
        {
            SysSet SysSet = Entity.SysSet.FirstOrDefault();
            SysSet.Cols = "Agreement";
            DataObj.Data = SysSet.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
            //Tools.OutString(ErrInfo.Return("0000"));
        }
    }
}
